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
using UnityEngine;
using UnityEngine.Profiling;
using pxr;

namespace USD.NET.Unity {

  /// <summary>
  /// A collection of methods for building the USD scene hierarchy in Unity.
  /// </summary>
  public static class HierarchyBuilder {
    static readonly SdfPath kAbsoluteRootPath = SdfPath.AbsoluteRootPath();

    public static void BuildObjectLists(Scene scene,
                                        GameObject unityRoot,
                                        SdfPath usdRoot,
                                        PrimMap map,
                                        SceneImportOptions options) {
      // PERFORMANCE: Internally, these collections are converted to SdfPathVectors,
      // so some time and garbage churn could be saved by using that type instead.
      if (options.importCameras) {
        map.Cameras = scene.Find<CameraSample>(usdRoot);
        ProcessPaths(map.Cameras, scene, unityRoot, usdRoot, map, options);
      }
      if (options.importMeshes) {
        map.Meshes = scene.Find<MeshSample>(usdRoot);
        ProcessPaths(map.Meshes, scene, unityRoot, usdRoot, map, options);
      }
      if (options.importMeshes) {
        map.Cubes = scene.Find<CubeSample>(usdRoot);
        ProcessPaths(map.Cubes, scene, unityRoot, usdRoot, map, options);
      }
      if (options.importSkinning) {
        map.SkelRoots = scene.Find<SkelRootSample>(usdRoot);
        ProcessPaths(map.SkelRoots, scene, unityRoot, usdRoot, map, options);
      }
      if (options.importTransforms) {
        map.Xforms = scene.Find<XformSample>(usdRoot);
        ProcessPaths(map.Xforms, scene, unityRoot, usdRoot, map, options);
      }
    }

    /// <summary>
    /// Map all UsdPrims and build Unity GameObjects, reconstructing the parent relationship.
    /// </summary>
    /// <remarks>
    /// When forceRebuild is true, game objects will be destroyed and recreated. If buildHierarchy
    /// is false, the primMap will be populated, but missing game objects will not be created.
    /// </remarks>
    static public PrimMap BuildGameObjects(Scene scene,
                                            GameObject unityRoot,
                                            SdfPath usdRoot,
                                            IEnumerable<SdfPath> paths,
                                            PrimMap map,
                                            SceneImportOptions options) {
      map[usdRoot] = unityRoot;

      BuildObjectLists(scene, unityRoot, usdRoot, map, options);

      // TODO: Should recurse to discover deeply nested instancing.
      // TODO: Generates garbage for every prim, but we expect few masters.
      if (options.importPointInstances || options.importSceneInstances) {
        Profiler.BeginSample("Build Temp Masters");
        foreach (var masterRootPrim in scene.Stage.GetMasters()) {
          var goMaster = new GameObject(masterRootPrim.GetPath().GetName());

          goMaster.hideFlags = HideFlags.HideInHierarchy;
          goMaster.SetActive(false);
          if (unityRoot != null) {
            goMaster.transform.SetParent(unityRoot.transform, worldPositionStays: false);
          }
          map.AddMasterRoot(masterRootPrim.GetPath(), goMaster);
          try {
            AddModelRoot(goMaster, masterRootPrim);
            AddVariantSet(goMaster, masterRootPrim);
          } catch (Exception ex) {
            Debug.LogException(new Exception("Error processing " + masterRootPrim.GetPath(), ex));
          }

          foreach (var usdPrim in masterRootPrim.GetDescendants()) {
            var goPrim = new GameObject(usdPrim.GetName());
            try {
              AddModelRoot(goPrim, usdPrim);
              AddVariantSet(goPrim, usdPrim);
            } catch (Exception ex) {
              Debug.LogException(new Exception("Error processing " + usdPrim.GetPath(), ex));
            }

            if (usdPrim.IsInstance()) {
              map.AddInstanceRoot(usdPrim.GetPath(), goPrim, usdPrim.GetMaster().GetPath());
            }

            var parentPath = usdPrim.GetPath().GetParentPath();
            Transform parentXf = null;
            if (parentPath == masterRootPrim.GetPath()) {
              parentXf = goMaster.transform;
            } else {
              parentXf = map[parentPath].transform;
            }

            map[usdPrim.GetPath()] = goPrim;
            goPrim.transform.SetParent(parentXf, worldPositionStays: false);
          }
        }
        Profiler.EndSample();
      }

      if (options.importSkinning) {
        Profiler.BeginSample("Expand Skeletons");
        foreach (var path in scene.Find<SkelRootSample>()) {
          try {
            var prim = scene.GetPrimAtPath(path);
            ExpandSkeleton(map[path], prim, map, scene);
          } catch (Exception ex) {
            Debug.LogException(new Exception("Error expanding skeleton at " + path, ex));
          }
        }
        Profiler.EndSample();
      }

      return map;
    }

