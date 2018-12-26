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
  public static class MaterialExporter {
    public static void ExportMaterial(Scene scene, Material mat, string usdMaterialPath) {
      string shaderPath = usdMaterialPath + "/PreviewSurface";

      var material = new MaterialSample();
      material.surface.SetConnectedPath(shaderPath, "outputs:surface");

      scene.Write(usdMaterialPath, material);

      var shader = new PreviewSurfaceSample();
      var texPath = /*TODO: this should be explicit*/
            System.IO.Path.GetDirectoryName(scene.FilePath);

      if (mat.shader.name == "Standard (Specular setup)") {
        StandardShaderExporter.ExportStandardSpecular(scene, shaderPath, mat, shader, texPath);
      } else if (mat.shader.name == "Standard (Roughness setup)") {
        StandardShaderExporter.ExportStandardRoughness(scene, shaderPath, mat, shader, texPath);
      } else if (mat.shader.name == "Standard") {
        StandardShaderExporter.ExportStandard(scene, shaderPath, mat, shader, texPath);
      } else if (mat.shader.name == "HDRenderPipeline/Lit") {
        HdrpShaderIo.ExportLit(scene, shaderPath, mat, shader, texPath);
      } else {
        StandardShaderExporter.ExportGeneric(scene, shaderPath, mat, shader, texPath);
      }

      scene.Write(shaderPath, shader);
      scene.GetPrimAtPath(shaderPath).CreateAttribute(pxr.UsdShadeTokens.outputsSurface,
                                                      SdfValueTypeNames.Token,
                                                      false,
                                                      pxr.SdfVariability.SdfVariabilityUniform);
    }

  }
}
