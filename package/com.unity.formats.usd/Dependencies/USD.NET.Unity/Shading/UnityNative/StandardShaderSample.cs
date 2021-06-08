// Copyright 2021 Unity Technologies. All rights reserved.
// Copyright 2017 Google Inc. All rights reserved.
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

namespace USD.NET.Unity
{
    [System.Serializable]
    [UsdSchema("Shader")]
    public class StandardShaderSample : ShaderSample
    {
        public StandardShaderSample()
        {
            id = new pxr.TfToken("Unity.Standard");
        }

        // Note that this is not an input/parameter to be copied, it is a fundamental quality of the
        // shader that must be handled by the importer.
        [UsdNamespace("info")]
        public bool enableGpuInstancing;

        [InputParameter("_Color")]
        public Connectable<Color> albedo = new Connectable<Color>(Color.white);

        [InputTexture("_MainTex")]
        public Connectable<Color> albedoMap = new Connectable<Color>();

        [InputParameter("_Cutoff")]
        public Connectable<float> cutoff = new Connectable<float>(1.0f);

        [InputParameter("_Glossiness")]
        public Connectable<float> smoothness = new Connectable<float>(0.5f);

        [InputParameter("_GlossMapScale")]
        public Connectable<float> smoothnessScale = new Connectable<float>(0.5f);

        [InputParameter("_SmoothnessTextureChannel")]
        public Connectable<float> smoothnessTextureChannel = new Connectable<float>();

        [InputTexture("_MetallicGlossMap"), RequireShaderKeywords("_METALLICGLOSSMAP")]
        public Connectable<float> metallicMap = new Connectable<float>();
        [InputParameter("_Metallic")]
        public Connectable<float> metallicScale = new Connectable<float>(0.5f);

        // Unity has no notion of a bool, but this will be mapped to a 1/0 float.
        [InputParameter("_SpecularHighlights")]
        public Connectable<bool> enableSpecularHighlights = new Connectable<bool>();

        [InputParameter("_GlossyReflections")]
        public Connectable<bool> enableGlossyReflections = new Connectable<bool>();

        [InputTexture("_BumpMap"), RequireShaderKeywords("_NORMALMAP")]
        public Connectable<Color> normalMap = new Connectable<Color>();
        [InputParameter("_BumpScale")]
        public Connectable<float> normalMapScale = new Connectable<float>(0.5f);

        [InputTexture("_ParallaxMap"), RequireShaderKeywords("_PARALLAXMAP")]
        public Connectable<Color> parallaxMap = new Connectable<Color>();
        [InputParameter("_Parallax")]
        public Connectable<float> parallaxMapScale = new Connectable<float>(0.01f);

        [InputTexture("_OcclusionMap")]
        public Connectable<float> occlusionMap = new Connectable<float>(0.5f);
        [InputParameter("_OcclusionStrength")]
        public Connectable<float> occlusionMapScale = new Connectable<float>();

        [InputParameter("_EmissionColor")]
        public Connectable<Color> emission = new Connectable<Color>(Color.black);

        [InputTexture("_EmissionMap"), RequireShaderKeywords("_EMISSION")]
        public Connectable<Color> emissionMap = new Connectable<Color>();

        [InputTexture("_DetailMask")]
        public Connectable<Color> detailMask = new Connectable<Color>();

        [InputTexture("_DetailAlbedoMap"), RequireShaderKeywords("_DETAIL_MULX2")]
        public Connectable<Color> detailAlbedoMap = new Connectable<Color>();

        [InputTexture("_DetailNormalMap"), RequireShaderKeywords("_DETAIL_MULX2")]
        public Connectable<Color> detailNormalMap = new Connectable<Color>();
        [InputParameter("_DetailNormalMapScale")]
        public Connectable<float> detailNormalMapScale = new Connectable<float>();

        [InputParameter("_UVSec")]
        public Connectable<float> uvSetForSecondaryTextures = new Connectable<float>();

        [InputParameter("_Mode")]
        public Connectable<float> renderingMode = new Connectable<float>();

        [InputParameter("_SrcBlend")]
        public Connectable<float> srcBlend = new Connectable<float>((int)UnityEngine.Rendering.BlendMode.One);

        [InputParameter("_DstBlend")]
        public Connectable<float> dstBlend = new Connectable<float>((int)UnityEngine.Rendering.BlendMode.Zero);

        [InputParameter("_ZWrite")]
        public Connectable<float> zwrite = new Connectable<float>(1);
    }
}
