Shader "Hidden/USD/NormalChannel"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
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

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            sampler2D _MainTex;

            // from https://forum.unity.com/threads/runtime-generated-bump-maps-are-not-marked-as-normal-maps.413778/#post-4935776
            // Unpack normal as DXT5nm (1, y, 1, x) or BC5 (x, y, 0, 1)
            // Note neutral texture like "bump" is (0, 0, 1, 1) to work with both plain RGB normal and DXT5nm/BC5
            /*
            fixed3 UnpackNormalmapRGorAG(fixed4 packednormal)
            {
                // This do the trick
                packednormal.x *= packednormal.w;
                fixed3 normal;
                normal.xy = packednormal.xy * 2 - 1;
                normal.z = sqrt(1 - saturate(dot(normal.xy, normal.xy)));
                return normal;
            }
            */

            // from https://forum.unity.com/threads/runtime-generated-bump-maps-are-not-marked-as-normal-maps.413778/#post-5424156
            // float4 DTXnm2RGBA(float4 col)
            // {
            //  float4 c = col;
            //  c.r = c.a * 2 - 1;  //red<-alpha (x<-w)
            //  c.g = c.g * 2 - 1; //green is always the same (y)
            //  float2 xy = float2(c.r, c.g); //this is the xy vector, can be written also just "c.xy"
            //  c.b = sqrt(1 - clamp(dot(xy, xy), 0, 1)); //recalculate the blue channel (z)
            //  return float4(c.r * 0.5f + 0.5f, c.g * 0.5f + 0.5f, c.b * 0.5f + 0.5f, 1); //back to 0-1 range
            // }

            float4 frag (v2f i) : SV_Target
            {
                float4 col = tex2D(_MainTex, i.uv);
                // If a texture is marked as a normal map
                // the values are stored in the A and G channel.
                return float4(UnpackNormalmapRGorAG(col) * 0.5f + 0.5f, 1);
            }

            ENDCG
        }
    }
}
