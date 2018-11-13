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

using UnityEngine;

namespace USD.NET.Unity {

  /// <summary>
  /// Root entry point for importing an entire USD scene.
  /// </summary>
  public static class SceneImporter {

    /// <summary>
    /// Rebuilds the USD scene as Unity GameObjects, maintaining a mapping from USD to Unity.
    /// </summary>
    public static PrimMap BuildScene(Scene scene,
                                     GameObject root,
                                     pxr.SdfPath usdPrimRoot,
                                     SceneImportOptions importOptions) {

      // Reconstruct the USD hierarchy as Unity GameObjects.
      // A PrimMap is returned for tracking the USD <-> Unity mapping.
      var primMap = HierarchyBuilder.BuildGameObjects(scene, root, usdPrimRoot);

      //
      // Import known prim types.
      //

      // Materials.
      foreach (var pathAndSample in scene.ReadAll<MaterialSample>(usdPrimRoot)) {
        try {
          GameObject go = primMap[pathAndSample.path];
          var mat = MaterialImporter.BuildMaterial(scene, pathAndSample.sample, importOptions);
          if (mat != null) {
            importOptions.materialMap[pathAndSample.path] = mat;
          }
        } catch (System.Exception ex) {
          Debug.LogException(
              new System.Exception("Error processing material <" + pathAndSample.path + ">", ex));
        }
      }

      // Xforms.
      //
      // Note that we are specifically filtering on XformSample, not Xformable, this way only
      // Xforms are processed to avoid doing that work redundantly.
      foreach (var pathAndSample in scene.ReadAll<XformSample>(usdPrimRoot)) {
        try {
          GameObject go = primMap[pathAndSample.path];
          XformImporter.BuildXform(pathAndSample.sample, go, importOptions);
        } catch (System.Exception ex) {
          Debug.LogException(
              new System.Exception("Error processing xform <" + pathAndSample.path + ">", ex));
        }
      }

      // Meshes.
      foreach (var pathAndSample in scene.ReadAll<MeshSample>(usdPrimRoot)) {
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

      // Cubes.
      foreach (var pathAndSample in scene.ReadAll<CubeSample>(usdPrimRoot)) {
        try {
          GameObject go = primMap[pathAndSample.path];
          XformImporter.BuildXform(pathAndSample.sample, go, importOptions);
          CubeImporter.BuildCube(pathAndSample.sample, go, importOptions);
        } catch (System.Exception ex) {
          Debug.LogException(
              new System.Exception("Error processing cube <" + pathAndSample.path + ">", ex));
        }
      }

      // Cameras.
      foreach (var pathAndSample in scene.ReadAll<CameraSample>(usdPrimRoot)) {
        try {
          GameObject go = primMap[pathAndSample.path];
          XformImporter.BuildXform(pathAndSample.sample, go, importOptions);
          CameraImporter.BuildCamera(pathAndSample.sample, go, importOptions);
        } catch (System.Exception ex) {
          Debug.LogException(
              new System.Exception("Error processing camera <" + pathAndSample.path + ">", ex));
        }
      }

      // Build out masters for instancing.
      foreach (var masterRootPath in primMap.GetMasterRootPaths()) {
        try {
          Transform masterRootXf = primMap[masterRootPath].transform;

          foreach (var pathAndSample in scene.ReadAll<XformSample>(masterRootPath)) {
            try {
              GameObject go = primMap[pathAndSample.path];
              XformImporter.BuildXform(pathAndSample.sample, go, importOptions);
            } catch (System.Exception ex) {
              Debug.LogException(
                  new System.Exception("Error processing xform <" + pathAndSample.path + ">", ex));
            }
          }

          // Meshes.
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

          // Cubes.
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

          // Cameras.
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
        } catch (System.Exception ex) {
          Debug.LogException(
              new System.Exception("Error processing master <" + masterRootPath + ">", ex));
        }
      }

      //
      // Post-process dependencies: materials and bones.
      //

      try {
        // Process all material bindings in a single vectorized request.
        MaterialImporter.ProcessMaterialBindings(scene, importOptions);
      } catch (System.Exception ex) {
        Debug.LogException(new System.Exception("Failed in ProcessMaterialBindings", ex));
      }

      var skelCache = new pxr.UsdSkelCache();
      Debug.Log("Processes skeletons");
      foreach (var path in scene.Find<SkelRootSample>(usdPrimRoot)) {
        try {
          Debug.Log("Root: " + path);
          var skelRootPrim = scene.GetPrimAtPath(path);
          if (!skelRootPrim) {
            Debug.LogWarning("Prim not found: " + path);
            continue;
          }
          var skelRoot = new pxr.UsdSkelRoot(skelRootPrim);
          if (!skelRoot) {
            Debug.LogWarning("Prim not SkelRoot: " + path);
            continue;
          }

          if (!skelCache.Populate(skelRoot)) {
            Debug.LogWarning("Failed to populate skel cache: " + path);
            continue;
          }

          var bindings = new pxr.UsdSkelBindingVector();
          if (!skelCache.ComputeSkelBindings(skelRoot, bindings)) {
            Debug.LogWarning("ComputeSkelBindings failed");
            continue;
          }
          if (bindings.Count == 0) {
            Debug.LogWarning("No bindings found " + path);
          }

          foreach (var skelBinding in bindings) {
            var skelSample = new SkeletonSample();
            var skelPath = skelBinding.GetSkeleton().GetPath();
            scene.Read(skelPath, skelSample);

            foreach (var skinningQuery in skelBinding.GetSkinningTargetsAsVector()) {
              var meshPath = skinningQuery.GetPrim().GetPath();
              try {
                var skelBindingSample = new SkelBindingSample();
                var goMesh = primMap[meshPath];
                scene.Read(meshPath, skelBindingSample);
                Debug.LogWarning(" Indices: " + skelBindingSample.jointIndices.value.Length + " path: " + meshPath);
                SkeletonImporter.BuildSkinnedMesh(
                    meshPath,
                    skelPath,
                    skelSample,
                    skelBindingSample,
                    goMesh,
                    primMap,
                    importOptions);
              } catch (System.Exception ex) {
                Debug.LogException(new System.Exception("Error skinning mesh: " + meshPath, ex));
              }
            }
          }

        } catch (System.Exception ex) {
          Debug.LogException(
              new System.Exception("Error processing SkelRoot <" + path + ">", ex));
        }
      }

      //
      // Apply instancing.
      //

      try {
        // Build scene instances.
        InstanceImporter.BuildSceneInstances(primMap, importOptions);
      } catch (System.Exception ex) {
        Debug.LogException(new System.Exception("Failed in BuildSceneInstances", ex));
      }

      // Build point instances.
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
      }

      //
      // Apply root transform corrections to all root prims.
      //

      foreach (System.Collections.Generic.KeyValuePair<pxr.SdfPath, GameObject> kvp in primMap) {
        if (kvp.Key.IsRootPrimPath() && kvp.Value != null) {
          // The root object at which the USD scene will be reconstructed.
          // It may need a Z-up to Y-up conversion and a right- to left-handed change of basis.
          XformImporter.BuildSceneRoot(scene, kvp.Value.transform, importOptions);
        }
      }

      //
      // Clean up.
      //

      // Destroy all temp masters.
      foreach (var path in primMap.GetMasterRootPaths()) {
        GameObject.DestroyImmediate(primMap[path]);
      }

      return primMap;
    }

  }
}
