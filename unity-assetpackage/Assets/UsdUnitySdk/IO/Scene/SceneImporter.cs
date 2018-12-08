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

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Profiling;

#if UNITY_EDITOR
using UnityEditor;
using System.IO;
#endif

namespace USD.NET.Unity {

  [UsdSchema("Mesh")]
  class LimitedMeshSample : XformableSample {
    public Vector3[] points;
  }

  /// <summary>
  /// Root entry point for importing an entire USD scene.
  /// </summary>
  public static class SceneImporter {

    public delegate void ImportNotice(PrimMap primMap,
                                      Scene usdScene,
                                      pxr.SdfPath usdPrimRoot,
                                      GameObject unityRoot,
                                      SceneImportOptions importOptions);

    /// <summary>
    /// Executes after the PrimMap has been constructed and before import.
    /// </summary>
    public static event ImportNotice AfterBuildPrimMap;

    /// <summary>
    /// Executes after the entire import process (BuildScene) completes. Note that this event may
    /// fire after several frames, when the coroutine overload of BuildScene is used.
    /// </summary>
    public static event ImportNotice AfterImport;

#if UNITY_EDITOR
    /// <summary>
    /// Custom importer. This works almost exactly as the ScriptedImporter, but does not require
    /// the new API.
    /// </summary>
    public static void SaveAsSinglePrefab(GameObject rootObject,
                                          string prefabPath,
                                          SceneImportOptions importOptions) {
      Directory.CreateDirectory(Path.GetDirectoryName(prefabPath));

      GameObject oldPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
      GameObject prefab = null;

      if (oldPrefab == null) {
        // Create the prefab. At this point, the meshes do not yet exist and will be
        // dangling references
        prefab = PrefabUtility.CreatePrefab(prefabPath, rootObject);
        HashSet<Mesh> meshes;
        HashSet<Material> materials;
        AddObjectsToAsset(rootObject, prefab, importOptions, out meshes, out materials);

        foreach (var mesh in meshes) {
          AssetDatabase.AddObjectToAsset(mesh, prefab);
        }

        foreach (var mat in materials) {
          AssetDatabase.AddObjectToAsset(mat, prefab);
        }

        // Fix the dangling references.
        prefab = PrefabUtility.ReplacePrefab(rootObject, prefab);
      } else {
        HashSet<Mesh> meshes;
        HashSet<Material> materials;
        AddObjectsToAsset(rootObject, oldPrefab, importOptions, out meshes, out materials);

        // ReplacePrefab only removes the GameObjects from the asset.
        // Clear out all non-prefab junk (ie, meshes), because otherwise it piles up.
        // The main difference between LoadAllAssetRepresentations and LoadAllAssets
        // is that the former returns MonoBehaviours and the latter does not.
        foreach (var obj in AssetDatabase.LoadAllAssetRepresentationsAtPath(prefabPath)) {
          if (obj is GameObject) {
            continue;
          }
          if (obj is Mesh && meshes.Contains((Mesh)obj)) {
            meshes.Remove((Mesh)obj);
            continue;
          }
          if (obj is Material && materials.Contains((Material)obj)) {
            materials.Remove((Material)obj);
            continue;
          }
          Object.DestroyImmediate(obj, allowDestroyingAssets: true);
        }

        foreach (var mesh in meshes) {
          AssetDatabase.AddObjectToAsset(mesh, oldPrefab);
        }

        foreach (var mat in materials) {
          AssetDatabase.AddObjectToAsset(mat, oldPrefab);
        }

        if (oldPrefab != rootObject) {
          prefab = PrefabUtility.ReplacePrefab(
              rootObject, oldPrefab, ReplacePrefabOptions.ReplaceNameBased);
        }
      }

      AssetDatabase.ImportAsset(prefabPath, ImportAssetOptions.ForceUpdate);
      AssetDatabase.SaveAssets();
    }

