// Copyright 2018 Jeremy Cowles. All rights reserved.
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace USD.NET.Unity {
  // The export function allows for dispatch to different export functions without knowing what
  // type of data they export (e.g. mesh vs. transform).
  public delegate void ExportFunction(ObjectContext objContext, ExportContext exportContext);

  delegate void ObjectProcessor(GameObject go,
                                ExportContext context);

  public struct ObjectContext {
    public GameObject gameObject;
    public string path;
    public SampleBase sample;
    public object additionalData;
  }

  public enum ActiveExportPolicy {
    // Inactive GameObjects in Unity become invisible objects in USD, which is actually the
    // closest semantic mapping.
    ExportAsVisibility,

    // Inactive GameObjects in Unity become deactivated objects in USD. Caution, this is not truly
    // an equivalent state because deactivated objects in USD are fully unloaded and their
    // subtree will not exist after being deactivated.
    ExportAsActive,

    // Inactive GameObjects will not be exported.
    DoNotExport,

    // Inactive GameObjects will be exported without special handling.
    Ignore,
  }

  public class ExportContext {
    public Scene scene;
    public Transform exportRoot;
    public bool exportMaterials = true;
    public BasisTransformation basisTransform = BasisTransformation.FastWithNegativeScale;
    public ActiveExportPolicy activePolicy = ActiveExportPolicy.ExportAsVisibility;
    public Dictionary<GameObject, ExportPlan> plans = new Dictionary<GameObject, ExportPlan>();
    public Dictionary<Material, string> matMap = new Dictionary<Material, string>();
    public Dictionary<Transform, Transform[]> skelMap = new Dictionary<Transform, Transform[]>();
    public Dictionary<Transform, Matrix4x4> bones = new Dictionary<Transform, Matrix4x4>();

    // Sample object instances, shared across multiple export methods.
    public Dictionary<Type, SampleBase> samples = new Dictionary<Type, SampleBase>();
  }

  public class Exporter {
    // The USD path at which the Unity data will be written.
    public string path;

    // The sample type to be used when exporting.
    public SampleBase sample;

    // The export function which implements the logic to populate the sample.
    public ExportFunction exportFunc;

    // Additional arguments required for export.
    public object data;
  }

  // An export plan will be created for each path in the scene. Each ExportPlan will use one of
  // the fixed export functions. For example, when setting up export for a mesh, an ExportPlan
  // will be created for that path in the scenegraph and the ExportFunction will the one which is
  // capable of exporting a mesh.
  public class ExportPlan {
    // The functions to run when exporting this object.
    public List<Exporter> exporters = new List<Exporter>();
  }

  /// <summary>
  /// The scene exporter can be used to export data to USD.
  /// </summary>
  public static class SceneExporter {


    // ------------------------------------------------------------------------------------------ //
    // Main Export Logic.
    // ------------------------------------------------------------------------------------------ //

    public static void Export(GameObject root,
                              Scene scene,
                              BasisTransformation basisTransform,
                              bool exportUnvarying) {
      var context = new ExportContext();
      context.scene = scene;
      context.basisTransform = basisTransform;
      context.exportRoot = root.transform.parent;
      SyncExportContext(root, context);

      // Since this is a one-shot convenience function, we will automatically split the export
      // into varying and unvarying data, unless the user explicitly requested unvarying.
      if (exportUnvarying && scene.Time != null) {
        double? oldTime = scene.Time;
        scene.Time = null;
        Export(root, context);
        scene.Time = oldTime;
      }

      // Export data for the requested time.
      context.exportMaterials = false;
      Export(root, context);
    }
    public static void Export(GameObject root,
                          ExportContext context) {
      // Remove parent transform effects while exporting.
      // This must be restored before returning from this function.
      var parent = root.transform.parent;
      root.transform.SetParent(null, worldPositionStays: false);

      // Also zero out and restore local rotations on the root.
      var localPos = root.transform.localPosition;
      var localRot = root.transform.localRotation;
      var localScale = root.transform.localScale;
      root.transform.localPosition = Vector3.zero;
      root.transform.localRotation = Quaternion.identity;
      root.transform.localScale = Vector3.one;

      try {
        ExportImpl(root, context);
      } finally {
        root.transform.localPosition = localPos;
        root.transform.localRotation = localRot;
        root.transform.localScale = localScale;
        root.transform.SetParent(parent);
      }
    }

    private static void ExportImpl(GameObject root,
                              ExportContext context) {
      var scene = context.scene;
      bool skipInactive = context.activePolicy == ActiveExportPolicy.DoNotExport;

      if (context.exportMaterials) {
        // TODO: should account for skipped objects and also skip their materials.
        foreach (var kvp in context.matMap) {
          Material mat = kvp.Key;
          string usdPath = kvp.Value;
          if (!mat || usdPath == null) {
            continue;
          }
          MaterialExporter.ExportMaterial(scene, kvp.Key, kvp.Value);
        }
      }

      foreach (var kvp in context.plans) {
        GameObject go = kvp.Key;
        ExportPlan exportPlan = kvp.Value;

        if (!go || exportPlan == null) {
          continue;
        }

        if (skipInactive && go.activeInHierarchy == false) {
          continue;
        }

        foreach (Exporter exporter in exportPlan.exporters) {
          string path = exporter.path;

          SampleBase sample = exporter.sample;
          var objCtx = new ObjectContext {
            gameObject = go,
            path = path,
            sample = sample,
            additionalData = exporter.data
          };

          exporter.exportFunc(objCtx, context);

          if (!go.gameObject.activeSelf) {
            switch (context.activePolicy) {
              case ActiveExportPolicy.Ignore:
                // Nothing to see here.
                break;

              case ActiveExportPolicy.ExportAsVisibility:
                // Make the prim invisible.
                var im = new pxr.UsdGeomImageable(scene.GetPrimAtPath(path));
                if (im) {
                  im.CreateVisibilityAttr().Set(pxr.UsdGeomTokens.invisible);
                }
                break;

              case ActiveExportPolicy.ExportAsActive:
                // TODO: this may actually cause errors because exported prims will not exist in
                // the USD scene graph. Right now, that's too much responsibility on the caller,
                // because the error messages will be mysterious.

                // Make the prim inactive.
                scene.GetPrimAtPath(path).SetActive(false);
                break;
            }
          }

        } // foreach exporter
      } // foreach plan
    }

    // ------------------------------------------------------------------------------------------ //
    // Init Hierarchy.
    // ------------------------------------------------------------------------------------------ //

    static void Traverse(GameObject obj,
                         ObjectProcessor processor,
                         ExportContext context) {
      processor(obj, context);
      foreach (Transform child in obj.transform) {
        Traverse(child.gameObject, processor, context);
      }
    }

    static void AccumNestedBones(Transform curXf,
                                 List<Transform> children,
                                 ExportContext ctx) {
      if (ctx.bones.ContainsKey(curXf)) {
        children.Add(curXf);
      }
      foreach (Transform child in curXf.transform) {
        AccumNestedBones(child, children, ctx);
      }
    }

    static T CreateSample<T>(ExportContext context) where T : SampleBase, new() {
      return new T();
      /*
      SampleBase sb;
      if (context.samples.TryGetValue(typeof(T), out sb)) {
        return (T)sb;
      }

      sb = (new T());
      context.samples[typeof(T)] = sb;
      return (T)sb;
      */
    }

    public static void SyncExportContext(GameObject exportRoot,
                              ExportContext context) {

      Traverse(exportRoot, InitExportableObjects, context);

      Transform expRoot = context.exportRoot;
      var foundAnimators = new List<Transform>();
      foreach (Transform rootBone in context.skelMap.Keys.ToList()) {
        foreach (var xf in foundAnimators) {
          if (rootBone.IsChildOf(xf)) {
            continue;
          }
        }
        var parentXf = rootBone;
        while (parentXf != null) {

          // If there is an animator & avitar, assume this is the root of the rig.
          // This feels very ad hoc, it would be nice to not use a heuristic.
          var anim = parentXf.GetComponent<Animator>();
          if (anim != null && anim.avatar != null) {

            SkelRootSample rootSample = CreateSample<SkelRootSample>(context);
            rootSample.skeleton = UnityTypeConverter.GetPath(parentXf, expRoot) + "/_skeleton";
            rootSample.animationSource = UnityTypeConverter.GetPath(parentXf, expRoot) + "/_anim";

            CreateExportPlan(
                parentXf.gameObject,
                rootSample,
                SkeletonExporter.ExportSkelRoot,
                context,
                insertFirst: true);

            CreateExportPlan(
                parentXf.gameObject,
                CreateSample<SkeletonSample>(context),
                SkeletonExporter.ExportSkeleton,
                context,
                insertFirst: true,
                pathSuffix: "/_skeleton");

            CreateExportPlan(
                parentXf.gameObject,
                CreateSample<SkelAnimationSample>(context),
                SkeletonExporter.ExportSkelAnimation,
                context,
                insertFirst: true,
                pathSuffix: "/_anim");

            // Exporting animation is only possible while in-editor (in 2018 and earlier).
#if UNITY_EDITOR
#if false   // Currently disabled, future work.
            if (anim.layerCount > 0) {
              for (int l = 0; l < anim.layerCount; l++) {
                int clipCount = anim.GetCurrentAnimatorClipInfoCount(l);
                var clipInfos = anim.GetCurrentAnimatorClipInfo(l);
                foreach (var clipInfo in clipInfos) {
                  var bindings = UnityEditor.AnimationUtility.GetCurveBindings(clipInfo.clip);
                  // Properties are expressed as individual values, for transforms this is:
                  //   m_LocalPosition.x,y,z
                  //   m_LocalScale.x,y,z
                  //   m_LocalRotation.x,y,z,w
                  // Which means they must be reaggregated into matrices.
                  foreach (var binding in bindings) {
                    if (binding.type != typeof(Transform)) {
                      continue;
                    }
                    Debug.Log(binding.path + "." + binding.propertyName);
                    var knot = UnityEditor.AnimationUtility.GetEditorCurve(clipInfo.clip, binding);
                  }
                }
              }
            }
#endif // disabled.
#endif // Editor only.

            foundAnimators.Add(anim.transform);

            var children = new List<Transform>();
            var meshes = new List<GameObject>();
            AccumNestedBones(parentXf, children, context);
            context.skelMap[parentXf] = children.ToArray();
            context.skelMap.Remove(rootBone);
            break;
          }
          parentXf = parentXf.parent;
        }
      }
    }

    static void InitExportableObjects(GameObject go,
                                      ExportContext context) {
      var smr = go.GetComponent<SkinnedMeshRenderer>();
      var mr = go.GetComponent<MeshRenderer>();
      var mf = go.GetComponent<MeshFilter>();
      var cam = go.GetComponent<Camera>();
      var anim = go.GetComponent<Animator>();
      Transform expRoot = context.exportRoot;

      if (anim != null && anim.avatar != null && anim.avatar.isValid) {
        // Assume any animator that has an avitar is a skeleton root.
      }

      if (smr != null) {
        foreach (var mat in smr.sharedMaterials) {
          if (!context.matMap.ContainsKey(mat)) {
            string usdPath = "/World/Materials/" + pxr.UsdCs.TfMakeValidIdentifier(mat.name + "_" + mat.GetInstanceID().ToString());
            context.matMap.Add(mat, usdPath);
          }
        }
        CreateExportPlan(go, CreateSample<MeshSample>(context), MeshExporter.ExportSkinnedMesh, context);
        if (smr.rootBone == null) {
          Debug.LogWarning("No root bone at: " + UnityTypeConverter.GetPath(go.transform, expRoot));
        } else if (smr.bones == null || smr.bones.Length == 0) {
          Debug.LogWarning("No bones at: " + UnityTypeConverter.GetPath(go.transform, expRoot));
        } else {
          MergeBones(smr.rootBone, smr.bones, smr.sharedMesh.bindposes, context);
        }
      } else if (mf != null && mr != null) {
        foreach (var mat in mr.sharedMaterials) {
          if (mat == null) {
            continue;
          }
          if (!context.matMap.ContainsKey(mat)) {
            string usdPath = "/World/Materials/" + pxr.UsdCs.TfMakeValidIdentifier(mat.name + "_" + mat.GetInstanceID().ToString());
            context.matMap.Add(mat, usdPath);
          }
        }
        CreateExportPlan(go, CreateSample<MeshSample>(context), MeshExporter.ExportMesh, context);
      } else if (cam) {
        CreateExportPlan(go, CreateSample<CameraSample>(context), CameraExporter.ExportCamera, context);
      }
    }

    static void MergeBones(Transform rootBone, Transform[] bones, Matrix4x4[] bindPoses, ExportContext context) {
      if (!context.bones.ContainsKey(rootBone) && !context.skelMap.ContainsKey(rootBone)) {
        context.skelMap.Add(rootBone, bones);
      }
      for (int i = 0; i < bones.Length; i++) {
        Transform bone = bones[i];
        context.bones[bone] = bindPoses[i];
        if (bone == rootBone) {
          continue;
        }
        if (context.skelMap.ContainsKey(bone)) {
          context.skelMap.Remove(bone);
        }
      }
    }

    static void CreateExportPlan(GameObject go,
                                 SampleBase sample,
                                 ExportFunction exportFunc,
                                 ExportContext context,
                                 string pathSuffix = null,
                                 object data = null,
                                 bool insertFirst = true) {
      // This is an exportable object.
      Transform expRoot = context.exportRoot;
      string path = Unity.UnityTypeConverter.GetPath(go.transform, expRoot);
      if (!string.IsNullOrEmpty(pathSuffix)) {
        path += pathSuffix;
      }
      if (!context.plans.ContainsKey(go)) {
        context.plans.Add(go, new ExportPlan());
      }

      var exp = new Exporter { exportFunc = exportFunc, sample = sample, path = path, data = data };
      if (insertFirst) {
        context.plans[go].exporters.Insert(0, exp);
      } else {
        context.plans[go].exporters.Add(exp);
      }

      // Include the parent xform hierarchy.
      // Note that the parent hierarchy is memoised, so despite looking expensive, the time
      // complexity is linear.
      Transform xf = go.transform.parent;
      if (xf != context.exportRoot && !context.plans.ContainsKey(xf.gameObject)) {
        // Since all GameObjects have a Transform, export all un-exported parents as transform.
        CreateExportPlan(xf.gameObject, CreateSample<XformSample>(context), XformExporter.ExportXform, context);
      }
    }

    static Matrix4x4 ComputeWorldXf(Transform curBone, ExportContext context) {
      if (!context.bones.ContainsKey(curBone)) {
        return curBone.parent.localToWorldMatrix;
      }
      return context.bones[curBone] * ComputeWorldXf(curBone.parent, context);
    }

  }
}