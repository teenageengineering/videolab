//
// Kvant/Warp - Warp (hyperspace) light streaks effect
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

// Note:
// Texcoord0 in the vertex data gives a line ID (line number) and
// two random numbers. We can use them to scatter the lines.

// Seed for PRNG
float _RandomSeed;

// PRNG function
float LineRandom(float ln, float salt)
{
    float2 uv = float2(ln, salt * 0.938198424 + _RandomSeed * 11.0938495);
    return frac(sin(dot(uv, float2(12.9898, 78.233))) * 43758.5453);
}

// Line parameters
float _LineRadius;
float2 _LineWidth; // (min, max)

float3 ApplyLineParams(float3 v, float3 uvw)
{
    // Get a random width using a random number from the vertex.
    float sz = lerp(_LineWidth.x, _LineWidth.y, uvw.y);

    return v * float3(_LineRadius, _LineRadius, sz);
}

// Line speed
float _SpeedRandomness;

float GetLineSpeed(float3 uvw)
{
    return 1 - _SpeedRandomness * (1 - uvw.z);
}

// Line position
float3 _Extent;
float _Throttle;

float3 GetLinePosition(float3 uvw, float time)
{
    // Z offset <= random number from the vertex
    float z = uvw.z;

    // Apply speed and time.
    z += GetLineSpeed(uvw) * time;

    // Line ID <= original ID + number of wrapping around
    float ln = uvw.x + trunc(z);

    // Random offset
    float2 xy = float2(LineRandom(ln, 0), LineRandom(ln, 1));

    // Offset by throttling
    float offset = (uvw.x > _Throttle) * 1000000;

    // Apply the extent.
    return (0.5 - float3(xy, frac(z))) * _Extent + offset;
}
