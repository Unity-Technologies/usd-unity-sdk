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

using System;
using System.Collections.Generic;
using pxr;
using UnityEngine;

namespace USD.NET.Unity {

  public static class StandardShaderIo {

    private static string SetupTexture(Scene scene,
                                     string usdShaderPath,
                                     Material material,
                                     PreviewSurfaceSample surface,
                                     string destTexturePath,
                                     string textureName,
                                     string textureOutput) {
#if UNITY_EDITOR
      var srcPath = UnityEditor.AssetDatabase.GetAssetPath(material.GetTexture(textureName));
      srcPath = srcPath.Substring("Assets/".Length);
      srcPath = Application.dataPath + "/" + srcPath;
      var fileName = System.IO.Path.GetFileName(srcPath);
      var filePath = System.IO.Path.Combine(destTexturePath, fileName);
      System.IO.File.Copy(srcPath, filePath, overwrite: true);
#else
      // Not supported at run-time, too many things can go wrong
      // (can't encode compressed textures, etc).
      throw new System.Exception("Not supported at run-time");
#endif
      var uvReader = new PrimvarReaderSample<Vector2>();
      uvReader.varname.defaultValue = new TfToken("uv");
      scene.Write(usdShaderPath + "/uvReader", uvReader);
      var tex = new USD.NET.Unity.TextureReaderSample(filePath, usdShaderPath + "/uvReader.outputs:result");
      scene.Write(usdShaderPath + "/" + textureName, tex);
      return usdShaderPath + "/" + textureName + ".outputs:" + textureOutput;
    }

    public static void ExportStandardSpecular(Scene scene,
                                              string usdShaderPath,
                                              Material material,
                                              PreviewSurfaceSample surface,
                                              string destTexturePath) {
      Color c;

      ImportStandardCommon(scene, usdShaderPath, material, surface, destTexturePath);
      surface.useSpecularWorkflow.defaultValue = 1;

      if (material.HasProperty("_SpecGlossMap") && material.GetTexture("_SpecGlossMap") != null) {
        var newTex = SetupTexture(scene, usdShaderPath, material, surface, destTexturePath, "_SpecGlossMap", "rgb");
        surface.specularColor.SetConnectedPath(newTex);
      } else if (material.HasProperty("_SpecColor")) {
        // If there is a spec color, then this is not metallic workflow.
        c = material.GetColor("_SpecColor");
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
    }

    public static void ExportStandardRoughness(Scene scene,
                                          string usdShaderPath,
                                          Material material,
                                          PreviewSurfaceSample surface,
                                          string destTexturePath) {
      ImportStandardCommon(scene, usdShaderPath, material, surface, destTexturePath);
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
                                      PreviewSurfaceSample surface,
                                      string destTexturePath) {
      ImportStandardCommon(scene, usdShaderPath, material, surface, destTexturePath);
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

    private static void ImportStandardCommon(Scene scene,
                                          string usdShaderPath,
                                          Material mat,
                                          PreviewSurfaceSample surface,
                                          string destTexturePath) {
      Color c;

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
        } else if (mat.HasProperty("_EmissionColor")) {
          c = mat.GetColor("_EmissionColor").linear;
          surface.emissiveColor.defaultValue = new Vector3(c.r, c.g, c.b);
        }
      }
    }

  }
}
