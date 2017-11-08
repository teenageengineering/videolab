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

#include "UnityCG.cginc"

// --------
// Options for further customization
// --------

// By default, a 5-tap Gaussian with the linear sampling technique is used
// in the bilateral noise filter. It can be replaced with a 7-tap Gaussian
// with adaptive sampling by enabling the macro below. Although the
// differences are not noticeable in most cases, it may provide preferable
// results with some special usage (e.g. NPR without textureing).
// #define BLUR_HIGH_QUALITY

// By default, a fixed sampling pattern is used in the AO estimator. Although
// this gives preferable results in most cases, a completely random sampling
// pattern could give aesthetically better results. Disable the macro below
// to use such a random pattern instead of the fixed one.
#define FIX_SAMPLING_PATTERN

// The SampleNormal function normalizes samples from G-buffer because
// they're possibly unnormalized. We can eliminate this if it can be said
// that there is no wrong shader that outputs unnormalized normals.
// #define VALIDATE_NORMALS

// The constant below determines the contrast of occlusion. This allows
// users to control over/under occlusion. At the moment, this is not exposed
// to the editor because it’s rarely useful.
static const float kContrast = 0.6;

// The constant below controls the geometry-awareness of the bilateral
// filter. The higher value, the more sensitive it is.
static const float kGeometryCoeff = 0.8;

// The constants below are used in the AO estimator. Beta is mainly used
// for suppressing self-shadowing noise, and Epsilon is used to prevent
// calculation underflow. See the paper (Morgan 2011 http://goo.gl/2iz3P)
// for further details of these constants.
static const float kBeta = 0.002;
static const float kEpsilon = 1e-4;

// --------

// System built-in variables
sampler2D _CameraGBufferTexture2;
sampler2D_float _CameraDepthTexture;
sampler2D _CameraDepthNormalsTexture;

// Sample count
#if !defined(SHADER_API_GLES)
int _SampleCount;
#else
// GLES2: In many cases, dynamic looping is not supported.
static const int _SampleCount = 3;
#endif

// Source texture properties
sampler2D _MainTex;
float4 _MainTex_TexelSize;
half4 _MainTex_ST;

sampler2D _OcclusionTexture;
float4 _OcclusionTexture_TexelSize;

// Other parameters
half _Intensity;
float _Radius;
float _Downsample;

// Accessors for packed AO/normal buffer
fixed4 PackAONormal(fixed ao, fixed3 n)
{
    return fixed4(ao, n * 0.5 + 0.5);
}

fixed GetPackedAO(fixed4 p)
{
    return p.r;
}

fixed3 GetPackedNormal(fixed4 p)
{
    return p.gba * 2 - 1;
}

// Boundary check for depth sampler
// (returns a very large value if it lies out of bounds)
float CheckBounds(float2 uv, float d)
{
    float ob = any(uv < 0) + any(uv > 1);
#if defined(UNITY_REVERSED_Z)
    ob += (d <= 0.00001);
#else
    ob += (d >= 0.99999);
#endif
    return ob * 1e8;
}

// Z buffer depth to linear 0-1 depth
float LinearizeDepth(float z)
{
    float isOrtho = unity_OrthoParams.w;
    float isPers = 1 - unity_OrthoParams.w;
    z *= _ZBufferParams.x;
    return (1 - isOrtho * z) / (isPers * z + _ZBufferParams.y);
}

// Depth/normal sampling functions
float SampleDepth(float2 uv)
{
#if defined(SOURCE_GBUFFER) || defined(SOURCE_DEPTH)
    float d = LinearizeDepth(SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, uv));
#else
    float4 cdn = tex2D(_CameraDepthNormalsTexture, uv);
    float d = DecodeFloatRG(cdn.zw);
#endif
    return d * _ProjectionParams.z + CheckBounds(uv, d);
}

float3 SampleNormal(float2 uv)
{
#if defined(SOURCE_GBUFFER)
    float3 norm = tex2D(_CameraGBufferTexture2, uv).xyz;
    norm = norm * 2 - any(norm); // gets (0,0,0) when norm == 0
    norm = mul((float3x3)unity_WorldToCamera, norm);
#if defined(VALIDATE_NORMALS)
    norm = normalize(norm);
#endif
    return norm;
#else
    float4 cdn = tex2D(_CameraDepthNormalsTexture, uv);
    return DecodeViewNormalStereo(cdn) * float3(1, 1, -1);
#endif
}

float SampleDepthNormal(float2 uv, out float3 normal)
{
#if defined(SOURCE_GBUFFER) || defined(SOURCE_DEPTH)
    normal = SampleNormal(uv);
    return SampleDepth(uv);
#else
    float4 cdn = tex2D(_CameraDepthNormalsTexture, uv);
    normal = DecodeViewNormalStereo(cdn) * float3(1, 1, -1);
    float d = DecodeFloatRG(cdn.zw);
    return d * _ProjectionParams.z + CheckBounds(uv, d);
#endif
}

// Normal vector comparer (for geometry-aware weighting)
half CompareNormal(half3 d1, half3 d2)
{
    return smoothstep(kGeometryCoeff, 1, dot(d1, d2));
}

// Common vertex shader
struct v2f
{
    float4 pos : SV_POSITION;
    half2 uv : TEXCOORD0;    // Screen space UV (supports stereo rendering)
    half2 uv01 : TEXCOORD1;  // Original UV (from 0 to 1)
    half2 uvAlt : TEXCOORD2; // Alternative UV (supports v-flip case)
};

v2f vert(appdata_img v)
{
    half2 uvAlt = v.texcoord;
#if UNITY_UV_STARTS_AT_TOP
    if (_MainTex_TexelSize.y < 0.0) uvAlt.y = 1 - uvAlt.y;
#endif

    v2f o;
    o.pos = UnityObjectToClipPos(v.vertex);
#if defined(UNITY_SINGLE_PASS_STEREO)
    o.uv = UnityStereoScreenSpaceUVAdjust(v.texcoord, _MainTex_ST);
    o.uvAlt = UnityStereoScreenSpaceUVAdjust(uvAlt, _MainTex_ST);
#else
    o.uv = v.texcoord;
    o.uvAlt = uvAlt;
#endif
    o.uv01 = uvAlt;

    return o;
}
