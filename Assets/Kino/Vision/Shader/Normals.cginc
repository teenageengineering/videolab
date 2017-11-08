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
half _Validate;

sampler2D _CameraGBufferTexture2;
sampler2D _CameraDepthNormalsTexture;

half4 frag_normals(v2f_common i) : SV_Target
{
    half4 src = tex2D(_MainTex, i.uv);

#ifdef USE_CAMERA_DEPTH_NORMALS
    float4 cdn = tex2D(_CameraDepthNormalsTexture, i.uvAlt);
    float3 n = DecodeViewNormalStereo(cdn);
    float isZero = (dot(n, 1) == 0);
#else // USE_GBUFFER
    float3 n = tex2D(_CameraGBufferTexture2, i.uvAlt).xyz;
    float isZero = (dot(n, 1) == 0);
    n = mul((float3x3)unity_WorldToCamera, n * 2 - 1);
    n.z = -n.z;
#endif

    float l = length(n);
    float invalid = max(l < 0.99, l > 1.01) - isZero;

    n = (n + 1) * 0.5;
#if !UNITY_COLORSPACE_GAMMA
    n = GammaToLinearSpace(n);
#endif

    half3 rgb = lerp(n, half3(1, 0, 0), invalid * _Validate);
    rgb = lerp(src.rgb, rgb, _Blend);

    return half4(rgb, src.a);
}
