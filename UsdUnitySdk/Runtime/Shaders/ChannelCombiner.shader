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

Shader "Hidden/USD/ChannelCombiner"
{
  //
  // Creates a new texture with inputs packed into RGBA.
  //
  Properties
  {
    _R("Red", 2D) = "black" {}
    _G("Green", 2D) = "black" {}
    _B("Blue", 2D) = "black" {}
    _A("Alpha", 2D) = "black" {}
  }

  SubShader
  {
    // No culling or depth
    Cull Off ZWrite Off ZTest Always

    Pass
    {
      CGPROGRAM
      #pragma vertex vert
      #pragma fragment frag

      #include "UnityCG.cginc"

      struct appdata
      {
        float4 vertex : POSITION;
        float2 uv : TEXCOORD0;
      };

      struct v2f
      {
        float2 uv : TEXCOORD0;
        float4 vertex : SV_POSITION;
      };

      v2f vert(appdata v)
      {
        v2f o;
        o.vertex = UnityObjectToClipPos(v.vertex);
        o.uv = v.uv;
        return o;
      }

      sampler2D _R;
      sampler2D _G;
      sampler2D _B;
      sampler2D _A;

      bool _InvertAlpha;

      float4 frag(v2f i) : SV_Target
      {
        // All input textures are assumed to be single channel.
        //
        // This is a limitation of the current implementation and may need to be extended to
        // support USD driving the exact channel, in the event that the textures are already
        // packed in the USD representation.
        
        float r = tex2D(_R, i.uv).r;
        float g = tex2D(_G, i.uv).r;
        float b = tex2D(_B, i.uv).r;
        float a = tex2D(_A, i.uv).r;

        a = lerp(a, 1 - a, _InvertAlpha);

        return float4(r, g, b, a);
      }
    ENDCG
  }
  }
}
