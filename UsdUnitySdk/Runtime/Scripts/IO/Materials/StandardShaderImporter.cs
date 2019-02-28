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

namespace Unity.Formats.USD {

  public class StandardShaderImporter : ShaderImporterBase {

    public StandardShaderImporter(Material material) : base(material) {
    }

    public override void ImportFromUsd() {
      Material mat = Material;

      if (DiffuseMap) {
        mat.SetTexture("_MainTex", DiffuseMap);
      } else {
        mat.color = Diffuse.GetValueOrDefault(mat.color);
      }

      if (NormalMap) {
        mat.SetTexture("_BumpMap", NormalMap);
        mat.EnableKeyword("_NORMALMAP");
      } else {
        // TODO: Unity has no notion of a constant normal map.
      }

      if (DisplacementMap) {
        mat.SetTexture("_ParallaxMap", DisplacementMap);
        mat.EnableKeyword("_PARALLAXMAP");
      } else {
        // TODO: Unity has no notion of a parallax map.
      }

      if (OcclusionMap) {
        mat.SetTexture("_OcclusionMap", OcclusionMap);
      } else {
        // TODO: Unity has no notion of a constant occlusion value.
      }

      if (EmissionMap) {
        mat.SetTexture("_EmissionMap", EmissionMap);
        mat.EnableKeyword("_EMISSION");
      } else {
        var rgb = Emission.GetValueOrDefault(Color.black);
        mat.SetColor("_EmissionColor", rgb);
        if (rgb.r > 0 || rgb.g > 0 || rgb.b > 0) {
          mat.EnableKeyword("_EMISSION");
        }
      }

      if (!IsMetallicWorkflow) {
        if (SpecularMap) {
          mat.SetTexture("_SpecGlossMap", SpecularMap);
          mat.EnableKeyword("_SPECGLOSSMAP");
        } else {
          var rgb = Specular.GetValueOrDefault(Color.gray);
          mat.SetColor("_SpecColor", rgb);
        }

        if (RoughnessMap) {
          // Roughness for spec setup is tricky, since it may require merging two textures.
          // For now, just detect that case and issue a warning (e.g. when roughness has a map,
          // but isn't the albedo or spec map).
          // Roughness also needs to be converted to glossiness.
          if (RoughnessMap != SpecularMap && SpecularMap != null) {
            var specGlossTex = MaterialImporter.CombineRoughnessToGloss(SpecularMap, RoughnessMap);
            mat.SetTexture("_SpecGlossMap", specGlossTex);
            mat.EnableKeyword("_SPECGLOSSMAP");
          } else if (SpecularMap == null && RoughnessMap != DiffuseMap) {
            var mainGlossTex = MaterialImporter.CombineRoughnessToGloss(DiffuseMap, RoughnessMap);
            mat.SetTexture("_SpecGlossMap", mainGlossTex);
            mat.EnableKeyword("_SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A");
          } else {
            // TODO: create a new texture with constant spec value, combined with roughness texture.
          }
        } else {
          float smoothness = 1 - Roughness.GetValueOrDefault(.5f);
          mat.SetFloat("_Glossiness", smoothness);
          mat.SetFloat("_GlossMapScale", smoothness);
        }
      } else {
        if (MetallicMap) {
          mat.SetTexture("_MetallicGlossMap", MetallicMap);
          mat.EnableKeyword("_METALLICGLOSSMAP");
        } else {
          mat.SetFloat("_Metallic", Metallic.GetValueOrDefault(0));
        }

        if (RoughnessMap) {
          // In this case roughness get its own map, but still must be converted to glossiness.
          mat.SetTexture("_SpecGlossMap", RoughnessMap);
        } else {
          float smoothness = 1 - Roughness.GetValueOrDefault(.5f);
          mat.SetFloat("_Glossiness", smoothness);
          mat.SetFloat("_GlossMapScale", smoothness);
        }
      }
    }
  }

}
