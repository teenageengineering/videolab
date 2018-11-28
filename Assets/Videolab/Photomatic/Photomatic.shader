Shader "Hidden/Photomatic"
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

            float4 _hsbc;
            float4 _fx;
            float4 _colorMask;

            float3 applyHue(float3 aColor, float aHue)
            {
                float angle = radians(aHue);
                float3 k = float3(0.57735, 0.57735, 0.57735);
                float cosAngle = cos(angle);
                //Rodrigues' rotation formula
                return aColor * cosAngle + cross(k, aColor) * sin(angle) + k * dot(k, aColor) * (1 - cosAngle);
            }

            fixed4 frag(v2f_img i) : SV_Target
            {
                float2 pivot = float2(0.5f, 0.5f);
                float2 uv = (i.uv - pivot) / _fx.w + pivot;
                uv = lerp(uv, 1.0 - uv, _fx.xy);
                float3 src = tex2D(_MainTex, uv).rgb;

                float hue = 360 * _hsbc.x;
                float saturation = _hsbc.y * 2;
                float brightness = _hsbc.z * 2 - 1;
                float contrast = _hsbc.w * 2;

                src = applyHue(src, hue);
                src = (src - 0.5f) * contrast + 0.5f;
                src = src + brightness;        
                float3 intensity = dot(src, float3(0.299,0.587,0.114));
                src = lerp(intensity, src, saturation);
                src = lerp(src, 1.0 - src, _fx.z);
                src = src * _colorMask.rgb;

                return fixed4(src, 1.0);
            }

            ENDCG
        }
    }
}
