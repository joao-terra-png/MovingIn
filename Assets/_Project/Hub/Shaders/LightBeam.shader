Shader "MovingIn/LightBeam"
{
    Properties
    {
        _Color ("Color", Color) = (1, 0.9, 0.7, 0.25)
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
                float edge = sin(i.uv.x * 3.14159);
                float fall = pow(saturate(1.0 - i.uv.y), 1.6);
                float shimmer = 0.85 + 0.15 * sin(_Time.y * 1.7 + i.uv.x * 7.0 + i.uv.y * 3.0);
                return fixed4(_Color.rgb, _Color.a * edge * fall * shimmer);
            }
            ENDCG
        }
    }
}
