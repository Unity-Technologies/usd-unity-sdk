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

namespace USD.NET.Unity.IO {

  /// <summary>
  /// A collection of methods used for importing USD Material data into Unity.
  /// </summary>
  public static class MaterialImporter {

    /// <summary>
    /// Reads and returns the UsdPreviewSurface data for the prim at the given path, if present.
    /// </summary>
    /// <param name="scene">The USD scene object.</param>
    /// <param name="primPath">The path to the object in the USD scene.</param>
    /// <returns>A PreviewSurfaceSample if found, otherwise null.</returns>
    private static PreviewSurfaceSample GetSurfaceShaderPrim(Scene scene, string primPath) {
      var materialBinding = new MaterialBindingSample();
      scene.Read(primPath, materialBinding);

      var matPath = materialBinding.binding.GetTarget(0);
      if (matPath == null) {
        //Debug.LogWarning("No material binding found at: <" + meshPath + ">");
        return null;
      }

      var materialSample = new MaterialSample();
      scene.Read(matPath, materialSample);
      if (string.IsNullOrEmpty(materialSample.surface.connectedPath)) {
        Debug.LogWarning("Material surface not connected: <" + matPath + ">");
      }

      var exportSurf = new PreviewSurfaceSample();
      scene.Read(new pxr.SdfPath(materialSample.surface.connectedPath).GetPrimPath(), exportSurf);

      return exportSurf;
    }

  }
}
