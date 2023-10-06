// Copyright 2023 Unity Technologies. All rights reserved.
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

namespace Unity.Formats.USD
{
    public class UrpShaderImporter : ShaderImporterBase
    {
        public UrpShaderImporter(Material material) : base(material)
        {
        }

        // Based on HdrpShaderImporter.cs, reordered to the same order as URP's Lit.shader
        public override void ImportFromUsd()
        {
            Material mat = Material;

            mat.SetFloat("_WorkflowMode", IsSpecularWorkflow ? 0.0f : 1.0f);

            // BaseMap
            if (DiffuseMap)
            {
                mat.SetTexture("_BaseMap", DiffuseMap);
                mat.SetColor("_BaseColor", Color.white);
            }
            else
            {
                mat.SetColor("_BaseColor", Diffuse.GetValueOrDefault(mat.color));
            }

            bool isCutout = OpacityThreshold.HasValue && OpacityThreshold.Value > 0.0f;

            // AlphaCutoff
            if (OpacityThreshold.HasValue)
            {
                mat.SetFloat("_Cutoff", OpacityThreshold.GetValueOrDefault(0.5f));
                if (OpacityThreshold.Value > 0.0f)
                {
                    mat.SetFloat("_AlphaClip", 1);
                    //mat.SetFloat("_AlphaToMask", 1);
                    mat.EnableKeyword("_ALPHATEST_ON");
                    mat.SetOverrideTag("RenderType", "TransparentCutout");
                }
            }

            // Opacity is often set to a default of 1, only treat it as transparent if a map is assigned.
            if (OpacityMap)
            {
                mat.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
                mat.EnableKeyword("_ALPHAPREMULTIPLY_ON");

                if (!isCutout)
                {
                    mat.SetOverrideTag("RenderType", "Transparent");
                    mat.SetFloat("_ZWrite", 0);
                    mat.SetShaderPassEnabled("DepthOnly", false);
                    mat.SetShaderPassEnabled("SHADOWCASTER", false);
                }
            }

            // Smoothness
            // TODO: We could alternatively bake the roughness into the albedo alpha channel. However this should be
            // optional because we shouldn't silently overwrite the albedo alpha channel. USDU-474
            Texture2D metallicGloss = null;
            if (RoughnessMap && !IsSpecularWorkflow && MetallicMap)
            {
                // If we have and are using a metallic map, combine the roughness into the metallic map alpha channel.
                // CombineRoughness also flips rough to smooth.
                metallicGloss = MaterialImporter.CombineRoughness(MetallicMap, RoughnessMap, "metallicGloss");
                mat.EnableKeyword("_METALLICSPECGLOSSMAP");
                mat.SetFloat("_SmoothnessTextureChannel", 0.0f);
            }
            else
            {
                // TODO: In URP we combine a smoothness map with a constant smoothness value, so we should actually keep
                // both. However we only read one or the other from the USD file currently, so settle for this. USDU-474
                var smoothness = 1 - Roughness.GetValueOrDefault();
                mat.SetFloat("_Smoothness", smoothness);
            }

            // Metallic or Specular
            if (!IsSpecularWorkflow)
            {
                if (!MetallicMap)
                {
                    mat.SetFloat("_Metallic", Metallic.GetValueOrDefault());
                }
                else if (metallicGloss)
                {
                    mat.SetTexture("_MetallicGlossMap", metallicGloss);
                }
                else
                {
                    mat.SetTexture("_MetallicGlossMap", MetallicMap);
                }
            }
            else
            {
                if (SpecularMap)
                {
                    mat.SetTexture("_SpecGlossMap", SpecularMap);
                }
                else
                {
                    mat.SetColor("_SpecColor", Specular.GetValueOrDefault());
                }
            }

            // SpecularHighlights - Ignored

            // NormalMap
            if (NormalMap)
            {
                mat.SetTexture("_BumpMap", NormalMap);
                mat.EnableKeyword("_NORMALMAP");
            }

            // Parallax - ??
            if (DisplacementMap)
            {
                mat.SetTexture("_ParallaxMap", DisplacementMap);
                mat.EnableKeyword("_PARALLAXMAP");
            }

            // Occlusion
            if (OcclusionMap)
            {
                mat.SetTexture("_OcclusionMap", OcclusionMap);
                mat.EnableKeyword("_OCCLUSIONMAP");
                // Is _OcclusionStrength required?
            }

            // Emission
            if (EmissionMap)
            {
                mat.SetTexture("_EmissionMap", EmissionMap);
                mat.SetColor("_EmissionColor", Color.white * 1000);
                mat.globalIlluminationFlags = MaterialGlobalIlluminationFlags.BakedEmissive;
                mat.EnableKeyword("_EMISSIVE_COLOR_MAP");
            }
            else
            {
                mat.SetColor("_EmissionColor", Emission.GetValueOrDefault());
            }

            // DetailMask - ignoring for now

            // ClearCoat - not used in Lit but keep for compatibility or something
            /*
            if (ClearcoatMap)
            {
                mat.SetTexture("_CoatMaskMap", ClearcoatMap);
                mat.EnableKeyword("_MATERIAL_FEATURE_CLEAR_COAT");
            }

            mat.SetFloat("_CoatMask", ClearcoatRoughness.GetValueOrDefault());
            */

            // Remove compile errors when URP package not installed
#if URP_AVAILABLE
            UnityEditor.BaseShaderGUI.SetupMaterialBlendMode(mat);
#endif
        }
    }
}