    static void CreateAncestors(SdfPath path,
                                PrimMap map,
                                GameObject unityRoot,
                                SdfPath usdRoot,
                                SceneImportOptions options,
                                out GameObject parentGo) {
      var parentPath = path.GetParentPath();
      if (map.TryGetValue(parentPath, out parentGo) && parentGo) {
        return;
      }

      // Base case.
      if (parentPath == usdRoot) {
        map[parentPath] = unityRoot;
        return;
      }

      if (parentPath == kAbsoluteRootPath) {
        // Something went wrong.
        Debug.LogException(new Exception(
            "Error: unexpected path </> creating ancestors for <" + usdRoot.ToString() + ">"));
      }

      // Recursive case.
      // First, get the grandparent (parent's parent).
      GameObject grandparentGo;
      CreateAncestors(parentPath, map, unityRoot, usdRoot, options, out grandparentGo);

      // Then find/create the current parent.
      parentGo = FindOrCreateGameObject(grandparentGo.transform,
                                        parentPath,
                                        unityRoot.transform,
                                        options);
      map[parentPath] = parentGo;
    }

    static void ProcessPaths(SdfPath[] paths,
                             Scene scene,
                             GameObject unityRoot,
                             SdfPath usdRoot,
                             PrimMap map,
                             SceneImportOptions options) {
      Profiler.BeginSample("Process all paths");
      foreach (SdfPath path in paths) {
        var prim = scene.GetPrimAtPath(path);
        GameObject parentGo = null;
        CreateAncestors(path, map, unityRoot, usdRoot, options, out parentGo);

        if (!parentGo) {
          Debug.LogWarning("Parent path not found for child: " + path.ToString());
          continue;
        }

        var parent = parentGo ? parentGo.transform : null;
        GameObject go;
        if (!map.TryGetValue(path, out go)) {
          go = FindOrCreateGameObject(parent, path, unityRoot.transform, options);
        }

        if (go) {
          if (options.importSceneInstances) {
            Profiler.BeginSample("Add Scene Instance Root");
            if (prim.IsInstance()) {
              map.AddInstanceRoot(prim.GetPath(), go, prim.GetMaster().GetPath());
            }
            Profiler.EndSample();
          }

          Profiler.BeginSample("Add to Map");
          map[scene.GetSdfPath(path)] = go;
          Profiler.EndSample();
        }

        if (!options.importHierarchy) {
          continue;
        }

        try {
          Profiler.BeginSample("Add Model Root");
          AddModelRoot(go, prim);
          Profiler.EndSample();

          Profiler.BeginSample("Add Variant Set");
          AddVariantSet(go, prim);
          Profiler.EndSample();
        } catch (Exception ex) {
          Debug.LogException(new Exception("Error processing " + prim.GetPath(), ex));
        }
      }
      Profiler.EndSample();
    }

    static void ExpandSkeleton(GameObject go, UsdPrim prim, PrimMap map, Scene scene) {
      if (!prim) { return; }

      var skelRoot = new UsdSkelRoot(prim);
      if (!skelRoot) { return; }

      var skelRel = prim.GetRelationship(UsdSkelTokens.skelSkeleton);
      if (!skelRel) { return; }

      SdfPathVector targets = skelRel.GetForwardedTargets();
      if (targets == null || targets.Count == 0) { return; }

      var skelPrim = prim.GetStage().GetPrimAtPath(targets[0]);
      if (!skelPrim) { return; }

      var skel = new UsdSkelSkeleton(skelPrim);
      if (!skel) { return; }

      var jointsAttr = skel.GetJointsAttr();
      if (!jointsAttr) { return; }

      var vtJoints = jointsAttr.Get();
      if (vtJoints.IsEmpty()) { return; }
      var vtStrings = UsdCs.VtValueToVtTokenArray(vtJoints);
      var joints = UnityTypeConverter.FromVtArray(vtStrings);

      var skelPath = skelPrim.GetPath();
      foreach (var joint in joints) {
        var path = skelPath.AppendPath(scene.GetSdfPath(joint));
        GameObject parentGo = null;
        if (!map.TryGetValue(path.GetParentPath(), out parentGo)) {
          Debug.LogWarning("Parent joint path not found: " + path.GetParentPath().ToString()
                         + " for prim: " + path.ToString());
          continue;
        }

        Transform child = parentGo.transform.Find(path.GetName());
        if (!child) {
          child = new GameObject(path.GetName()).transform;
          child.SetParent(parentGo.transform, worldPositionStays: false);
        }

        map[path] = child.gameObject;
      }
    }

