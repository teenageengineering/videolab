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

// Geometry-aware separable bilateral filter
half4 frag_blur(v2f i) : SV_Target
{
#if defined(BLUR_HORIZONTAL)
    // Horizontal pass: Always use 2 texels interval to match to
    // the dither pattern.
    float2 delta = float2(_MainTex_TexelSize.x * 2, 0);
#else
    // Vertical pass: Apply _Downsample to match to the dither
    // pattern in the original occlusion buffer.
    float2 delta = float2(0, _MainTex_TexelSize.y / _Downsample * 2);
#endif

#if defined(BLUR_HIGH_QUALITY)

    // High quality 7-tap Gaussian with adaptive sampling

    fixed4 p0  = tex2D(_MainTex, i.uv);
    fixed4 p1a = tex2D(_MainTex, i.uv - delta);
    fixed4 p1b = tex2D(_MainTex, i.uv + delta);
    fixed4 p2a = tex2D(_MainTex, i.uv - delta * 2);
    fixed4 p2b = tex2D(_MainTex, i.uv + delta * 2);
    fixed4 p3a = tex2D(_MainTex, i.uv - delta * 3.2307692308);
    fixed4 p3b = tex2D(_MainTex, i.uv + delta * 3.2307692308);

#if defined(BLUR_SAMPLE_CENTER_NORMAL)
    fixed3 n0 = SampleNormal(i.uv);
#else
    fixed3 n0 = GetPackedNormal(p0);
#endif

    half w0 = 0.37004405286;
    half w1a = CompareNormal(n0, GetPackedNormal(p1a)) * 0.31718061674;
    half w1b = CompareNormal(n0, GetPackedNormal(p1b)) * 0.31718061674;
    half w2a = CompareNormal(n0, GetPackedNormal(p2a)) * 0.19823788546;
    half w2b = CompareNormal(n0, GetPackedNormal(p2b)) * 0.19823788546;
    half w3a = CompareNormal(n0, GetPackedNormal(p3a)) * 0.11453744493;
    half w3b = CompareNormal(n0, GetPackedNormal(p3b)) * 0.11453744493;

    half s;
    s  = GetPackedAO(p0)  * w0;
    s += GetPackedAO(p1a) * w1a;
    s += GetPackedAO(p1b) * w1b;
    s += GetPackedAO(p2a) * w2a;
    s += GetPackedAO(p2b) * w2b;
    s += GetPackedAO(p3a) * w3a;
    s += GetPackedAO(p3b) * w3b;

    s /= w0 + w1a + w1b + w2a + w2b + w3a + w3b;

#else

    // Fater 5-tap Gaussian with linear sampling

    fixed4 p0  = tex2D(_MainTex, i.uv);
    fixed4 p1a = tex2D(_MainTex, i.uv - delta * 1.3846153846);
    fixed4 p1b = tex2D(_MainTex, i.uv + delta * 1.3846153846);
    fixed4 p2a = tex2D(_MainTex, i.uv - delta * 3.2307692308);
    fixed4 p2b = tex2D(_MainTex, i.uv + delta * 3.2307692308);

#if defined(BLUR_SAMPLE_CENTER_NORMAL)
    fixed3 n0 = SampleNormal(i.uv);
#else
    fixed3 n0 = GetPackedNormal(p0);
#endif

    half w0 = 0.2270270270;
    half w1a = CompareNormal(n0, GetPackedNormal(p1a)) * 0.3162162162;
    half w1b = CompareNormal(n0, GetPackedNormal(p1b)) * 0.3162162162;
    half w2a = CompareNormal(n0, GetPackedNormal(p2a)) * 0.0702702703;
    half w2b = CompareNormal(n0, GetPackedNormal(p2b)) * 0.0702702703;

    half s;
    s  = GetPackedAO(p0)  * w0;
    s += GetPackedAO(p1a) * w1a;
    s += GetPackedAO(p1b) * w1b;
    s += GetPackedAO(p2a) * w2a;
    s += GetPackedAO(p2b) * w2b;

    s /= w0 + w1a + w1b + w2a + w2b;

#endif

    return PackAONormal(s, n0);
}
