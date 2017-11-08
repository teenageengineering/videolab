//
// Kino/Obscurance - Screen space ambient obscurance image effect
//
// Copyright (C) 2016 Keijiro Takahashi
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

#include "Common.cginc"

// Trigonometric function utility
float2 CosSin(float theta)
{
    float sn, cs;
    sincos(theta, sn, cs);
    return float2(cs, sn);
}

// Pseudo random number generator with 2D coordinates
float UVRandom(float u, float v)
{
    float f = dot(float2(12.9898, 78.233), float2(u, v));
    return frac(43758.5453 * sin(f));
}

// Interleaved gradient function from Jimenez 2014 http://goo.gl/eomGso
float GradientNoise(float2 uv)
{
    uv = floor(uv * _ScreenParams.xy);
    float f = dot(float2(0.06711056f, 0.00583715f), uv);
    return frac(52.9829189f * frac(f));
}

// Check if the camera is perspective.
// (returns 1.0 when orthographic)
float CheckPerspective(float x)
{
    return lerp(x, 1, unity_OrthoParams.w);
}

// Reconstruct view-space position from UV and depth.
// p11_22 = (unity_CameraProjection._11, unity_CameraProjection._22)
// p13_31 = (unity_CameraProjection._13, unity_CameraProjection._23)
float3 ReconstructViewPos(float2 uv, float depth, float2 p11_22, float2 p13_31)
{
    return float3((uv * 2 - 1 - p13_31) / p11_22 * CheckPerspective(depth), depth);
}

// Sample point picker
float3 PickSamplePoint(float2 uv, float index)
{
    // Uniformaly distributed points on a unit sphere http://goo.gl/X2F1Ho
#if defined(FIX_SAMPLING_PATTERN)
    float gn = GradientNoise(uv * _Downsample);
    float u = frac(UVRandom(0, index) + gn) * 2 - 1;
    float theta = (UVRandom(1, index) + gn) * UNITY_PI * 2;
#else
    float u = UVRandom(uv.x + _Time.x, uv.y + index) * 2 - 1;
    float theta = UVRandom(-uv.x - _Time.x, uv.y + index) * UNITY_PI * 2;
#endif
    float3 v = float3(CosSin(theta) * sqrt(1 - u * u), u);
    // Make them distributed between [0, _Radius]
    float l = sqrt((index + 1) / _SampleCount) * _Radius;
    return v * l;
}

//
// Distance-based AO estimator based on Morgan 2011 http://goo.gl/2iz3P
//
half4 frag_ao(v2f i) : SV_Target
{
    float2 uv = i.uvAlt;
    float2 uv01 = i.uv01;

    // Parameters used in coordinate conversion
    float3x3 proj = (float3x3)unity_CameraProjection;
    float2 p11_22 = float2(unity_CameraProjection._11, unity_CameraProjection._22);
    float2 p13_31 = float2(unity_CameraProjection._13, unity_CameraProjection._23);

    // View space normal and depth
    float3 norm_o;
    float depth_o = SampleDepthNormal(uv, norm_o);

#if defined(SOURCE_DEPTHNORMALS)
    // Offset the depth value to avoid precision error.
    // (depth in the DepthNormals mode has only 16-bit precision)
    depth_o -= _ProjectionParams.z / 65536;
#endif

    // Reconstruct the view-space position.
    float3 vpos_o = ReconstructViewPos(uv01, depth_o, p11_22, p13_31);

    float ao = 0.0;

    for (int s = 0; s < _SampleCount; s++)
    {
        // Sample point
#if defined(SHADER_API_D3D11)
        // This 'floor(1.0001 * s)' operation is needed to avoid a NVidia
        // shader issue. This issue is only observed on DX11.
        float3 v_s1 = PickSamplePoint(uv, floor(1.0001 * s));
#else
        float3 v_s1 = PickSamplePoint(uv, s);
#endif
        v_s1 = faceforward(v_s1, -norm_o, v_s1);
        float3 vpos_s1 = vpos_o + v_s1;

        // Reproject the sample point
        float3 spos_s1 = mul(proj, vpos_s1);
        float2 uv_s1_01 = (spos_s1.xy / CheckPerspective(vpos_s1.z) + 1) * 0.5;
#if defined(UNITY_SINGLE_PASS_STEREO)
        float2 uv_s1 = UnityStereoScreenSpaceUVAdjust(uv_s1_01, _MainTex_ST);
#else
        float2 uv_s1 = uv_s1_01;
#endif

        // Depth at the sample point
        float depth_s1 = SampleDepth(uv_s1);

        // Relative position of the sample point
        float3 vpos_s2 = ReconstructViewPos(uv_s1_01, depth_s1, p11_22, p13_31);
        float3 v_s2 = vpos_s2 - vpos_o;

        // Estimate the obscurance value
        float a1 = max(dot(v_s2, norm_o) - kBeta * depth_o, 0);
        float a2 = dot(v_s2, v_s2) + kEpsilon;
        ao += a1 / a2;
    }

    ao *= _Radius; // intensity normalization

    // Apply other parameters.
    ao = pow(ao * _Intensity / _SampleCount, kContrast);

    return PackAONormal(ao, norm_o);
}
