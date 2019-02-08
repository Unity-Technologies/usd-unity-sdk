Shader "Hidden/USD/CombineAndConvertRoughness"
{
  Properties
  {
    _MainTex("Texture", 2D) = "white" {}
    _RoughnessTex("Texture", 2D) = "white" {}
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

      sampler2D _MainTex;
      sampler2D _RoughnessTex;

      fixed4 frag(v2f i) : SV_Target
      {
        // Use RGB from the main texture and set A = R of the second texture
        // inverted, to convert roughness into gloss.
        fixed4 col = tex2D(_MainTex, i.uv);
        col.a = 1 - tex2D(_RoughnessTex, i.uv).r;
        return col;
      }
    ENDCG
  }
  }
}
