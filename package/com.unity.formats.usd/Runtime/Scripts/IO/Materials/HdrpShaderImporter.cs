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
    public class HdrpShaderImporter : ShaderImporterBase
    {
        private static Material ChannelCombinerMat;

        public HdrpShaderImporter(Material material) : base(material)
        {
        }

        public override void ImportFromUsd()
        {
            Material mat = Material;

            if (DiffuseMap)
            {
                Debug.Log("here");
                mat.SetTexture("_BaseColorMap", DiffuseMap);
                mat.SetColor("_BaseColor", Color.white);
            }
            else
            {
                mat.SetColor("_BaseColor", Diffuse.GetValueOrDefault(mat.color));
            }

            // TODO: What about opacity map?

            if (!IsSpecularWorkflow)
            {
                // Robustness: It would be ideal if this parameter were provided by HDRP, however that
                // would require this asset package having a dependency on the HDRP package itself,
                // which is (yet) not desirable.
                mat.SetFloat("_MaterialID", /*Standard Metallic*/ 1);
            }
            else
            {
                mat.SetFloat("_MaterialID", /*Spec Color*/ 4);
                mat.EnableKeyword("_MATERIAL_FEATURE_SPECULAR_COLOR");
                mat.EnableKeyword("_SPECULARCOLORMAP");
            }

            // R=Metallic, G=Occlusion, B=Displacement, A=Roughness(Smoothness)
            var MaskMap = BuildMaskMap(!IsSpecularWorkflow ? MetallicMap : null, OcclusionMap, DisplacementMap,
                RoughnessMap);
            if (MaskMap)
            {
                mat.SetTexture("_MaskMap", MaskMap);
                mat.EnableKeyword("_MASKMAP");
            }

            if (!IsSpecularWorkflow)
            {
                if (!MetallicMap)
                {
                    mat.SetFloat("_Metallic", Metallic.GetValueOrDefault());
                }
            }
            else
            {
                if (SpecularMap)
                {
                    mat.SetTexture("_SpecularColorMap", SpecularMap);
                }
                else
                {
                    mat.SetColor("_SpecularColor", Specular.GetValueOrDefault());
                }
            }

            if (!RoughnessMap)
            {
                var smoothness = 1 - Roughness.GetValueOrDefault();
                mat.SetFloat("_Smoothness", smoothness);
                // HDRP Lit does not seem to respect smoothness, so just clamp to the correct value.
                mat.SetFloat("_SmoothnessRemapMin", smoothness);
                mat.SetFloat("_SmoothnessRemapMax", smoothness);
            }

            if (!OcclusionMap)
            {
                mat.SetFloat("_AORemapMin", Occlusion.GetValueOrDefault());
                mat.SetFloat("_AORemapMax", Occlusion.GetValueOrDefault());
            }

            // Single displacement scalar value not supported.

            if (ClearcoatMap)
            {
                mat.SetTexture("_CoatMaskMap", ClearcoatMap);
                mat.EnableKeyword("_MATERIAL_FEATURE_CLEAR_COAT");
            }

            mat.SetFloat("_CoatMask", ClearcoatRoughness.GetValueOrDefault());

            if (NormalMap)
            {
                mat.SetTexture("_NormalMap", NormalMap);
                mat.EnableKeyword("_NORMALMAP");
            }

            if (EmissionMap)
            {
                mat.SetTexture("_EmissiveColorMap", EmissionMap);
                mat.SetColor("_EmissiveColor", Color.white * 1000);
                mat.SetColor("_EmissiveColorLDR", Color.white);
                mat.globalIlluminationFlags = MaterialGlobalIlluminationFlags.BakedEmissive;
                mat.EnableKeyword("_EMISSIVE_COLOR_MAP");
            }
            else
            {
                mat.SetColor("_EmissionColor", Emission.GetValueOrDefault());
            }
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
