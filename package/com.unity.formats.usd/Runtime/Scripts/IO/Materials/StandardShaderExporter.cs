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

namespace Unity.Formats.USD {

  public class StandardShaderExporter : ShaderExporterBase {

    public static void ExportStandardSpecular(Scene scene,
                                              string usdShaderPath,
                                              Material material,
                                              UnityPreviewSurfaceSample surface,
                                              string destTexturePath) {
      Color c;

      ExportStandardCommon(scene, usdShaderPath, material, surface, destTexturePath);
      surface.useSpecularWorkflow.defaultValue = 1;
      surface.metallic.defaultValue = 0;

      if (material.HasProperty("_SpecGlossMap") && material.GetTexture("_SpecGlossMap") != null) {
        var newTex = SetupTexture(scene, usdShaderPath, material, surface, destTexturePath, "_SpecGlossMap", "rgb");
        surface.specularColor.SetConnectedPath(newTex);
      } else if (material.HasProperty("_SpecColor")) {
        // If there is a spec color, then this is not metallic workflow.
        c = material.GetColor("_SpecColor").linear;
        surface.specularColor.defaultValue = new Vector3(c.r, c.g, c.b);
      } else {
        c = new Color(.5f, .5f, .5f);
        surface.specularColor.defaultValue = new Vector3(c.r, c.g, c.b);
      }

      // TODO: Specular and roughness are combined and the shader configuration dictates
      // where the glossiness comes from (albedo or spec alpha).

      if (material.HasProperty("_Glossiness")) {
        surface.roughness.defaultValue = 1 - material.GetFloat("_Glossiness");
      } else {
        surface.roughness.defaultValue = 0.5f;
      }
    }

    public static void ExportStandardRoughness(Scene scene,
                                          string usdShaderPath,
                                          Material material,
                                          UnityPreviewSurfaceSample surface,
                                          string destTexturePath) {
      ExportStandardCommon(scene, usdShaderPath, material, surface, destTexturePath);
      surface.useSpecularWorkflow.defaultValue = 0;

      if (material.HasProperty("_MetallicGlossMap") && material.GetTexture("_MetallicGlossMap") != null) {
        var newTex = SetupTexture(scene, usdShaderPath, material, surface, destTexturePath, "_MetallicGlossMap", "r");
        surface.metallic.SetConnectedPath(newTex);
      } else if (material.HasProperty("_Metallic")) {
        surface.metallic.defaultValue = material.GetFloat("_Metallic");
      } else {
        surface.metallic.defaultValue = .5f;
      }

      if (material.HasProperty("_SpecGlossMap") && material.GetTexture("_SpecGlossMap") != null) {
        var newTex = SetupTexture(scene, usdShaderPath, material, surface, destTexturePath, "_SpecGlossMap", "r");
        surface.roughness.SetConnectedPath(newTex);
      } else if (material.HasProperty("_Glossiness")) {
        surface.roughness.defaultValue = 1 - material.GetFloat("_Glossiness");
      } else {
        surface.roughness.defaultValue = 0.5f;
      }
    }

    public static void ExportStandard(Scene scene,
                                      string usdShaderPath,
                                      Material material,
                                      UnityPreviewSurfaceSample surface,
                                      string destTexturePath) {
      ExportStandardCommon(scene, usdShaderPath, material, surface, destTexturePath);
      surface.useSpecularWorkflow.defaultValue = 0;

      if (material.HasProperty("_MetallicGlossMap") && material.GetTexture("_MetallicGlossMap") != null) {
        var newTex = SetupTexture(scene, usdShaderPath, material, surface, destTexturePath, "_MetallicGlossMap", "r");
        surface.metallic.SetConnectedPath(newTex);
      } else if (material.HasProperty("_Metallic")) {
        surface.metallic.defaultValue = material.GetFloat("_Metallic");
      } else {
        surface.metallic.defaultValue = .5f;
      }

      if (material.HasProperty("_Glossiness")) {
        surface.roughness.defaultValue = 1 - material.GetFloat("_Glossiness");
      } else {
        surface.roughness.defaultValue = 0.5f;
      }
    }

