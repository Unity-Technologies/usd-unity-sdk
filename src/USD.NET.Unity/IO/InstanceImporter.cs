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
  /// A collection of methods used for translating USD instances into Unity instances (CPU or GPU).
  /// </summary>
  public static class InstanceImporter {

    /// <summary>
    /// Given a PrimMap, finds all instanced objects and their respective master objects and
    /// instantiates Unity clones using GameObject.Instantiate. Note that this does not result
    /// in GPU instancing.
    /// </summary>
    public static void BuildSceneInstances(PrimMap primMap, SceneImportOptions options) {

      if (options.enableGpuInstancing) {
        foreach (var masterPath in primMap.GetMasterRootPaths()) {
          EnableGpuInstancing(primMap[masterPath]);
        }
      }

      foreach (var instance in primMap.GetInstanceRoots()) {
        GameObject goInstance = instance.gameObject;
        GameObject goMaster = primMap[instance.masterPath];
        foreach (Transform child in goMaster.transform) {
          var newChild = GameObject.Instantiate(child.gameObject);
          newChild.transform.SetParent(goInstance.transform, worldPositionStays: false);
          primMap.AddInstance(newChild);
        }
      }
    }

    public static void BuildPointInstances(Scene scene,
                                           PrimMap primMap,
                                           string pointInstancerPath,
                                           PointInstancerSample sample,
                                           GameObject root,
                                           SceneImportOptions options) {
      Matrix4x4[] transforms = sample.ComputeInstanceMatrices(scene, pointInstancerPath);
      int i = 0;

      foreach (var protoRoot in sample.prototypes.targetPaths) {
        var go = primMap[new pxr.SdfPath(protoRoot)];
        go.SetActive(false);
        if (options.enableGpuInstancing) {
          EnableGpuInstancing(go);
        }
      }

      foreach (var index in sample.protoIndices) {
        var targetPath = sample.prototypes.targetPaths[index];

        var goMaster = primMap[new pxr.SdfPath(targetPath)];
        var xf = transforms[i];

        var goInstance = GameObject.Instantiate(goMaster, root.transform);
        goInstance.SetActive(true);
        goInstance.name = goMaster.name + "_" + i;
        XformImporter.BuildXform(xf, goInstance, options);

        // Safety net.
        if (i > 100000) {
          break;
        }

        i++;
      }
    }

    private static void EnableGpuInstancing(GameObject go) {
      foreach (MeshRenderer mr in go.GetComponentsInChildren<MeshRenderer>()) {
        if (mr.sharedMaterial != null && !mr.sharedMaterial.enableInstancing) {
          mr.sharedMaterial = Material.Instantiate(mr.sharedMaterial);
          mr.sharedMaterial.enableInstancing = true;
        }
        for (int i = 0; i < mr.sharedMaterials.Length; i++) {
          var im = mr.sharedMaterials[i];
          if (im == null || im.enableInstancing == true) { continue; }
          mr.sharedMaterials[i] = Material.Instantiate(im);
          mr.sharedMaterials[i].enableInstancing = true;
        }
      }
    }

  }
}
