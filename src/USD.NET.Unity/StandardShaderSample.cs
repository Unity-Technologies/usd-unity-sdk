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

namespace USD.NET.Unity {

  [UsdSchema("Shader")]
  public class StandardShaderSample : ShaderSample {

    [UsdNamespace("inputs"), InputParameter("_Color")]
    public Connectable<Color> albedo = new Connectable<Color>(Color.white);

    [UsdNamespace("inputs"), InputTexture("_MainTex")]
    public Connectable<Color> albedoMap = new Connectable<Color>();

    [UsdNamespace("inputs"), InputParameter("_Cutoff")]
    public Connectable<float> cutoff = new Connectable<float>(1.0f);

    [UsdNamespace("inputs"), InputParameter("_Glossiness")]
    public Connectable<float> smoothness = new Connectable<float>(0.5f);

    [UsdNamespace("inputs"), InputParameter("_GlossMapScale")]
    public Connectable<float> smoothnessScale = new Connectable<float>(0.5f);

    [UsdNamespace("inputs"), InputParameter("_SmoothnessTextureChannel")]
    public Connectable<float> smoothnessTextureChannel = new Connectable<float>();

    [UsdNamespace("inputs"), InputTexture("_MetallicGlossMap"), RequireShaderKeywords("_METALLICGLOSSMAP")]
    public Connectable<float> metallicMap = new Connectable<float>();
    [UsdNamespace("inputs"), InputParameter("_Metallic")]
    public Connectable<float> metallicScale = new Connectable<float>(0.5f);

    // Unity has no notion of a bool, but this will be mapped to a 1/0 float.
    [UsdNamespace("inputs"), InputParameter("_SpecularHighlights")]
    public Connectable<bool> enableSpecularHighlights = new Connectable<bool>();

    [UsdNamespace("inputs"), InputParameter("_GlossyReflections")]
    public Connectable<bool> enableGlossyReflections = new Connectable<bool>();

    [UsdNamespace("inputs"), InputTexture("_BumpMap"), RequireShaderKeywords("_NORMALMAP")]
    public Connectable<Color> normalMap = new Connectable<Color>();
    [UsdNamespace("inputs"), InputParameter("_BumpScale")]
    public Connectable<float> normalMapScale = new Connectable<float>(0.5f);

    [UsdNamespace("inputs"), InputTexture("_ParallaxMap"), RequireShaderKeywords("_PARALLAXMAP")]
    public Connectable<Color> parallaxMap = new Connectable<Color>();
    [UsdNamespace("inputs"), InputParameter("_Parallax")]
    public Connectable<float> parallaxMapScale = new Connectable<float>(0.01f);

    [UsdNamespace("inputs"), InputTexture("_OcclusionMap")]
    public Connectable<float> occlusionMap = new Connectable<float>(0.5f);
    [UsdNamespace("inputs"), InputParameter("_OcclusionStrength")]
    public Connectable<float> occlusionMapScale = new Connectable<float>();

    [UsdNamespace("inputs"), InputParameter("_EmissionColor")]
    public Connectable<Color> emission = new Connectable<Color>(Color.black);

    [UsdNamespace("inputs"), InputTexture( "_EmissionMap"), RequireShaderKeywords("_EMISSION")]
    public Connectable<Color> emissionMap = new Connectable<Color>();

    [UsdNamespace("inputs"), InputTexture("_DetailMask")]
    public Connectable<Color> detailMask = new Connectable<Color>();

    [UsdNamespace("inputs"), InputTexture("_DetailAlbedoMap"), RequireShaderKeywords("_DETAIL_MULX2")]
    public Connectable<Color> detailAlbedoMap = new Connectable<Color>();

    [UsdNamespace("inputs"), InputTexture("_DetailNormalMap"), RequireShaderKeywords("_DETAIL_MULX2")]
    public Connectable<Color> detailNormalMap = new Connectable<Color>();
    [UsdNamespace("inputs"), InputParameter("_DetailNormalMapScale")]
    public Connectable<float> detailNormalMapScale = new Connectable<float>();

    [UsdNamespace("inputs"), InputParameter("_UVSec")]
    public Connectable<float> uvSetForSecondaryTextures = new Connectable<float>();

    [UsdNamespace("inputs"), InputParameter("_Mode")]
    public Connectable<float> renderingMode = new Connectable<float>();

    [UsdNamespace("inputs"), InputParameter("_SrcBlend")]
    public Connectable<float> srcBlend = new Connectable<float>((int)UnityEngine.Rendering.BlendMode.One);

    [UsdNamespace("inputs"), InputParameter("_DstBlend")]
    public Connectable<float> dstBlend = new Connectable<float>((int)UnityEngine.Rendering.BlendMode.Zero);

    [UsdNamespace("inputs"), InputParameter("_ZWrite")]
    public Connectable<float> zwrite = new Connectable<float>(1);
  }

}
