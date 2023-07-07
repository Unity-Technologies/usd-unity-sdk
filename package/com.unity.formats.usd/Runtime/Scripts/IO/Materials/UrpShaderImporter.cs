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

namespace Unity.Formats.USD
{
    public class UrpShaderImporter : ShaderImporterBase
    {
        private static Material ChannelCombinerMat;

        public UrpShaderImporter(Material material) : base(material)
        {
        }

        // Based on HdrpShaderImporter, reordered to the same order as URP's Lit.shader
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

            // AlphaCutoff - ignored

            // Smoothness
            // TODO: Roughness/ Smoothness map in URP Lit can be the metallic or albedo map alpha channel.
            // For now just set the float value.
            var smoothness = 1 - Roughness.GetValueOrDefault();
            mat.SetFloat("_Smoothness", smoothness);

            // HACK: Let's reuse the MaskMapPacker to pack the Metallic and Smoothness maps into a single texture
            // No- still need to invert the Roughness texture to make it a smoothness texture. Nevermind. // TODO
            /*
            if (MetallicMap)
            {
                var newMetallicTex = BuildMaskMap(MetallicMap, MetallicMap, MetallicMap, RoughnessMap);
                MetallicMap = newMetallicTex;
            }
            */

            // Metallic or Specular
            if (!IsSpecularWorkflow)
            {
                if (!MetallicMap)
                {
                    mat.SetFloat("_Metallic", Metallic.GetValueOrDefault());
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


            // DetailMask
            // VRC: This works, but I'm not sure the use of it yet
            /*
            // R=Metallic, G=Occlusion, B=Displacement, A=Roughness(Smoothness)
            var MaskMap = BuildMaskMap(!IsSpecularWorkflow ? MetallicMap : null, OcclusionMap, DisplacementMap,
                RoughnessMap);
            if (MaskMap)
            {
                mat.SetTexture("_DetailMask", MaskMap);
                mat.EnableKeyword("_MASKMAP");
            }
            */

            // ClearCoat - not used in Lit but keep for compatibility or something
            /*
            if (ClearcoatMap)
            {
                mat.SetTexture("_CoatMaskMap", ClearcoatMap);
                mat.EnableKeyword("_MATERIAL_FEATURE_CLEAR_COAT");
            }

            mat.SetFloat("_CoatMask", ClearcoatRoughness.GetValueOrDefault());
            */
        }

        private static Texture2D BuildMaskMap(Texture2D red, Texture2D green, Texture2D blue, Texture2D alpha)
        {
            var maxW = Mathf.Max(red ? red.width : 0, green ? green.width : 0);
            maxW = Mathf.Max(maxW, blue ? blue.width : 0);
            maxW = Mathf.Max(maxW, alpha ? alpha.width : 0);

            var maxH = Mathf.Max(red ? red.height : 0, green ? green.height : 0);
            maxH = Mathf.Max(maxH, blue ? blue.height : 0);
            maxH = Mathf.Max(maxH, alpha ? alpha.height : 0);

            if (maxH == 0 || maxW == 0)
            {
                return null;
            }

            var tmp = RenderTexture.GetTemporary(maxW, maxH, 0, RenderTextureFormat.ARGBFloat,
                RenderTextureReadWrite.Linear);

            if (!ChannelCombinerMat)
            {
                ChannelCombinerMat = new Material(Shader.Find("Hidden/USD/ChannelCombiner"));
            }

            var newTex = new Texture2D(maxW, maxH, TextureFormat.ARGB32, true, true);
            ChannelCombinerMat.SetVector("_Invert", new Vector4(0, 0, 0, 1));
            ChannelCombinerMat.SetTexture("_R", red ? red : Texture2D.blackTexture);
            ChannelCombinerMat.SetVector("_RScale", new Vector4(1, 0, 0, 0));
            ChannelCombinerMat.SetTexture("_G", green ? green : Texture2D.blackTexture);
            ChannelCombinerMat.SetVector("_GScale", new Vector4(1, 0, 0, 0));
            ChannelCombinerMat.SetTexture("_B", blue ? blue : Texture2D.blackTexture);
            ChannelCombinerMat.SetVector("_BScale", new Vector4(1, 0, 0, 0));
            ChannelCombinerMat.SetTexture("_A", alpha ? alpha : Texture2D.blackTexture);
            ChannelCombinerMat.SetVector("_AScale", new Vector4(1, 0, 0, 0));
            Graphics.Blit(red, tmp, ChannelCombinerMat);

            RenderTexture.active = tmp;
            newTex.ReadPixels(new Rect(0, 0, tmp.width, tmp.height), 0, 0);
            newTex.Apply();
            RenderTexture.ReleaseTemporary(tmp);

#if UNITY_EDITOR
            var newAssetPath = UnityEditor.AssetDatabase.GenerateUniqueAssetPath("Assets/maskMap.png");
            var bytes = newTex.EncodeToPNG();
            Debug.Log(newAssetPath);
            System.IO.File.WriteAllBytes(newAssetPath, bytes);
            UnityEditor.AssetDatabase.ImportAsset(newAssetPath);
            var texImporter = (UnityEditor.TextureImporter)UnityEditor.AssetImporter.GetAtPath(newAssetPath);
            UnityEditor.EditorUtility.SetDirty(texImporter);
            texImporter.SaveAndReimport();
#endif
            // To get the correct file ID, the texture must be reloaded from the asset path.
            Texture2D.DestroyImmediate(newTex);
#if UNITY_EDITOR
            return (Texture2D)UnityEditor.AssetDatabase.LoadAssetAtPath(newAssetPath, typeof(Texture2D));
#else
            return null;
#endif
        }
    }
}
