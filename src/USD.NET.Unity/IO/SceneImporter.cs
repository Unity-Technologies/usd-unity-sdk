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
    public static PrimMap BuildScene(Scene scene, GameObject root, SceneImportOptions importOptions) {

      // The root object at which the USD scene will be reconstructed.
      // It may need a Z-up to Y-up conversion and a right- to left-handed change of basis.
      XformImporter.BuildSceneRoot(scene, root.transform, importOptions);

      // Reconstruct the USD hierarchy as Unity GameObjects.
      // A PrimMap is returned for tracking the USD <-> Unity mapping.
      var primMap = HierarchyBuilder.BuildGameObjects(scene, root);

      //
      // Import known prim types.
      //

      // Xforms.
      //
      // Note that we are specifically filtering on XformSample, not Xformable, this way only
      // Xforms are processed to avoid doing that work redundantly.
      foreach (var pathAndSample in scene.ReadAll<XformSample>()) {
        GameObject go = primMap[pathAndSample.path];
        XformImporter.BuildXform(pathAndSample.sample, go, importOptions);
      }

      // Meshes.
      foreach (var pathAndSample in scene.ReadAll<MeshSample>()) {
        GameObject go = primMap[pathAndSample.path];
        XformImporter.BuildXform(pathAndSample.sample, go, importOptions);
        MeshImporter.BuildMesh(pathAndSample.sample, go, importOptions);
      }

      // Build out masters for instancing.
      foreach (var masterRootPath in primMap.GetMasterRootPaths()) {
        Transform masterRootXf = primMap[masterRootPath].transform;

        foreach (var pathAndSample in scene.ReadAll<XformSample>(masterRootPath)) {
          GameObject go = primMap[pathAndSample.path];
          XformImporter.BuildXform(pathAndSample.sample, go, importOptions);
        }

        // Meshes.
        foreach (var pathAndSample in scene.ReadAll<MeshSample>(masterRootPath)) {
          GameObject go = primMap[pathAndSample.path];
          XformImporter.BuildXform(pathAndSample.sample, go, importOptions);
          MeshImporter.BuildMesh(pathAndSample.sample, go, importOptions);
        }
      }

      // Build scene instances.
      InstanceImporter.BuildSceneInstances(primMap, importOptions);

      // Build point instances.
      // TODO: right now all point instancer data is read, but we only need prototypes and indices.
      foreach (var pathAndSample in scene.ReadAll<PointInstancerSample>()) {
        GameObject instancerGo = primMap[pathAndSample.path];

        // Now build the point instances.
        InstanceImporter.BuildPointInstances(scene,
                                             primMap,
                                             pathAndSample.path,
                                             pathAndSample.sample,
                                             instancerGo,
                                             importOptions);
      }

      return primMap;
    }
  }
}
