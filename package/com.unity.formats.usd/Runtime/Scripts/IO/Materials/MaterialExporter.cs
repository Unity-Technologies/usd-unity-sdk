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
  public static class MaterialExporter {

    public delegate void ExportHandler(Scene scene,
                                       string usdShaderPath,
                                       Material material,
                                       UnityPreviewSurfaceSample surface,
                                       string destTexturePath);

    public static readonly string kStandardFallbackHandler = "USD.NET/Fallback/Standard";
    public static readonly string kHdrpFallbackHandler = "USD.NET/Fallback/HDRP";
    public static readonly string kLwrpFallbackHandler = "USD.NET/Fallback/LWRP";

    /// <summary>
    /// The ExportRegistry is a mapping from Unity shader name (e.g. "Standard") to ExportHandler.
    /// When exporting a Material, the shader name is used to lookup the export hanlder function
    /// in this registry.
    /// </summary>
    static public Dictionary<string, ExportHandler> ExportRegistry
        = new Dictionary<string, ExportHandler>();

    static MaterialExporter() {
      ExportRegistry.Add("Standard",
                         StandardShaderExporter.ExportStandard);
      ExportRegistry.Add("Standard (Roughness setup)",
                         StandardShaderExporter.ExportStandardRoughness);
      ExportRegistry.Add("Standard (Specular setup)",
                         StandardShaderExporter.ExportStandardSpecular);
      ExportRegistry.Add("HDRenderPipeline/Lit",
                         HdrpShaderExporter.ExportLit);
      ExportRegistry.Add(kStandardFallbackHandler,
                         StandardShaderExporter.ExportGeneric);
    }

    public static void ExportMaterial(Scene scene, Material mat, string usdMaterialPath) {
      string shaderPath = usdMaterialPath + "/PreviewSurface";

      var material = new MaterialSample();
      material.surface.SetConnectedPath(shaderPath, "outputs:surface");

      scene.Write(usdMaterialPath, material);

      var shader = new UnityPreviewSurfaceSample();
      var texPath = /*TODO: this should be explicit*/
            System.IO.Path.GetDirectoryName(scene.FilePath);

      ExportHandler handler = null;
      if (!ExportRegistry.TryGetValue(mat.shader.name, out handler)) {
        handler = ExportRegistry[kStandardFallbackHandler];
      }

      if (handler == null) {
        Debug.LogException(new System.Exception("Could not find handler to export shader: " + mat.shader.name));
        return;
      }

      handler(scene, shaderPath, mat, shader, texPath);

      scene.Write(shaderPath, shader);
      scene.GetPrimAtPath(shaderPath).CreateAttribute(pxr.UsdShadeTokens.outputsSurface,
                                                      SdfValueTypeNames.Token,
                                                      false,
                                                      pxr.SdfVariability.SdfVariabilityUniform);
    }

  }
}
