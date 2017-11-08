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

// Gamma encoding (only needed in gamma lighting mode)
half EncodeAO(half x)
{
    half x_g = 1 - max(1.055 * pow(1 - x, 0.416666667) - 0.055, 0);
    // ColorSpaceLuminance.w == 0 (gamma) or 1 (linear)
    return lerp(x_g, x, unity_ColorSpaceLuminance.w);
}

// Geometry-aware bilateral filter (single pass/small kernel)
half BlurSmall(sampler2D tex, float2 uv, float2 delta)
{
    fixed4 p0 = tex2D(tex, uv);
    fixed4 p1 = tex2D(tex, uv + float2(-delta.x, -delta.y));
    fixed4 p2 = tex2D(tex, uv + float2(+delta.x, -delta.y));
    fixed4 p3 = tex2D(tex, uv + float2(-delta.x, +delta.y));
    fixed4 p4 = tex2D(tex, uv + float2(+delta.x, +delta.y));

    fixed3 n0 = GetPackedNormal(p0);

    half w0 = 1;
    half w1 = CompareNormal(n0, GetPackedNormal(p1));
    half w2 = CompareNormal(n0, GetPackedNormal(p2));
    half w3 = CompareNormal(n0, GetPackedNormal(p3));
    half w4 = CompareNormal(n0, GetPackedNormal(p4));

    half s;
    s  = GetPackedAO(p0) * w0;
    s += GetPackedAO(p1) * w1;
    s += GetPackedAO(p2) * w2;
    s += GetPackedAO(p3) * w3;
    s += GetPackedAO(p4) * w4;

    return s / (w0 + w1 + w2 + w3 + w4);
}

// Final composition shader
half4 frag_composition(v2f i) : SV_Target
{
    float2 delta = _MainTex_TexelSize.xy / _Downsample;
    half ao = BlurSmall(_OcclusionTexture, i.uvAlt, delta);

    half4 color = tex2D(_MainTex, i.uv);
    color.rgb *= 1 - EncodeAO(ao);

    return color;
}

// Final composition shader (ambient-only mode)
v2f_img vert_composition_gbuffer(appdata_img v)
{
    v2f_img o;
    o.pos = v.vertex * float4(2, 2, 0, 0) + float4(0, 0, 0, 1);
#if UNITY_UV_STARTS_AT_TOP
    o.uv = v.texcoord * float2(1, -1) + float2(0, 1);
#else
    o.uv = v.texcoord;
#endif
    return o;
}

#if !defined(SHADER_API_GLES) // excluding the MRT pass under GLES2

struct CompositionOutput
{
    half4 gbuffer0 : SV_Target0;
    half4 gbuffer3 : SV_Target1;
};

CompositionOutput frag_composition_gbuffer(v2f_img i)
{
    // Workaround: _OcclusionTexture_Texelsize hasn't been set properly
    // for some reasons. Use _ScreenParams instead.
    float2 delta = (_ScreenParams.zw - 1) / _Downsample;
    half ao = BlurSmall(_OcclusionTexture, i.uv, delta);

    CompositionOutput o;
    o.gbuffer0 = half4(0, 0, 0, ao);
    o.gbuffer3 = half4((half3)EncodeAO(ao), 0);
    return o;
}

#else

fixed4 frag_composition_gbuffer(v2f_img i) : SV_Target0
{
    return 0;
}

#endif
