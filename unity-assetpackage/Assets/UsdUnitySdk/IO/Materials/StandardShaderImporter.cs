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
  public static class StandardShaderImporter {
    static public Material BuildMaterial(Scene scene,
                                         Material unityMaterial,
                                         MaterialSample materialSample,
                                         PreviewSurfaceSample previewSurf,
                                         SceneImportOptions options) {
      Material mat = unityMaterial;

      if (previewSurf.diffuseColor.IsConnected()) {
        mat.SetTexture("_MainTex",
            MaterialImporter.ImportConnectedTexture(scene, previewSurf.diffuseColor, options));
      } else {
        // TODO: this should delegate to a material mapper.
        var rgb = previewSurf.diffuseColor.defaultValue;
        mat.color = new Color(rgb.x, rgb.y, rgb.z).gamma;
      }

      if (previewSurf.normal.IsConnected()) {
        mat.SetTexture("_BumpMap",
            MaterialImporter.ImportConnectedTexture(scene, previewSurf.normal, options));
      } else {
        // TODO: Unity has no notion of a constant normal map.
      }

      if (previewSurf.displacement.IsConnected()) {
        mat.SetTexture("_ParallaxMap",
            MaterialImporter.ImportConnectedTexture(scene, previewSurf.displacement, options));
      } else {
        // TODO: Unity has no notion of a parallax map.
      }

      if (previewSurf.occlusion.IsConnected()) {
        mat.SetTexture("_OcclusionMap",
            MaterialImporter.ImportConnectedTexture(scene, previewSurf.occlusion, options));
      } else {
        // TODO: Unity has no notion of a constant occlusion value.
      }

      if (previewSurf.emissiveColor.IsConnected()) {
        var tex = MaterialImporter.ImportConnectedTexture(scene, previewSurf.emissiveColor, options);
        mat.SetTexture("_EmissionMap", tex);
        if (tex != null) {
          mat.EnableKeyword("_EMISSION");
        }
      } else {
        var rgb = previewSurf.emissiveColor.defaultValue;
        mat.SetColor("_EmissionColor", new Color(rgb.x, rgb.y, rgb.z).gamma);
        if (rgb.x > 0 || rgb.y > 0 || rgb.z > 0) {
          mat.EnableKeyword("_EMISSION");
        }
      }

      if (previewSurf.useSpecularWorkflow.defaultValue == 1) {
        Texture2D specTex = null;
        if (previewSurf.specularColor.IsConnected()) {
          specTex = MaterialImporter.ImportConnectedTexture(scene, previewSurf.specularColor, options);
          mat.SetTexture("_SpecGlossMap", specTex);
        } else {
          var rgb = previewSurf.diffuseColor.defaultValue;
          mat.SetColor("_SpecColor", new Color(rgb.x, rgb.y, rgb.z).gamma);
        }

        if (previewSurf.roughness.IsConnected()) {
          // Roughness for spec setup is tricky, since it may require merging two textures.
          // For now, just detect that case and issue a warning (e.g. when roughness has a map,
          // but isn't the albedo or spec map).
          // Roughness also needs to be converted to glossiness.
          var roughTex = MaterialImporter.ImportConnectedTexture(scene, previewSurf.roughness, options);
          if (roughTex != null && roughTex != specTex) {
            var specGlossTex = MaterialImporter.CombineRoughnessToGloss(specTex, roughTex);
            mat.SetTexture("_SpecGlossMap", specGlossTex);
          }
          mat.EnableKeyword("_SPECGLOSSMAP");
        } else {
          mat.SetFloat("_Glossiness", 1 - previewSurf.roughness.defaultValue);
        }
      } else {
        if (previewSurf.metallic.IsConnected()) {
          mat.SetTexture("_MetallicGlossMap",
              MaterialImporter.ImportConnectedTexture(scene, previewSurf.metallic, options));
        } else {
          mat.SetFloat("_Metallic", previewSurf.metallic.defaultValue);
        }

        if (previewSurf.roughness.IsConnected()) {
          // In this case roughness get its own map, but still must be converted to glossiness.
          mat.SetTexture("_SpecGlossMap",
              MaterialImporter.ImportConnectedTexture(scene, previewSurf.roughness, options));
        } else {
          mat.SetFloat("_Glossiness", 1 - previewSurf.metallic.defaultValue);
        }
      }

      return mat;
    }
  }

}
