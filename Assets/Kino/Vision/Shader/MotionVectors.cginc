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
half _Amplitude;
float2 _Scale;

sampler2D_half _CameraMotionVectorsTexture;

// Convert a motion vector into RGBA color.
half4 VectorToColor(float2 mv)
{
    half phi = atan2(mv.x, mv.y);
    half hue = (phi / UNITY_PI + 1) * 0.5;

    half r = abs(hue * 6 - 3) - 1;
    half g = 2 - abs(hue * 6 - 2);
    half b = 2 - abs(hue * 6 - 4);
    half a = length(mv);

    return saturate(half4(r, g, b, a));
}

// Motion vectors overlay shader (fragment only)
half4 frag_overlay(v2f_common i) : SV_Target
{
    half4 src = tex2D(_MainTex, i.uv);

    half2 mv = tex2D(_CameraMotionVectorsTexture, i.uvAlt).rg * _Amplitude;
    half4 mc = VectorToColor(mv);

    half3 rgb = mc.rgb;
#if !UNITY_COLORSPACE_GAMMA
    rgb = GammaToLinearSpace(rgb);
#endif

    half src_ratio = saturate(2 - _Blend * 2);
    half mc_ratio = saturate(_Blend * 2);
    rgb = lerp(src.rgb * src_ratio, rgb, mc.a * mc_ratio);

    return half4(rgb, src.a);
}

// Vertex/fragment shader for drawing arrows
struct v2f_arrows
{
    float4 vertex : SV_POSITION;
    float2 scoord : TEXCOORD;
    half4 color : COLOR;
};

v2f_arrows vert_arrows(appdata_base v)
{
    // Retrieve the motion vector.
    float4 uv = float4(v.texcoord.xy, 0, 0);
    half2 mv = tex2Dlod(_CameraMotionVectorsTexture, uv).rg * _Amplitude;

    // Arrow color
    half4 color = VectorToColor(mv);

    // Make a rotation matrix based on the motion vector.
    float2x2 rot = float2x2(mv.y, mv.x, -mv.x, mv.y);

    // Rotate and scale the body of the arrow.
    float2 pos = mul(rot, v.vertex.zy) * _Scale;

    // Normalized variant of the motion vector and the rotation matrix.
    float2 mv_n = normalize(mv);
    float2x2 rot_n = float2x2(mv_n.y, mv_n.x, -mv_n.x, mv_n.y);

    // Rotate and scale the head of the arrow.
    float2 head = float2(v.vertex.x, -abs(v.vertex.x)) * 0.3;
    head *= saturate(color.a);
    pos += mul(rot_n, head) * _Scale;

    // Offset the arrow position.
    pos += v.texcoord.xy * 2 - 1;

    // Convert to the screen coordinates.
    float2 scoord = (pos + 1) * 0.5 * _ScreenParams.xy;

    // Snap to a pixel-perfect position.
    scoord = round(scoord);

    // Bring back to the normalized screen space.
    pos = (scoord + 0.5) * (_ScreenParams.zw - 1) * 2 - 1;
    pos.y *= _ProjectionParams.x;

    // Color tweaks
    color.rgb = GammaToLinearSpace(lerp(color.rgb, 1, 0.5));
    color.a *= _Blend;

    // Output
    v2f_arrows o;
    o.vertex = float4(pos, 0, 1);
    o.scoord = scoord;
    o.color = saturate(color);
    return o;
}

half4 frag_arrows(v2f_arrows i) : SV_Target
{
    // Pseudo anti-aliasing.
    float aa = length(frac(i.scoord) - 0.5) / 0.707;
    aa *= (aa * (aa * 0.305306011 + 0.682171111) + 0.012522878); // gamma

    half4 c = i.color;
    c.a *= aa;
    return c;
}