    static void AddObjectsToAsset(GameObject rootObject,
                                  Object asset,
                                  SceneImportOptions importOptions,
                                  out HashSet<Mesh> usedMeshes,
                                  out HashSet<Material> usedMaterials) {
      var meshes = new HashSet<Mesh>();
      var materials = new HashSet<Material>();

      materials.Add(importOptions.materialMap.FallbackMasterMaterial);
      materials.Add(importOptions.materialMap.MetallicWorkflowMaterial);
      materials.Add(importOptions.materialMap.SpecularWorkflowMaterial);

      var tempMat = importOptions.materialMap.FallbackMasterMaterial;
      if (tempMat != null && AssetDatabase.GetAssetPath(tempMat) == "") {
        materials.Add(tempMat);
      }

      tempMat = importOptions.materialMap.MetallicWorkflowMaterial;
      if (tempMat != null && AssetDatabase.GetAssetPath(tempMat) == "") {
        materials.Add(tempMat);
      }

      tempMat = importOptions.materialMap.SpecularWorkflowMaterial;
      if (tempMat != null && AssetDatabase.GetAssetPath(tempMat) == "") {
        materials.Add(tempMat);
      }

      foreach (var mf in rootObject.GetComponentsInChildren<MeshFilter>()) {
        if (mf.sharedMesh != null && meshes.Add(mf.sharedMesh)) {
        }
      }

      foreach (var mf in rootObject.GetComponentsInChildren<MeshRenderer>()) {
        foreach (var mat in mf.sharedMaterials) {
          if (mat == null || !materials.Add(mat)) {
            continue;
          }
        }
      }

      foreach (var mf in rootObject.GetComponentsInChildren<SkinnedMeshRenderer>()) {
        if (mf.sharedMesh != null && meshes.Add(mf.sharedMesh)) {
        }
        foreach (var mat in mf.sharedMaterials) {
          if (mat == null || !materials.Add(mat)) {
            continue;
          }
        }
      }

      usedMeshes = meshes;
      usedMaterials = materials;
    }
#endif

    /// <summary>
    /// Rebuilds the USD scene as Unity GameObjects, maintaining a mapping from USD to Unity.
    /// </summary>
    public static PrimMap BuildScene(Scene scene,
                                     GameObject root,
                                     pxr.SdfPath usdPrimRoot,
                                     bool composingSubtree,
                                     SceneImportOptions importOptions) {
      try {
        Profiler.BeginSample("USD: Build Scene");
        var primMap = new PrimMap();
        var builder = BuildScene_(scene,
                                  root,
                                  usdPrimRoot,
                                  composingSubtree,
                                  importOptions,
                                  primMap,
                                  0);
        while (builder.MoveNext()) { }
        return primMap;
      } finally {
        Profiler.EndSample();
      }
    }

    public static IEnumerator BuildScene(Scene scene,
                                     GameObject root,
                                     pxr.SdfPath usdPrimRoot,
                                     bool composingSubtree,
                                     SceneImportOptions importOptions,
                                     PrimMap primMap,
                                     float targetFrameMilliseconds) {
      return BuildScene_(scene,
                         root,
                         usdPrimRoot,
                         composingSubtree,
                         importOptions,
                         primMap,
                         targetFrameMilliseconds);
    }

    private static bool ShouldYield(float targetTime, System.Diagnostics.Stopwatch timer) {
      return timer.ElapsedMilliseconds > targetTime;
    }

    private static void ResetTimer(System.Diagnostics.Stopwatch timer) {
      timer.Stop();
      timer.Reset();
      timer.Start();
    }

