#include "Common.cginc"

half _Metallic;
half _Smoothness;

half3 _BaseColor;
half _BaseRandom;

half _GlowIntensity;
half _GlowProb;
half3 _GlowColor;
half _GlowRandom;

struct Input
{
    half filamentID;
};

void vert(inout appdata_full v, out Input data)
{
    UNITY_INITIALIZE_OUTPUT(Input, data);

    float2 uv = v.texcoord.xy;

    // Orthonormal basis vectors
    float3x3 basis = DecodeBasis(SampleBasis(uv));

    // Apply the local transformation and move to the sampled position.
    v.vertex.xyz = SamplePosition(uv) + mul(v.vertex, basis) * Thickness(uv);
    v.normal = mul(v.normal, basis);

    // Parameters for the pixel shader
    data.filamentID = uv.x + _RandomSeed * 58.92128;
}

void surf(Input IN, inout SurfaceOutputStandard o)
{
    // Random color
    half3 color = HueToRGB(frac(IN.filamentID * 314.2213));

    // Glow effect
    half glow = frac(IN.filamentID * 138.9044 + _Time.y / 2) < _GlowProb;

    o.Albedo = lerp(_BaseColor, color, _BaseRandom);
    o.Smoothness = _Smoothness;
    o.Metallic = _Metallic;
    o.Emission = lerp(_GlowColor, color, _GlowRandom) * _GlowIntensity * glow;
}
