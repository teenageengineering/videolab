//
// Kino/Vision - Frame visualization utility
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

half _Blend;
half _Repeat;

sampler2D_float _CameraDepthTexture;
sampler2D _CameraDepthNormalsTexture;

float LinearizeDepth(float z)
{
    float isOrtho = unity_OrthoParams.w;
    float isPers = 1 - unity_OrthoParams.w;
    z *= _ZBufferParams.x;
    return (1 - isOrtho * z) / (isPers * z + _ZBufferParams.y);
}

half4 frag_depth(v2f_common i) : SV_Target
{
    half4 src = tex2D(_MainTex, i.uv);

#ifdef USE_CAMERA_DEPTH
    float depth = SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, i.uvAlt);
    depth = LinearizeDepth(depth);
#else // USE_CAMERA_DEPTH_NORMALS
    float4 cdn = tex2D(_CameraDepthNormalsTexture, i.uvAlt);
    float depth = DecodeFloatRG(cdn.zw);
#endif

    float dr = frac(depth * _Repeat);
    float d1 = 1 - dr;
    float d2 = 1 / (1 + dr * 100);
    half3 rgb = half3(d1, d2, d2);

#if !UNITY_COLORSPACE_GAMMA
    rgb = GammaToLinearSpace(rgb);
#endif

    rgb = lerp(src.rgb, rgb, _Blend);

    return half4(rgb, src.a);
}
