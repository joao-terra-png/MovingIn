Shader "MovingIn/ScreenGrade"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Tint ("Tint", Color) = (1, 1, 1, 1)
        _Saturation ("Saturation", Float) = 1
        _Contrast ("Contrast", Float) = 1
        _Vignette ("Vignette", Float) = 0.3
    }
    SubShader
    {
        Cull Off
        ZWrite Off
        ZTest Always
        Pass
        {
            CGPROGRAM
            #pragma vertex vert_img
            #pragma fragment frag
            #include "UnityCG.cginc"

            sampler2D _MainTex;
            fixed4 _Tint;
            float _Saturation;
            float _Contrast;
            float _Vignette;

            fixed4 frag (v2f_img i) : SV_Target
            {
                fixed4 c = tex2D(_MainTex, i.uv);
                float lum = dot(c.rgb, float3(0.299, 0.587, 0.114));
                c.rgb = lerp(lum.xxx, c.rgb, _Saturation);
                c.rgb = (c.rgb - 0.5) * _Contrast + 0.5;
                c.rgb *= _Tint.rgb;
                float d = distance(i.uv, float2(0.5, 0.5)) * 1.414;
                c.rgb *= 1.0 - _Vignette * smoothstep(0.45, 1.0, d);
                return c;
            }
            ENDCG
        }
    }
}
