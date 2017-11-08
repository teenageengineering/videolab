//
// Kino/Motion - Motion blur effect
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

// Returns the largest vector of v1 and v2.
half2 VMax(half2 v1, half2 v2)
{
    return dot(v1, v1) < dot(v2, v2) ? v2 : v1;
}

// Fragment shader: Velocity texture setup
half4 frag_VelocitySetup(v2f_img i) : SV_Target
{
    // Sample the motion vector.
    float2 v = tex2D(_CameraMotionVectorsTexture, i.uv).rg;

    // Apply the exposure time and convert to the pixel space.
    v *= (_VelocityScale * 0.5) * _CameraMotionVectorsTexture_TexelSize.zw;

    // Clamp the vector with the maximum blur radius.
    v /= max(1, length(v) * _RcpMaxBlurRadius);

    // Sample the depth of the pixel.
    half d = LinearizeDepth(SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, i.uv));

    // Pack into 10/10/10/2 format.
    return half4((v * _RcpMaxBlurRadius + 1) / 2, d, 0);
}

// Fragment shader: TileMax filter (2 pixel width with normalization)
half4 frag_TileMax1(v2f_img i) : SV_Target
{
    float4 d = _MainTex_TexelSize.xyxy * float4(-0.5, -0.5, 0.5, 0.5);

    half2 v1 = tex2D(_MainTex, i.uv + d.xy).rg;
    half2 v2 = tex2D(_MainTex, i.uv + d.zy).rg;
    half2 v3 = tex2D(_MainTex, i.uv + d.xw).rg;
    half2 v4 = tex2D(_MainTex, i.uv + d.zw).rg;

    v1 = (v1 * 2 - 1) * _MaxBlurRadius;
    v2 = (v2 * 2 - 1) * _MaxBlurRadius;
    v3 = (v3 * 2 - 1) * _MaxBlurRadius;
    v4 = (v4 * 2 - 1) * _MaxBlurRadius;

    return half4(VMax(VMax(VMax(v1, v2), v3), v4), 0, 0);
}

// Fragment shader: TileMax filter (2 pixel width)
half4 frag_TileMax2(v2f_img i) : SV_Target
{
    float4 d = _MainTex_TexelSize.xyxy * float4(-0.5, -0.5, 0.5, 0.5);

    half2 v1 = tex2D(_MainTex, i.uv + d.xy).rg;
    half2 v2 = tex2D(_MainTex, i.uv + d.zy).rg;
    half2 v3 = tex2D(_MainTex, i.uv + d.xw).rg;
    half2 v4 = tex2D(_MainTex, i.uv + d.zw).rg;

    return half4(VMax(VMax(VMax(v1, v2), v3), v4), 0, 0);
}

// Fragment shader: TileMax filter (variable width)
half4 frag_TileMaxV(v2f_img i) : SV_Target
{
    float2 uv0 = i.uv + _MainTex_TexelSize.xy * _TileMaxOffs.xy;

    float2 du = float2(_MainTex_TexelSize.x, 0);
    float2 dv = float2(0, _MainTex_TexelSize.y);

    half2 vo = 0;

    UNITY_LOOP for (int ix = 0; ix < _TileMaxLoop; ix++)
    {
        UNITY_LOOP for (int iy = 0; iy < _TileMaxLoop; iy++)
        {
            float2 uv = uv0 + du * ix + dv * iy;
            vo = VMax(vo, tex2D(_MainTex, uv).rg);
        }
    }

    return half4(vo, 0, 0);
}

// Fragment shader: NeighborMax filter
half4 frag_NeighborMax(v2f_img i) : SV_Target
{
    const half cw = 1.01f; // center weight tweak

    float4 d = _MainTex_TexelSize.xyxy * float4(1, 1, -1, 0);

    half2 v1 = tex2D(_MainTex, i.uv - d.xy).rg;
    half2 v2 = tex2D(_MainTex, i.uv - d.wy).rg;
    half2 v3 = tex2D(_MainTex, i.uv - d.zy).rg;

    half2 v4 = tex2D(_MainTex, i.uv - d.xw).rg;
    half2 v5 = tex2D(_MainTex, i.uv       ).rg * cw;
    half2 v6 = tex2D(_MainTex, i.uv + d.xw).rg;

    half2 v7 = tex2D(_MainTex, i.uv + d.zy).rg;
    half2 v8 = tex2D(_MainTex, i.uv + d.wy).rg;
    half2 v9 = tex2D(_MainTex, i.uv + d.xy).rg;

    half2 va = VMax(v1, VMax(v2, v3));
    half2 vb = VMax(v4, VMax(v5, v6));
    half2 vc = VMax(v7, VMax(v8, v9));

    return half4(VMax(va, VMax(vb, vc)) / cw, 0, 0);
}
