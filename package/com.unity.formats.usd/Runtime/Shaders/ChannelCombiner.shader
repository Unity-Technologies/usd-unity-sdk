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

      float4 _RScale;
      float4 _GScale;
      float4 _BScale;
      float4 _AScale;
      float4 _Invert;

      float4 frag(v2f i) : SV_Target
      {
        // _[X]Scale values drive how channels are combined;
        // usually you'll want to just set the channel you're interested from as 1 and the rest to 0.
        // so if you want to have _R.g as output, use _Rscale = float4(0,1,0,0) to only have that
        // if you don't have an alpha texture, make sure to set _Invert.a = 1 so we end up with a white alpha.

        float r = dot(tex2D(_R, i.uv), _RScale);
        float g = dot(tex2D(_G, i.uv), _GScale);
        float b = dot(tex2D(_B, i.uv), _BScale);
        float a = dot(tex2D(_A, i.uv), _AScale);

        float4 c = float4(r,g,b,a);
        c = lerp(c, 1 - c, _Invert);

        return c;
      }
    ENDCG
  }
  }
}
