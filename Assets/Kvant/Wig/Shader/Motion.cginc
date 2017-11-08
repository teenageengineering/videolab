#include "Common.cginc"

float4x4 _NonJitteredVP;
float4x4 _PreviousVP;
float4x4 _PreviousM;
float _MotionScale;

struct appdata
{
    float4 vertex : POSITION;
    float2 texcoord : TEXCOORD0;
};

struct v2f
{
    float4 vertex : SV_POSITION;
    float4 transfer0 : TEXCOORD0;
    float4 transfer1 : TEXCOORD1;
};

v2f vert(appdata v)
{
    float2 uv = v.texcoord.xy;

    // Apply the local transformation.
    float th = Thickness(uv);
    float3 p0 = mul(v.vertex.xyz, DecodeBasis(SamplePreviousBasis(uv))) * th;
    float3 p1 = mul(v.vertex.xyz, DecodeBasis(SampleBasis(uv))) * th;

    // Move to the sampled position.
    p0 += SamplePreviousPosition(uv);
    p1 += SamplePosition(uv);

    // Scale the difference.
    // This is needed because the timestep used in the simulation kernels is
    // smaller than deltaTime. MotionScale = deltaTime / timeStep.
    p0 -= (p1 - p0) * (_MotionScale - 1);

    // Transfer the data to the pixel shader.
    v2f o;
    o.vertex = UnityObjectToClipPos(float4(p1, 1));
    o.transfer0 = mul(_PreviousVP, mul(_PreviousM,  float4(p0, 1)));
    o.transfer1 = mul(_NonJitteredVP, mul(unity_ObjectToWorld, float4(p1, 1)));
    return o;
}

half4 frag(v2f i) : SV_Target
{
    float3 hp0 = i.transfer0.xyz / i.transfer0.w;
    float3 hp1 = i.transfer1.xyz / i.transfer1.w;

    float2 vp0 = (hp0.xy + 1) / 2;
    float2 vp1 = (hp1.xy + 1) / 2;

#if UNITY_UV_STARTS_AT_TOP
    vp0.y = 1 - vp0.y;
    vp1.y = 1 - vp1.y;
#endif

    return half4(vp1 - vp0, 0, 1);
}
