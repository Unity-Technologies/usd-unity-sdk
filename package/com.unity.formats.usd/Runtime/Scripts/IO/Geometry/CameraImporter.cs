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
using USD.NET;
using USD.NET.Unity;

namespace Unity.Formats.USD {

  /// <summary>
  /// A collection of methods used for importing USD Camera data into Unity.
  /// </summary>
  public static class CameraImporter {

    /// <summary>
    /// Copy camera data from USD to Unity with the given import options.
    /// </summary>
    public static void BuildCamera(pxr.SdfPath path,
      CameraSample usdCamera,
      GameObject go,
      SceneImportOptions options, Scene scene)
    {
      if (scene.AccessMask == null || scene.IsPopulatingAccessMask || scene.AccessMask.Included.ContainsKey(path))
      {
        var cam = ImporterBase.GetOrAddComponent<Camera>(go);
        usdCamera.CopyToCamera(cam, false);
        cam.nearClipPlane *= options.scale;
        cam.farClipPlane *= options.scale;
      }
    }
  }
}
