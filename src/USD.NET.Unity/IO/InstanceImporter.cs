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
    public static void BuildSceneInstances(PrimMap primMap) {
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
  }
}
