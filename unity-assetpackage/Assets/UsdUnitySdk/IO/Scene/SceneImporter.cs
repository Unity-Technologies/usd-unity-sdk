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

using System.Collections.Generic;
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
      // Pre-process UsdSkelRoots.
      //
      var skelRoots = new List<pxr.UsdSkelRoot>();
      var skelCache = new pxr.UsdSkelCache();
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
      }

      //
      // Import known prim types.
      //

      // Materials.
      if (importOptions.ShouldBindMaterials) {
        foreach (var pathAndSample in scene.ReadAll<MaterialSample>(usdPrimRoot)) {
          try {
            GameObject go = primMap[pathAndSample.path];
            var mat = MaterialImporter.BuildMaterial(scene, pathAndSample.path, pathAndSample.sample, importOptions);
            if (mat != null) {
              importOptions.materialMap[pathAndSample.path] = mat;
            }
          } catch (System.Exception ex) {
            Debug.LogException(
                new System.Exception("Error processing material <" + pathAndSample.path + ">", ex));
          }
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

          // This is pre-cached as part of calling skelCache.Populate and IsValid indicates if we
          // have the data required to setup a skinned mesh.
          var skinningQuery = skelCache.GetSkinningQuery(scene.GetPrimAtPath(pathAndSample.path));

          if (skinningQuery.IsValid()) {
            MeshImporter.BuildSkinnedMesh(pathAndSample.path, pathAndSample.sample, subsets, go, importOptions);
          } else {
            MeshImporter.BuildMesh(pathAndSample.path, pathAndSample.sample, subsets, go, importOptions);
          }
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

      //
      // SkinnedMesh bone bindings.
      //
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
                  var bonePath = new pxr.SdfPath(jointOrder[i]);
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
      }

      //
      // Bone transforms.
      //
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
            var goBone = primMap[skelPath.AppendPath(new pxr.SdfPath(joints[i]))];
            var restXform = UnityTypeConverter.FromMatrix(restXforms[i]);
            SkeletonImporter.BuildSkeletonBone(skelPath, goBone, restXform, joints, importOptions);
          }
        } catch (System.Exception ex) {
          Debug.LogException(
              new System.Exception("Error processing SkelRoot <" + skelPath + ">", ex));
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

      foreach (KeyValuePair<pxr.SdfPath, GameObject> kvp in primMap) {
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
