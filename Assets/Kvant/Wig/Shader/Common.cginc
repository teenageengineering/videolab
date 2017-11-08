#include "UnityCG.cginc"

// Point position buffer (w is unused)
sampler2D _PositionBuffer;
float4 _PositionBuffer_TexelSize;

sampler2D _PreviousPositionBuffer;
float4 _PreviousPositionBuffer_TexelSize;

// Point velocity buffer (w is unused)
sampler2D _VelocityBuffer;
float4 _VelocityBuffer_TexelSize;

// Orthonormal basis vectors buffer
// xy: normal, zw: tangent
// Vectors are encoded with the stereographic projection.
sampler2D _BasisBuffer;
float4 _BasisBuffer_TexelSize;

sampler2D _PreviousBasisBuffer;
float4 _PreviousBasisBuffer_TexelSize;

// Foundation mesh data
sampler2D _FoundationData;
float4 _FoundationData_TexelSize;

// Transformation of the foundation mesh
float4x4 _FoundationTransform;

// Filament thickness parameters
half _Thickness;
half _ThickRandom;

// Seed for PRNG
float _RandomSeed;

// Hue to RGB convertion
half3 HueToRGB(half h)
{
    half r = abs(h * 6 - 3) - 1;
    half g = 2 - abs(h * 6 - 2);
    half b = 2 - abs(h * 6 - 4);
    half3 rgb = saturate(half3(r, g, b));
#if UNITY_COLORSPACE_GAMMA
    return rgb;
#else
    return GammaToLinearSpace(rgb);
#endif
}

// Stereographic projection and inverse projection
float2 StereoProjection(float3 n)
{
    return n.xy / (1 - n.z);
}

float3 StereoInverseProjection(float2 p)
{
    float d = 2 / (dot(p.xy, p.xy) + 1);
    return float3(p.xy * d, 1 - d);
}

// Sampling function for position buffer
float3 SamplePosition(float2 uv)
{
    return tex2Dlod(_PositionBuffer, float4(uv, 0, 0)).xyz;
}

float3 SamplePosition(float2 uv, float delta)
{
    uv.y += _PositionBuffer_TexelSize.y * delta;
    return tex2Dlod(_PositionBuffer, float4(uv, 0, 0)).xyz;
}

float3 SamplePreviousPosition(float2 uv)
{
    return tex2Dlod(_PreviousPositionBuffer, float4(uv, 0, 0)).xyz;
}

// Sampling function for velocity buffer
float3 SampleVelocity(float2 uv)
{
    return tex2Dlod(_VelocityBuffer, float4(uv, 0, 0)).xyz;
}

// Sampling function for basis buffer
float4 SampleBasis(float2 uv)
{
    return tex2Dlod(_BasisBuffer, float4(uv, 0, 0));
}

float4 SampleBasis(float2 uv, float delta)
{
    uv.y += _BasisBuffer_TexelSize.y * delta;
    return tex2Dlod(_BasisBuffer, float4(uv, 0, 0));
}

float4 SamplePreviousBasis(float2 uv)
{
    return tex2Dlod(_PreviousBasisBuffer, float4(uv, 0, 0));
}

// Encoder/decoder function for basis buffer
float4 EncodeBasis(float3 ax, float3 az)
{
    return float4(StereoProjection(ax), StereoProjection(az));
}

float3x3 DecodeBasis(float4 bs)
{
    float3 ax = StereoInverseProjection(bs.xy);
    float3 az = StereoInverseProjection(bs.zw);
    return float3x3(ax, cross(az, ax), az);
}

// Sampling functions for foundation data
float4 SampleFoundationPosition(float2 uv)
{
    float3 p = tex2Dlod(_FoundationData, float4(uv.x, 0, 0, 0)).xyz;
    return mul(_FoundationTransform, float4(p, 1));
}

float3 SampleFoundationNormal(float2 uv)
{
    float3 n = tex2Dlod(_FoundationData, float4(uv.x, 1, 0, 0)).xyz;
    return mul((float3x3)_FoundationTransform, n);
}

// Filament thickness function
float Thickness(float2 uv)
{
    float t = _Thickness * (1 - uv.y * uv.y);
    t *= 1 - _ThickRandom * frac((uv.x + _RandomSeed) * 893.8912);
    return t;
}
