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
  /// A collection of methods used for importing USD Material data into Unity.
  /// </summary>
  public static class MaterialImporter {

    public static void ProcessMaterialBindings(Scene scene, SceneImportOptions importOptions) {
      var requests = importOptions.materialMap.ClearRequestedBindings();
      var prims = new pxr.UsdPrimVector();
      foreach (var pathAndRequest in requests) {
        var prim = scene.GetPrimAtPath(pathAndRequest.Key);
        if (prim == null) { continue; }
        prims.Add(prim);
      }

      var matBindTok = new pxr.TfToken("materialBind");
      var matVector = pxr.UsdShadeMaterialBindingAPI.ComputeBoundMaterials(prims, matBindTok);
      var materialSample = new MaterialSample();
      var matIndex = -1;

      foreach (var usdMat in matVector) {
        matIndex++;
        Material unityMat = importOptions.materialMap[usdMat.GetPath()];

        if (unityMat == null) {
          continue;
        }

        // PERF: this is slow and garbage-y.
        string path = prims[matIndex].GetPath();

        if (!requests.ContainsKey(path)) {
          Debug.LogError("Source object key not found: " + path);
          continue;
        }
        requests[path](unityMat);
      }
    }

    public static Material BuildMaterial(Scene scene,
                                         string materialPath,
                                         MaterialSample sample,
                                         SceneImportOptions options) {
      if (string.IsNullOrEmpty(sample.surface.connectedPath)) {
        Debug.LogWarning("Material has no connected surface: <" + materialPath + ">");
        return null;
      }
      var previewSurf = new PreviewSurfaceSample();
      scene.Read(new pxr.SdfPath(sample.surface.connectedPath).GetPrimPath(), previewSurf);

      // Currently, only UsdPreviewSurface is supported.
      if (previewSurf.id == null || previewSurf.id != "UsdPreviewSurface") {
        Debug.LogWarning("Unknown surface type: <" + sample.surface.connectedPath + ">"
                         + "Surface ID: " + previewSurf.id);
        return null;
      }
      var mat = Material.Instantiate(options.materialMap.FallbackMasterMaterial);

      if (previewSurf.diffuseColor.IsConnected()) {
        // TODO: look for the expected texture/primvar reader pair.
      } else {
        // TODO: this should delegate to a material mapper.
        var rgb = previewSurf.diffuseColor.defaultValue;
        mat.color = new Color(rgb.x, rgb.y, rgb.z).gamma;
      }

      return mat;
    }

    /// <summary>
    /// Reads and returns the UsdPreviewSurface data for the prim at the given path, if present.
    /// </summary>
    /// <param name="scene">The USD scene object.</param>
    /// <param name="primPath">The path to the object in the USD scene.</param>
    /// <returns>A PreviewSurfaceSample if found, otherwise null.</returns>
    public static PreviewSurfaceSample GetSurfaceShaderPrim(Scene scene, string primPath) {
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
