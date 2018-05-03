Shader "Hidden/Halftone"
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

            #pragma vertex vert_img
            #pragma fragment frag
            
            #include "UnityCG.cginc"
            
            sampler2D _MainTex;

            float _pointSize;
            float _frequency;
            float3 _sines;
            float3 _cosines;
            float _mono;
            float _invert;

            float gradLen(float value)
            {
                return length(float2(ddx(value), ddy(value)));
            }

            float3 aastep(float3 threshold, float3 value) 
            {
                float3 gl = 0.7 * float3(gradLen(value.x), gradLen(value.y), gradLen(value.z));
                return smoothstep(threshold - gl, threshold + gl, value);
            }

            fixed4 frag(v2f_img i) : SV_Target
            {
                float3 src = tex2D(_MainTex, i.uv).rgb;

                src = lerp(src, Luminance(src), _mono);

                src = lerp(src, 1.0 - src, _invert);
             
                float2 uv = (i.uv - 0.5);
                uv.x *= _ScreenParams.x / _ScreenParams.y;

                float2 xuv = mul(float2x2(_cosines.x, -_sines.x, _sines.x, _cosines.x), uv);
                float2 yuv = mul(float2x2(_cosines.y, -_sines.y, _sines.y, _cosines.y), uv);
                float2 zuv = mul(float2x2(_cosines.z, -_sines.z, _sines.z, _cosines.z), uv);

                float3 uvLen;
                uvLen.x = length(2.0 * frac(_frequency * xuv) - 1.0);
                uvLen.y = length(2.0 * frac(_frequency * yuv) - 1.0);
                uvLen.z = length(2.0 * frac(_frequency * zuv) - 1.0);

                src = aastep(0.0, sqrt(src) * _pointSize - uvLen);

                src = lerp(src, float3(src.r, src.r, src.r), _mono);

                src = lerp(src, 1.0 - src, _invert);

                return fixed4(src, 1.0);
            }

            ENDCG
        }
    }
}