    public static void ExportGeneric(Scene scene,
                                      string usdShaderPath,
                                      Material material,
                                      UnityPreviewSurfaceSample surface,
                                      string destTexturePath) {
      Color c;
      ExportStandardCommon(scene, usdShaderPath, material, surface, destTexturePath);

      if (material.HasProperty("_SpecColor")) {
        // If there is a spec color, then this is not metallic workflow.
        c = material.GetColor("_SpecColor").linear;
      } else {
        c = new Color(.4f, .4f, .4f);
      }
      surface.specularColor.defaultValue = new Vector3(c.r, c.g, c.b);

      if (material.HasProperty("_Metallic")) {
        surface.metallic.defaultValue = material.GetFloat("_Metallic");
      } else {
        surface.metallic.defaultValue = .5f;
      }

      // Gross heuristics to detect workflow.
      if (material.IsKeywordEnabled("_SPECGLOSSMAP")
          || material.HasProperty("_SpecColor")
          || material.HasProperty("_SpecularColor")
          || material.shader.name.ToLower().Contains("specular")) {
        if (material.HasProperty("_SpecGlossMap") && material.GetTexture("_SpecGlossMap") != null) {
          var newTex = SetupTexture(scene, usdShaderPath, material, surface, destTexturePath, "_SpecGlossMap", "rgb");
          surface.specularColor.SetConnectedPath(newTex);
        } else if (material.HasProperty("_SpecColor")) {
          // If there is a spec color, then this is not metallic workflow.
          c = material.GetColor("_SpecColor").linear;
          surface.specularColor.defaultValue = new Vector3(c.r, c.g, c.b);
        } else {
          c = new Color(.5f, .5f, .5f);
          surface.specularColor.defaultValue = new Vector3(c.r, c.g, c.b);
        }

        if (material.HasProperty("_Glossiness")) {
          surface.roughness.defaultValue = 1 - material.GetFloat("_Glossiness");
        } else {
          surface.roughness.defaultValue = 0.5f;
        }
        surface.useSpecularWorkflow.defaultValue = 1;
      } else {
        surface.useSpecularWorkflow.defaultValue = 0;
        if (material.HasProperty("_MetallicGlossMap") && material.GetTexture("_MetallicGlossMap") != null) {
          var newTex = SetupTexture(scene, usdShaderPath, material, surface, destTexturePath, "_MetallicGlossMap", "r");
          surface.metallic.SetConnectedPath(newTex);
        } else if (material.HasProperty("_Metallic")) {
          surface.metallic.defaultValue = material.GetFloat("_Metallic");
        } else {
          surface.metallic.defaultValue = .5f;
        }

        if (material.HasProperty("_Glossiness")) {
          surface.roughness.defaultValue = 1 - material.GetFloat("_Glossiness");
        } else {
          surface.roughness.defaultValue = 0.5f;
        }
      }

      if (material.HasProperty("_Smoothness")) {
        surface.roughness.defaultValue = 1 - material.GetFloat("_Smoothness");
      } else if (material.HasProperty("_Glossiness")) {
        surface.roughness.defaultValue = 1 - material.GetFloat("_Glossiness");
      } else if (material.HasProperty("_Roughness")) {
        surface.roughness.defaultValue = material.GetFloat("_Roughness");
      } else {
        surface.roughness.defaultValue = 0.5f;
      }
    }

    private static void ExportStandardCommon(Scene scene,
                                          string usdShaderPath,
                                          Material mat,
                                          UnityPreviewSurfaceSample surface,
                                          string destTexturePath) {
      Color c;

      // Export all generic parameter.
      // These are not useful to UsdPreviewSurface, but enable perfect round-tripping.
      surface.unity.shaderName = mat.shader.name;
      surface.unity.shaderKeywords = mat.shaderKeywords;
      
      // Unfortunately, parameter names can only be discovered generically in-editor.
#if UNITY_EDITOR
      for (int i = 0; i < UnityEditor.ShaderUtil.GetPropertyCount(mat.shader); i++) {
        string name = UnityEditor.ShaderUtil.GetPropertyName(mat.shader, i);
        if (!mat.HasProperty(name)) {
          continue;
        }
        switch (UnityEditor.ShaderUtil.GetPropertyType(mat.shader, i)) {
          case UnityEditor.ShaderUtil.ShaderPropertyType.Color:
            surface.unity.colorArgs.Add(name, mat.GetColor(name).linear);
            break;
          case UnityEditor.ShaderUtil.ShaderPropertyType.Float:
          case UnityEditor.ShaderUtil.ShaderPropertyType.Range:
            surface.unity.floatArgs.Add(name, mat.GetFloat(name));
            break;
          case UnityEditor.ShaderUtil.ShaderPropertyType.Vector:
            surface.unity.vectorArgs.Add(name, mat.GetVector(name));
            break;
        }
      }
#endif

      surface.opacity.defaultValue = 1;

      if (mat.HasProperty("_MainTex") && mat.GetTexture("_MainTex") != null) {
        var newTex = SetupTexture(scene, usdShaderPath, mat, surface, destTexturePath, "_MainTex", "rgb");
        surface.diffuseColor.SetConnectedPath(newTex);
      } else if (mat.HasProperty("_Color")) {
        // Standard.
        c = mat.GetColor("_Color").linear;
        surface.diffuseColor.defaultValue = new Vector3(c.r, c.g, c.b);
      } else {
        c = Color.white;
        surface.diffuseColor.defaultValue = new Vector3(c.r, c.g, c.b);
      }

      surface.useSpecularWorkflow.defaultValue = 1;

      if (mat.HasProperty("_BumpMap") && mat.GetTexture("_BumpMap") != null) {
        var newTex = SetupTexture(scene, usdShaderPath, mat, surface, destTexturePath, "_BumpMap", "rgb");
        surface.normal.SetConnectedPath(newTex);
      }

      if (mat.HasProperty("_ParallaxMap") && mat.GetTexture("_ParallaxMap") != null) {
        var newTex = SetupTexture(scene, usdShaderPath, mat, surface, destTexturePath, "_ParallaxMap", "rgb");
        surface.displacement.SetConnectedPath(newTex);
      }

      if (mat.HasProperty("_OcclusionMap") && mat.GetTexture("_OcclusionMap") != null) {
        var newTex = SetupTexture(scene, usdShaderPath, mat, surface, destTexturePath, "_OcclusionMap", "r");
        surface.occlusion.SetConnectedPath(newTex);
      }

      /*
      if (mat.HasProperty("_Metallic")) {
        surface.metallic.defaultValue = mat.GetFloat("_Metallic");
      } else {
        surface.metallic.defaultValue = .5f;
      }
      */
      
      if (mat.IsKeywordEnabled("_EMISSION")) {
        if (mat.HasProperty("_EmissionMap") && mat.GetTexture("_EmissionMap") != null) {
          var newTex = SetupTexture(scene, usdShaderPath, mat, surface, destTexturePath, "_EmissionMap", "rgb");
          surface.emissiveColor.SetConnectedPath(newTex);
        }
        
        if (mat.HasProperty("_EmissionColor")) {
          c = mat.GetColor("_EmissionColor").linear;
          surface.emissiveColor.defaultValue = new Vector3(c.r, c.g, c.b);
        }
      }
    }

  }
}
