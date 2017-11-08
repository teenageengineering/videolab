//
// KinoIsoline - Isoline effect
//
// Copyright (C) 2015 Keijiro Takahashi
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
//
Shader "Hidden/Kino/Isoline"
{
    Properties
    {
        _MainTex ("-", 2D) = ""{}
        _Color ("-", Color) = (0, 0, 0, 1)
        _BgColor ("-", Color) = (1, 1, 1, 0)
    }

    CGINCLUDE

    #pragma multi_compile _ DISTORTION
    #pragma multi_compile _ MODULATION_FRAC MODULATION_SIN MODULATION_NOISE

    #include "UnityCG.cginc"

    #if DISTORTION || MODULATION_NOISE
    #include "SimplexNoise3D.cginc"
    #endif

    sampler2D _MainTex;
    float2 _MainTex_TexelSize;

    sampler2D_float _CameraDepthTexture;

    float4x4 _InverseView;

    half4 _Color;
    half _Blend;
    float _FallOffDepth;
    half4 _BgColor;

    float3 _Axis;
    float _Density;
    float3 _Offset;

    float _DistFreq;
    float _DistAmp;

    float3 _ModAxis;
    float _ModFreq;
    float _ModTime;
    float _ModExp;

    float GetPotential(float3 wpos)
    {
        wpos += _Offset;
        #if DISTORTION
        wpos += snoise(wpos * _DistFreq) * _DistAmp;
        #endif
        return dot(wpos, _Axis) * _Density;
    }

    fixed4 frag (v2f_img i) : SV_Target
    {
        float4 disp = float4(_MainTex_TexelSize.xy, -_MainTex_TexelSize.x, 0);

        // four sample points for the roberts cross operator
        float2 uv0 = i.uv;           // TL
        float2 uv1 = i.uv + disp.xy; // BR
        float2 uv2 = i.uv + disp.xw; // TR
        float2 uv3 = i.uv + disp.wy; // BL

        // sample depth values
        float z0 = LinearEyeDepth(SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, uv0));
        float z1 = LinearEyeDepth(SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, uv1));
        float z2 = LinearEyeDepth(SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, uv2));
        float z3 = LinearEyeDepth(SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, uv3));

        // calculate fall-off parameter from the depth of the nearest point
        float zm = min(min(min(z0, z1), z2), z3);
        float falloff = 1.0 - saturate(zm / _FallOffDepth);

        // convert to world space position
        float2 p11_22 = float2(unity_CameraProjection._11, unity_CameraProjection._22);

        float3 wp0 = float3((uv0 * 2 - 1) / p11_22, -1) * z0;
        float3 wp1 = float3((uv1 * 2 - 1) / p11_22, -1) * z1;
        float3 wp2 = float3((uv2 * 2 - 1) / p11_22, -1) * z2;
        float3 wp3 = float3((uv3 * 2 - 1) / p11_22, -1) * z3;

        wp0 = mul(_InverseView, float4(wp0, 1)).xyz;
        wp1 = mul(_InverseView, float4(wp1, 1)).xyz;
        wp2 = mul(_InverseView, float4(wp2, 1)).xyz;
        wp3 = mul(_InverseView, float4(wp3, 1)).xyz;

        // calculate potential values
        float p0 = GetPotential(wp0);
        float p1 = GetPotential(wp1);
        float p2 = GetPotential(wp2);
        float p3 = GetPotential(wp3);

        // roberts cross operator
        float3 g1 = frac(p1) - frac(p0);
        float3 g2 = frac(p3) - frac(p2);
        float g = dot(g1, g1) + dot(g2, g2) > 1.4;
        g *= saturate(1.0 - abs(p1 - p0) - abs(p3 - p2));

        // line modulation
        #if MODULATION_NOISE
        float mp = snoise((wp0 - _ModAxis * _ModTime) * _ModFreq);
        g *= pow(saturate(0.5 + mp), _ModExp);
        #elif MODULATION_FRAC || MODULATION_SIN
        float mp = (dot(wp0, _ModAxis) - _ModTime) * _ModFreq;
        #if MODULATION_FRAC
        g *= pow(frac(mp), _ModExp);
        #else // MODULATION_SIN
        g *= pow((sin(mp) + 1) / 2, _ModExp);
        #endif
        #endif

        // blending
        half4 cs = tex2D(_MainTex, i.uv);
        half3 cb = lerp(cs.rgb, _BgColor.rgb, _BgColor.a);
        half3 cl = _Color.rgb * lerp(1, max(Luminance(cs), 0), _Blend);
        half3 co = lerp(cb, cl, saturate(g * falloff * _Color.a));
        return half4(co, cs.a);
    }

    ENDCG

    SubShader
    {
        Cull Off ZWrite Off ZTest Always
        Pass
        {
            CGPROGRAM
            #pragma vertex vert_img
            #pragma fragment frag
            #pragma target 3.0
            ENDCG
        }
    }
}
