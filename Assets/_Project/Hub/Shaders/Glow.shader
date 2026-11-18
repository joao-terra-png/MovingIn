Shader "MovingIn/Glow"
{
    Properties
    {
        _Color ("Color", Color) = (1, 0.8, 0.5, 0.8)
    }
    SubShader
    {
        Tags { "Queue" = "Transparent" "RenderType" = "Transparent" "IgnoreProjector" = "True" }
        Pass
        {
            Blend SrcAlpha One
            ZWrite Off
            Cull Off
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            fixed4 _Color;

            struct v2f
            {
                float4 pos : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            v2f vert (appdata_base v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = v.texcoord.xy;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float d = saturate(1.0 - distance(i.uv, float2(0.5, 0.5)) * 2.0);
                return fixed4(_Color.rgb, _Color.a * d * d);
            }
            ENDCG
        }
    }
}