    private static IEnumerator BuildScene_(Scene scene,
                                           GameObject root,
                                           pxr.SdfPath usdPrimRoot,
                                           bool composingSubtree,
                                           SceneImportOptions importOptions,
                                           PrimMap primMap,
                                           float targetFrameMilliseconds) {
      var timer = new System.Diagnostics.Stopwatch();

      // Setting an arbitrary fudge factor of 20% is very non-scientific, however it's better than
      // nothing. The correct way to hit a deadline is to predict how long each iteration actually
      // takes and then return early if the estimated time is over budget.
      float targetTime = targetFrameMilliseconds * .8f;

      timer.Start();

      // Reconstruct the USD hierarchy as Unity GameObjects.
      // A PrimMap is returned for tracking the USD <-> Unity mapping.
      Profiler.BeginSample("USD: Build Hierarchy");
      primMap.Clear();
      HierarchyBuilder.BuildGameObjects(scene,
                                        root,
                                        usdPrimRoot,
                                        scene.Find(usdPrimRoot.ToString(), "UsdSchemaBase"),
                                        primMap,
                                          importOptions);
      Profiler.EndSample();

      if (ShouldYield(targetTime, timer)) { yield return null; ResetTimer(timer); }

      Profiler.BeginSample("USD: AfterBuildPrimMap");
      if (AfterBuildPrimMap != null) {
        AfterBuildPrimMap(primMap, scene, usdPrimRoot, root, importOptions);
      }
      Profiler.EndSample();

      if (ShouldYield(targetTime, timer)) { yield return null; ResetTimer(timer); }

      //
      // Pre-process UsdSkelRoots.
      //
      var skelRoots = new List<pxr.UsdSkelRoot>();
      var skelCache = new pxr.UsdSkelCache();

      if (importOptions.importSkinning) {
        Profiler.BeginSample("USD: Process UsdSkelRoots");
        foreach (var path in scene.Find<SkelRootSample>(usdPrimRoot)) {
          try {
            var skelRootPrim = scene.GetPrimAtPath(path);
            if (!skelRootPrim) {
              Debug.LogWarning("SkelRoot prim not found: " + path);
              continue;
            }
            var skelRoot = new pxr.UsdSkelRoot(skelRootPrim);
            if (!skelRoot) {
              Debug.LogWarning("SkelRoot prim not SkelRoot type: " + path);
              continue;
            }
            if (!skelCache.Populate(skelRoot)) {
              Debug.LogWarning("Failed to populate skel cache: " + path);
              continue;
            }
            skelRoots.Add(skelRoot);
            GameObject go = primMap[path];
            ImporterBase.GetOrAddComponent<Animator>(go, true);
          } catch (System.Exception ex) {
            Debug.LogException(
                new System.Exception("Error pre-processing SkelRoot <" + path + ">", ex));
          }

          if (ShouldYield(targetTime, timer)) { yield return null; ResetTimer(timer); }
        }
        Profiler.EndSample();
      }

      //
      // Import known prim types.
      //

      // Materials.
      Profiler.BeginSample("USD: Build Materials");
      if (importOptions.ShouldBindMaterials) {
        foreach (var pathAndSample in scene.ReadAll<MaterialSample>(usdPrimRoot)) {
          try {
            GameObject go = primMap[pathAndSample.path];
            var mat = MaterialImporter.BuildMaterial(scene,
                                                     pathAndSample.path,
                                                     pathAndSample.sample,
                                                     importOptions);
            if (mat != null) {
              importOptions.materialMap[pathAndSample.path] = mat;
            }
          } catch (System.Exception ex) {
            Debug.LogException(
                new System.Exception("Error processing material <" + pathAndSample.path + ">", ex));
          }

          if (ShouldYield(targetTime, timer)) { yield return null; ResetTimer(timer); }
        }
      }
      Profiler.EndSample();

      // Xforms.
      //
      // Note that we are specifically filtering on XformSample, not Xformable, this way only
      // Xforms are processed to avoid doing that work redundantly.
      if (importOptions.importTransforms) {
        Profiler.BeginSample("USD: Build Xforms");
        foreach (var pathAndSample in scene.ReadAll<XformSample>(usdPrimRoot)) {
          try {
            GameObject go = primMap[pathAndSample.path];
            XformImporter.BuildXform(pathAndSample.sample, go, importOptions);
          } catch (System.Exception ex) {
            Debug.LogException(
                new System.Exception("Error processing xform <" + pathAndSample.path + ">", ex));
          }

          if (ShouldYield(targetTime, timer)) { yield return null; ResetTimer(timer); }
        }
        Profiler.EndSample();
      }

      // Meshes.
      if (importOptions.importMeshes) {

        Profiler.BeginSample("USD: Build Meshes");
        foreach (var pathAndSample in scene.ReadAll<MeshSample>(usdPrimRoot)) {
          try {
            GameObject go = primMap[pathAndSample.path];
            XformImporter.BuildXform(pathAndSample.sample, go, importOptions);

            Profiler.BeginSample("USD: Read Mesh Subsets");
            var subsets = MeshImporter.ReadGeomSubsets(scene, pathAndSample.path);
            Profiler.EndSample();

            // This is pre-cached as part of calling skelCache.Populate and IsValid indicates if we
            // have the data required to setup a skinned mesh.
            var skinningQuery = skelCache.GetSkinningQuery(scene.GetPrimAtPath(pathAndSample.path));
            if (skinningQuery.IsValid()) {
              Profiler.BeginSample("USD: Build Skinned Mesh");
              MeshImporter.BuildSkinnedMesh(pathAndSample.path, pathAndSample.sample, subsets, go, importOptions);
              Profiler.EndSample();
            } else {
              Profiler.BeginSample("USD: Build Mesh");
              MeshImporter.BuildMesh(pathAndSample.path, pathAndSample.sample, subsets, go, importOptions);
              Profiler.EndSample();
            }
          } catch (System.Exception ex) {
            Debug.LogException(
                new System.Exception("Error processing mesh <" + pathAndSample.path + ">", ex));
          }

          if (ShouldYield(targetTime, timer)) { yield return null; ResetTimer(timer); }
        }
        Profiler.EndSample();

        // Cubes.
        Profiler.BeginSample("USD: Build Cubes");
        foreach (var pathAndSample in scene.ReadAll<CubeSample>(usdPrimRoot)) {
          try {
            GameObject go = primMap[pathAndSample.path];
            XformImporter.BuildXform(pathAndSample.sample, go, importOptions);
            CubeImporter.BuildCube(pathAndSample.sample, go, importOptions);
          } catch (System.Exception ex) {
            Debug.LogException(
                new System.Exception("Error processing cube <" + pathAndSample.path + ">", ex));
          }

          if (ShouldYield(targetTime, timer)) { yield return null; ResetTimer(timer); }
        }
        Profiler.EndSample();
      }

      // Cameras.
      if (importOptions.importCameras) {
        Profiler.BeginSample("USD: Cameras");
        foreach (var pathAndSample in scene.ReadAll<CameraSample>(usdPrimRoot)) {
          try {
            GameObject go = primMap[pathAndSample.path];
            XformImporter.BuildXform(pathAndSample.sample, go, importOptions);
            CameraImporter.BuildCamera(pathAndSample.sample, go, importOptions);
          } catch (System.Exception ex) {
            Debug.LogException(
                new System.Exception("Error processing camera <" + pathAndSample.path + ">", ex));
          }

          if (ShouldYield(targetTime, timer)) { yield return null; ResetTimer(timer); }
        }
        Profiler.EndSample();
      }

      // Build out masters for instancing.
      Profiler.BeginSample("USD: Build Instances");
      foreach (var masterRootPath in primMap.GetMasterRootPaths()) {
        try {
          Transform masterRootXf = primMap[masterRootPath].transform;

          // Transforms
          if (importOptions.importTransforms) {
            Profiler.BeginSample("USD: Build Xforms");
            foreach (var pathAndSample in scene.ReadAll<XformSample>(masterRootPath)) {
              try {
                GameObject go = primMap[pathAndSample.path];
                XformImporter.BuildXform(pathAndSample.sample, go, importOptions);
              } catch (System.Exception ex) {
                Debug.LogException(
                    new System.Exception("Error processing xform <" + pathAndSample.path + ">", ex));
              }
            }
            Profiler.EndSample();
          }

          // Meshes.
          if (importOptions.importMeshes) {
            Profiler.BeginSample("USD: Build Meshes");
            foreach (var pathAndSample in scene.ReadAll<MeshSample>(masterRootPath)) {
              try {
                GameObject go = primMap[pathAndSample.path];
                XformImporter.BuildXform(pathAndSample.sample, go, importOptions);
                var subsets = MeshImporter.ReadGeomSubsets(scene, pathAndSample.path);
                MeshImporter.BuildMesh(pathAndSample.path, pathAndSample.sample, subsets, go, importOptions);
              } catch (System.Exception ex) {
                Debug.LogException(
                    new System.Exception("Error processing mesh <" + pathAndSample.path + ">", ex));
              }
            }
            Profiler.EndSample();

            // Cubes.
            Profiler.BeginSample("USD: Build Cubes");
            foreach (var pathAndSample in scene.ReadAll<CubeSample>(masterRootPath)) {
              try {
                GameObject go = primMap[pathAndSample.path];
                XformImporter.BuildXform(pathAndSample.sample, go, importOptions);
                CubeImporter.BuildCube(pathAndSample.sample, go, importOptions);
              } catch (System.Exception ex) {
                Debug.LogException(
                    new System.Exception("Error processing cube <" + pathAndSample.path + ">", ex));
              }
            }
            Profiler.EndSample();
          }

          // Cameras.
          if (importOptions.importCameras) {
            Profiler.BeginSample("USD: Build Cameras");
            foreach (var pathAndSample in scene.ReadAll<CameraSample>(masterRootPath)) {
              try {
                GameObject go = primMap[pathAndSample.path];
                XformImporter.BuildXform(pathAndSample.sample, go, importOptions);
                CameraImporter.BuildCamera(pathAndSample.sample, go, importOptions);
              } catch (System.Exception ex) {
                Debug.LogException(
                    new System.Exception("Error processing camera <" + pathAndSample.path + ">", ex));
              }
            }
            Profiler.EndSample();
          }

        } catch (System.Exception ex) {
          Debug.LogException(
              new System.Exception("Error processing master <" + masterRootPath + ">", ex));
        }

        if (ShouldYield(targetTime, timer)) { yield return null; ResetTimer(timer); }
      } // Instances.
      Profiler.EndSample();

      //
      // Post-process dependencies: materials and bones.
      //

      Profiler.BeginSample("USD: Process Material Bindings");
      try {
        // TODO: Currently ProcessMaterialBindings runs too long and will go over budget for any
        // large scene. However, pulling the loop into this code feels wrong in terms of
        // responsibilities.

        // Process all material bindings in a single vectorized request.
        MaterialImporter.ProcessMaterialBindings(scene, importOptions);
      } catch (System.Exception ex) {
        Debug.LogException(new System.Exception("Failed in ProcessMaterialBindings", ex));
      }
      Profiler.EndSample();

      if (ShouldYield(targetTime, timer)) { yield return null; ResetTimer(timer); }

      //
      // SkinnedMesh bone bindings.
      //
      if (importOptions.importSkinning) {
        Profiler.BeginSample("USD: Build Skeletons");
        var skeletonSamples = new Dictionary<pxr.SdfPath, SkeletonSample>();
        var skeletonBindPoses = new Dictionary<pxr.SdfPath, Dictionary<pxr.TfToken, Matrix4x4>>();
        var skeletonJoints = new Dictionary<pxr.SdfPath, pxr.VtTokenArray>();
        foreach (var skelRoot in skelRoots) {
          try {
            var bindings = new pxr.UsdSkelBindingVector();
            if (!skelCache.ComputeSkelBindings(skelRoot, bindings)) {
              Debug.LogWarning("ComputeSkelBindings failed");
              continue;
            }

            if (bindings.Count == 0) {
              Debug.LogWarning("No bindings found skelRoot: " + skelRoot.GetPath());
            }

            foreach (var skelBinding in bindings) {
              // The SkelRoot will likely have a skeleton binding, but it's inherited, so the bound
              // skeleton isn't actually known until it's queried from the binding. Still, we would
              // like not to reprocess skeletons redundantly, so skeletons are cached into a
              // dictionary.

              var skelPath = skelBinding.GetSkeleton().GetPath();
              SkeletonSample skelSample = null;
              if (!skeletonSamples.TryGetValue(skelPath, out skelSample)) {
                skelSample = new SkeletonSample();
                scene.Read(skelPath, skelSample);
                skeletonSamples.Add(skelPath, skelSample);

                // Unity uses the inverse bindTransform, since that's actually what's needed for skinning.
                // Do that once here, so each skinned mesh doesn't need to do it redunddantly.
                SkeletonImporter.BuildBindTransforms(skelPath, skelSample, importOptions);

                var bindXforms = new pxr.VtMatrix4dArray();

                var prim = scene.GetPrimAtPath(skelPath);
                var skel = new pxr.UsdSkelSkeleton(prim);
                pxr.UsdSkelSkeletonQuery skelQuery = skelCache.GetSkelQuery(skel);

                if (!skelQuery.GetJointWorldBindTransforms(bindXforms)) {
                  throw new System.Exception("Failed to compute binding trnsforms for <" + skelPath + ">");
                }

                var dict = new Dictionary<pxr.TfToken, Matrix4x4>();
                var xfs = UnityTypeConverter.FromVtArray(bindXforms);
                var joints = skelQuery.GetJointOrder();
                for (int i = 0; i < joints.size(); i++) {
                  dict[joints[i]] = xfs[i];
                }
                skeletonBindPoses.Add(skelPath, dict);
                skeletonJoints.Add(skelPath, joints);

                SkeletonImporter.BuildDebugBindTransforms(skelSample, primMap[skelPath], importOptions);
              }

              //
              // Apply skinning weights to each skinned mesh.
              //
              foreach (var skinningQuery in skelBinding.GetSkinningTargetsAsVector()) {
                var meshPath = skinningQuery.GetPrim().GetPath();
                try {
                  var skelBindingSample = new SkelBindingSample();
                  var goMesh = primMap[meshPath];

                  scene.Read(meshPath, skelBindingSample);

                  SkeletonImporter.BuildSkinnedMesh(
                      meshPath,
                      skelPath,
                      skelSample,
                      skelBindingSample,
                      goMesh,
                      primMap,
                      importOptions);

                  var jointOrder = new pxr.VtTokenArray();
                  if (!skinningQuery.GetJointOrder(jointOrder)) {
                    throw new System.Exception("Failed to read joint order for <" + meshPath + ">");
                  }

                  if (jointOrder.size() == 0) {
                    jointOrder = skeletonJoints[skelPath];
                  }

                  var bindPoses = new Matrix4x4[jointOrder.size()];
                  var bones = new Transform[jointOrder.size()];
                  for (int i = 0; i < bindPoses.Length; i++) {
                    bindPoses[i] = skeletonBindPoses[skelPath][jointOrder[i]].inverse;
                    var bonePath = scene.GetSdfPath(jointOrder[i]);
                    var boneGo = primMap[skelPath.AppendPath(bonePath)];
                    bones[i] = boneGo.transform;
                  }

                  goMesh.GetComponent<SkinnedMeshRenderer>().rootBone = primMap[skelPath].transform;
                } catch (System.Exception ex) {
                  Debug.LogException(new System.Exception("Error skinning mesh: " + meshPath, ex));
                }
              }
            }

          } catch (System.Exception ex) {
            Debug.LogException(
                new System.Exception("Error processing SkelRoot <" + skelRoot.GetPath() + ">", ex));
          }
        } // foreach SkelRoot
        Profiler.EndSample();

        if (ShouldYield(targetTime, timer)) { yield return null; ResetTimer(timer); }

        //
        // Bone transforms.
        //
        Profiler.BeginSample("USD: Pose Bones");
        foreach (var pathAndSample in skeletonSamples) {
          var skelPath = pathAndSample.Key;

          try {
            var prim = scene.GetPrimAtPath(skelPath);
            var skel = new pxr.UsdSkelSkeleton(prim);

            pxr.UsdSkelSkeletonQuery skelQuery = skelCache.GetSkelQuery(skel);
            var joints = skelQuery.GetJointOrder();
            var restXforms = new pxr.VtMatrix4dArray();
            var time = scene.Time.HasValue ? scene.Time.Value : pxr.UsdTimeCode.Default();
            if (!skelQuery.ComputeJointLocalTransforms(restXforms, time, atRest: false)) {
              throw new System.Exception("Failed to compute bind trnsforms for <" + skelPath + ">");
            }

            for (int i = 0; i < joints.size(); i++) {
              var goBone = primMap[skelPath.AppendPath(scene.GetSdfPath(joints[i]))];
              var restXform = UnityTypeConverter.FromMatrix(restXforms[i]);
              SkeletonImporter.BuildSkeletonBone(skelPath, goBone, restXform, joints, importOptions);
            }
          } catch (System.Exception ex) {
            Debug.LogException(
                new System.Exception("Error processing SkelRoot <" + skelPath + ">", ex));
          }

          if (ShouldYield(targetTime, timer)) { yield return null; ResetTimer(timer); }
        }
        Profiler.EndSample();
      }

      //
      // Apply instancing.
      //
      if (importOptions.importSceneInstances) {
        Profiler.BeginSample("USD: Build Scene-Instances");
        try {
          // Build scene instances.
          InstanceImporter.BuildSceneInstances(primMap, importOptions);
        } catch (System.Exception ex) {
          Debug.LogException(new System.Exception("Failed in BuildSceneInstances", ex));
        }
        Profiler.EndSample();
      }

      if (ShouldYield(targetTime, timer)) { yield return null; ResetTimer(timer); }

      // Build point instances.
      if (importOptions.importPointInstances) {
        Profiler.BeginSample("USD: Build Point-Instances");
        // TODO: right now all point instancer data is read, but we only need prototypes and indices.
        foreach (var pathAndSample in scene.ReadAll<PointInstancerSample>()) {
          try {
            GameObject instancerGo = primMap[pathAndSample.path];

            // Now build the point instances.
            InstanceImporter.BuildPointInstances(scene,
                                                 primMap,
                                                 pathAndSample.path,
                                                 pathAndSample.sample,
                                                 instancerGo,
                                                 importOptions);
          } catch (System.Exception ex) {
            Debug.LogError("Error processing point instancer <" + pathAndSample.path + ">: " + ex.Message);
          }

          if (ShouldYield(targetTime, timer)) { yield return null; ResetTimer(timer); }
        }
        Profiler.EndSample();
      }

      //
      // Apply root transform corrections.
      //
      Profiler.BeginSample("USD: Build Root Transforms");
      if (!composingSubtree) {
        if (!root) {
          // There is no single root,
          // Apply root transform corrections to all imported root prims.
          foreach (KeyValuePair<pxr.SdfPath, GameObject> kvp in primMap) {
            if (kvp.Key.IsRootPrimPath() && kvp.Value != null) {
              // The root object at which the USD scene will be reconstructed.
              // It may need a Z-up to Y-up conversion and a right- to left-handed change of basis.
              XformImporter.BuildSceneRoot(scene, kvp.Value.transform, importOptions);
            }
          }
        } else {
          // There is only one root, apply a single transform correction.
          XformImporter.BuildSceneRoot(scene, root.transform, importOptions);
        }
      }
      Profiler.EndSample();

      //
      // Clean up.
      //
      Profiler.BeginSample("USD: Cleanup Masters");
      // Destroy all temp masters.
      foreach (var path in primMap.GetMasterRootPaths()) {
        GameObject.DestroyImmediate(primMap[path]);
        if (ShouldYield(targetTime, timer)) { yield return null; ResetTimer(timer); }
      }
      Profiler.EndSample();

      //
      // AfterImport callback.
      //
      Profiler.BeginSample("AfterImport callback");
      if (AfterImport != null) {
        AfterImport(primMap, scene, usdPrimRoot, root, importOptions);
      }
      Profiler.EndSample();
    }

  }
}
