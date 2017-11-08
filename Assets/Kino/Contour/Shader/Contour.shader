//
// KinoContour - Contour line effect
//
// Copyright (C) 2015 Keijiro Takahashi
//
// Permission is hereby granted, free of charge, to any person obtaining a copy of
// this software and associated documentation files (the "Software"), to deal in
// the Software without restriction, including without limitation the rights to
// use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of
// the Software, and to permit persons to whom the Software is furnished to do so,
// subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS
// FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR
// COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER
// IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN
// CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//
Shader "Hidden/Kino/Contour"
{
    Properties
    {
        _MainTex ("-", 2D) = "" {}
        _Color ("-", Color) = (0, 0, 0, 1)
        _BgColor ("-", Color) = (1, 1, 1, 0)
    }

    CGINCLUDE

    #pragma multi_compile _ USE_DEPTH
    #pragma multi_compile _ USE_NORMAL

    #include "UnityCG.cginc"

    sampler2D _MainTex;
    float2 _MainTex_TexelSize;

    sampler2D_float _CameraDepthTexture;
    sampler2D _CameraGBufferTexture2;

    half4 _Color;
    half4 _BgColor;

    float _LowThreshold;
    float _HighThreshold;

    float _DepthSensitivity;
    float _NormalSensitivity;

    float _FallOffDepth;

    half4 frag(v2f_img i) : SV_Target
    {
        float4 disp = float4(_MainTex_TexelSize.xy, -_MainTex_TexelSize.x, 0);

        // four sample points for the roberts cross operator
        float2 uv0 = i.uv;           // TL
        float2 uv1 = i.uv + disp.xy; // BR
        float2 uv2 = i.uv + disp.xw; // TR
        float2 uv3 = i.uv + disp.wy; // BL

        float edge = 0;

        #ifdef USE_DEPTH

        // sample depth values
        float zs0 = SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, uv0);
        float zs1 = SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, uv1);
        float zs2 = SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, uv2);
        float zs3 = SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, uv3);

        // calculate fall-off parameter from the depth of the nearest point
        float zm = min(min(min(zs0, zs1), zs2), zs3);
        float falloff = 1.0 - saturate(LinearEyeDepth(zm) / _FallOffDepth);

        // convert to linear depth value
        float z0 = Linear01Depth(zs0);
        float z1 = Linear01Depth(zs1);
        float z2 = Linear01Depth(zs2);
        float z3 = Linear01Depth(zs3);

        // roberts cross operator
        float zg1 = z1 - z0;
        float zg2 = z3 - z2;
        float zg = sqrt(zg1 * zg1 + zg2 * zg2);

        edge = zg * falloff * _DepthSensitivity / Linear01Depth(zm);

        #endif

        #ifdef USE_NORMAL

        // sample normal vector values from the g-buffer
        float3 n0 = tex2D(_CameraGBufferTexture2, uv0);
        float3 n1 = tex2D(_CameraGBufferTexture2, uv1);
        float3 n2 = tex2D(_CameraGBufferTexture2, uv2);
        float3 n3 = tex2D(_CameraGBufferTexture2, uv3);

        // roberts cross operator
        float3 ng1 = n1 - n0;
        float3 ng2 = n3 - n2;
        float ng = sqrt(dot(ng1, ng1) + dot(ng2, ng2));

        edge = max(edge, ng * _NormalSensitivity);

        #endif

        // thresholding
        edge = saturate((edge - _LowThreshold) / (_HighThreshold - _LowThreshold));

        half4 cs = tex2D(_MainTex, i.uv);
        half3 c0 = lerp(cs.rgb, _BgColor.rgb, _BgColor.a);
        half3 co = lerp(c0, _Color.rgb, edge * _Color.a);
        return half4(co, cs.a);
    }

    ENDCG
    SubShader
    {
        Pass
        {
            ZTest Always Cull Off ZWrite Off
            CGPROGRAM
            #pragma vertex vert_img
            #pragma fragment frag
            #pragma target 3.0
            ENDCG
        }
    }
}
