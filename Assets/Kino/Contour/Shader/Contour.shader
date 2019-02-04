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
        _MainTex("", 2D) = "" {}
    }

    CGINCLUDE

    #include "UnityCG.cginc"

    sampler2D _MainTex;
    float2 _MainTex_TexelSize;

    sampler2D_float _CameraDepthTexture;
    sampler2D _CameraGBufferTexture2;

    half4 _Color;
    half4 _Background;

    half _Threshold;
    float _InvRange;

    half _ColorSensitivity;
    half _DepthSensitivity;
    half _NormalSensitivity;
    float _InvFallOff;

    half4 frag(v2f_img i) : SV_Target
    {
        // Source color
        half4 c0 = tex2D(_MainTex, i.uv);

        // Four sample points of the roberts cross operator
        float2 uv0 = i.uv;                                   // TL
        float2 uv1 = i.uv + _MainTex_TexelSize.xy;           // BR
        float2 uv2 = i.uv + float2(_MainTex_TexelSize.x, 0); // TR
        float2 uv3 = i.uv + float2(0, _MainTex_TexelSize.y); // BL

        half edge = 0;

    #ifdef _CONTOUR_COLOR

        // Color samples
        float3 c1 = tex2D(_MainTex, uv1).rgb;
        float3 c2 = tex2D(_MainTex, uv2).rgb;
        float3 c3 = tex2D(_MainTex, uv3).rgb;

        // Roberts cross operator
        float3 cg1 = c1 - c0;
        float3 cg2 = c3 - c2;
        float cg = sqrt(dot(cg1, cg1) + dot(cg2, cg2));

        edge = cg * _ColorSensitivity;

    #endif

    #ifdef _CONTOUR_DEPTH

        // Depth samples
        float zs0 = SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, uv0);
        float zs1 = SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, uv1);
        float zs2 = SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, uv2);
        float zs3 = SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, uv3);

        // Calculate fall-off parameter from the depth of the nearest point
        float zm = min(min(min(zs0, zs1), zs2), zs3);
        float falloff = 1.0 - saturate(LinearEyeDepth(zm) * _InvFallOff);

        // Convert to linear depth values.
        float z0 = Linear01Depth(zs0);
        float z1 = Linear01Depth(zs1);
        float z2 = Linear01Depth(zs2);
        float z3 = Linear01Depth(zs3);

        // Roberts cross operator
        float zg1 = z1 - z0;
        float zg2 = z3 - z2;
        float zg = sqrt(zg1 * zg1 + zg2 * zg2);

        edge = max(edge, zg * falloff * _DepthSensitivity / Linear01Depth(zm));

    #endif

    #ifdef _CONTOUR_NORMAL

        // Normal samples from the G-buffer
        float3 n0 = tex2D(_CameraGBufferTexture2, uv0).rgb;
        float3 n1 = tex2D(_CameraGBufferTexture2, uv1).rgb;
        float3 n2 = tex2D(_CameraGBufferTexture2, uv2).rgb;
        float3 n3 = tex2D(_CameraGBufferTexture2, uv3).rgb;

        // Roberts cross operator
        float3 ng1 = n1 - n0;
        float3 ng2 = n3 - n2;
        float ng = sqrt(dot(ng1, ng1) + dot(ng2, ng2));

        edge = max(edge, ng * _NormalSensitivity);

    #endif

        // Thresholding
        edge = saturate((edge - _Threshold) * _InvRange);

        half3 cb = lerp(c0.rgb, _Background.rgb, _Background.a);
        half3 co = lerp(cb, _Color.rgb, edge * _Color.a);
        return half4(co, c0.a);
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
            #pragma multi_compile _ _CONTOUR_COLOR
            #pragma multi_compile _ _CONTOUR_DEPTH
            #pragma multi_compile _ _CONTOUR_NORMAL
            ENDCG
        }
    }
}