    /// <summary>
    /// Exposes model root and asset metadata. The game object is primarily a tag which is useful
    /// for smart selection of models instead of geometry.
    /// </summary>
    static void AddModelRoot(GameObject go, UsdPrim prim) {
      if (!prim) {
        return;
      }

      var modelApi = new UsdModelAPI(prim);
      if (!modelApi) { return; }

      var kindTok = new TfToken();
      if (!modelApi.GetKind(kindTok)) {
        return;
      }

      if (KindRegistry.IsA(kindTok, KindTokens.assembly)) {
        var asm = go.GetComponent<UsdAssemblyRoot>();
        if (!asm) {
          go.AddComponent<UsdAssemblyRoot>();
        }
      } else if (modelApi.IsModel() && !modelApi.IsGroup()) {
        var mdl = go.GetComponent<UsdModelRoot>();
        if (!mdl) {
          mdl = go.AddComponent<UsdModelRoot>();
        }
        var info = new VtDictionary();
        if (modelApi.GetAssetInfo(info)) {
          var valName = info.GetValueAtPath("name");
          var valVersion = info.GetValueAtPath("version");
          var valIdentifier = info.GetValueAtPath("identifier");
          if (valIdentifier != null && !valIdentifier.IsEmpty()) {
            mdl.m_modelAssetPath = UsdCs.VtValueToSdfAssetPath(valIdentifier).GetAssetPath().ToString();
          }
          if (valName != null && !valName.IsEmpty()) {
            mdl.m_modelName = UsdCs.VtValueTostring(valName);
          }
          if (valVersion != null && !valVersion.IsEmpty()) {
            mdl.m_modelVersion = UsdCs.VtValueTostring(valVersion);
          }
        }
      } else {
        // If these tags were added previously, remove them.
        var mdl = go.GetComponent<UsdModelRoot>();
        if (mdl) {
          Component.DestroyImmediate(mdl);
        }
        var asm = go.GetComponent<UsdAssemblyRoot>();
        if (asm) {
          Component.DestroyImmediate(asm);
        }
      }
    }

    /// <summary>
    /// If there is a variant set authored on this prim, expose it so the user can change the
    /// variant selection.
    /// </summary>
    static void AddVariantSet(GameObject go, UsdPrim prim) {
      var sets = prim.GetVariantSets();
      var setNames = sets.GetNames();
      var vs = go.GetComponent<UsdVariantSet>();

      if (setNames.Count == 0) {
        if (vs) {
          Component.DestroyImmediate(vs);
        }
        return;
      }

      if (!vs) {
        vs = go.AddComponent<UsdVariantSet>();
      }

      vs.SyncVariants(prim, sets);
    }

    /// <summary>
    /// Checks for a child named "name" under the given parent, if it exists it is returned,
    /// else a new child is created with this name.
    /// </summary>
    static GameObject FindOrCreateGameObject(Transform parent,
                                             SdfPath path,
                                             Transform unityRoot,
                                             SceneImportOptions options) {
      Transform root = null;
      GameObject go = null;
      string name = path.GetName();

      if (parent == null) {
        go = GameObject.Find(name);
        root = go ? go.transform : null;
      } else {
        root = parent.Find(name);
        go = root ? root.gameObject : null;
      }

      if (parent != unityRoot && options.forceRebuild && go) {
        GameObject.DestroyImmediate(go);
        go = null;
      }
      
      if (!go) {
        // TODO: this should really not construct a game object if ImportHierarchy is false,
        // but it requires all downstream code be driven by the primMap instead of finding prims
        // via the usd scene. In addition, this requies the prim map to store lists of prims by
        // type, e.g. cameras, meshes, cubes, etc.
        go = new GameObject(name);
        var ua = go.AddComponent<UsdPrimSource>();
        ua.m_usdPrimPath = path.ToString();
      }

      if (parent != null) {
        go.transform.SetParent(parent, worldPositionStays: false);
      }
      return go;
    }

  }
}
