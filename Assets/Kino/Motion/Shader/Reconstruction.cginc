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

// Returns true or false with a given interval.
bool Interval(half phase, half interval)
{
    return frac(phase / interval) > 0.499;
}

// Interleaved gradient function from Jimenez 2014 http://goo.gl/eomGso
float GradientNoise(float2 uv)
{
    uv = floor((uv + _Time.y) * _ScreenParams.xy);
    float f = dot(float2(0.06711056f, 0.00583715f), uv);
    return frac(52.9829189f * frac(f));
}

// Jitter function for tile lookup
float2 JitterTile(float2 uv)
{
    float rx, ry;
    sincos(GradientNoise(uv + float2(2, 0)) * UNITY_PI * 2, ry, rx);
    return float2(rx, ry) * _NeighborMaxTex_TexelSize.xy / 4;
}

// Velocity sampling function
half3 SampleVelocity(float2 uv)
{
    half3 v = tex2Dlod(_VelocityTex, float4(uv, 0, 0)).xyz;
    return half3((v.xy * 2 - 1) * _MaxBlurRadius, v.z);
}

// Reconstruction fragment shader
half4 frag_Reconstruction(v2f_multitex i) : SV_Target
{
    // Color sample at the center point
    const half4 c_p = tex2D(_MainTex, i.uv0);

    // Velocity/Depth sample at the center point
    const half3 vd_p = SampleVelocity(i.uv1);
    const half l_v_p = max(length(vd_p.xy), 0.5);
    const half rcp_d_p = 1 / vd_p.z;

    // NeighborMax vector sample at the center point
    const half2 v_max = tex2D(_NeighborMaxTex, i.uv1 + JitterTile(i.uv1)).xy;
    const half l_v_max = length(v_max);
    const half rcp_l_v_max = 1 / l_v_max;

    // Escape early if the NeighborMax vector is small enough.
    if (l_v_max < 2) return c_p;

    // Use V_p as a secondary sampling direction except when it's too small
    // compared to V_max. This vector is rescaled to be the length of V_max.
    const half2 v_alt = (l_v_p * 2 > l_v_max) ? vd_p.xy * (l_v_max / l_v_p) : v_max;

    // Determine the sample count.
    const half sc = floor(min(_LoopCount, l_v_max / 2));

    // Loop variables (starts from the outermost sample)
    const half dt = 1 / sc;
    const half t_offs = (GradientNoise(i.uv0) - 0.5) * dt;
    half t = 1 - dt / 2;
    half count = 0;

    // Background velocity
    // This is used for tracking the maximum velocity in the background layer.
    half l_v_bg = max(l_v_p, 1);

    // Color accumlation
    half4 acc = 0;

    UNITY_LOOP while (t > dt / 4)
    {
        // Sampling direction (switched per every two samples)
        const half2 v_s = Interval(count, 4) ? v_alt : v_max;

        // Sample position (inverted per every sample)
        const half t_s = (Interval(count, 2) ? -t : t) + t_offs;

        // Distance to the sample position
        const half l_t = l_v_max * abs(t_s);

        // UVs for the sample position
        const float2 uv0 = i.uv0 + v_s * t_s * _MainTex_TexelSize.xy;
        const float2 uv1 = i.uv1 + v_s * t_s * _VelocityTex_TexelSize.xy;

        // Color sample
        const half3 c = tex2Dlod(_MainTex, float4(uv0, 0, 0)).rgb;

        // Velocity/Depth sample
        const half3 vd = SampleVelocity(uv1);

        // Background/Foreground separation
        const half fg = saturate((vd_p.z - vd.z) * 20 * rcp_d_p);

        // Length of the velocity vector
        const half l_v = lerp(l_v_bg, length(vd.xy), fg);

        // Sample weight
        // (Distance test) * (Spreading out by motion) * (Triangular window)
        const half w = saturate(l_v - l_t) / l_v * (1.2 - t);

        // Color accumulation
        acc += half4(c, 1) * w;

        // Update the background velocity.
        l_v_bg = max(l_v_bg, l_v);

        // Advance to the next sample.
        t = Interval(count, 2) ? t - dt : t;
        count += 1;
    }

    // Add the center sample.
    acc += half4(c_p.rgb, 1) * (1.2 / (l_v_bg * sc * 2));

    return half4(acc.rgb / acc.a, c_p.a);
}
