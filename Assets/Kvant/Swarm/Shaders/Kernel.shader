//
// GPGPU kernels for Swarm
//
// Position buffer format:
// .xyz = particle position
// .w   = random number
//
// Velocity buffer format:
// .xyz = particle velocity
// .w   = 0
//
Shader "Hidden/Kvant/Swarm/Kernel"
{
    Properties
    {
        _PositionTex ("-", 2D) = ""{}
        _VelocityTex ("-", 2D) = ""{}
    }

    CGINCLUDE

    #include "UnityCG.cginc"
    #include "SimplexNoiseGrad3D.cginc"

    sampler2D _PositionTex;
    sampler2D _VelocityTex;
    float4 _PositionTex_TexelSize;
    float4 _VelocityTex_TexelSize;

    float3 _Acceleration;   // min, max, drag
    float4 _Attractor;      // x, y, z, spread
    float3 _Flow;
    float3 _NoiseParams;    // amplitude, frequency, spread
    float3 _NoiseOffset;
    float2 _SwirlParams;    // amplitude, frequency
    float2 _Config;         // deltaTime, randomSeed

    // pseudo random number generator
    float nrand(float2 uv, float salt)
    {
        uv += float2(salt, _Config.y);
        return frac(sin(dot(uv, float2(12.9898, 78.233))) * 43758.5453);
    }

    // divergence-free noise vector field
    float3 dfnoise_field(float3 p, float k)
    {
        p += float3(0.9, 1.0, 1.1) * k * _NoiseParams.z;
        float3 n1 = snoise_grad(p);
        float3 n2 = snoise_grad(p + float3(15.3, 13.1, 17.4));
        return cross(n1, n2);
    }

    // attractor position with spread parameter
    float3 attract_point(float2 uv)
    {
        float3 r = float3(nrand(uv, 0), nrand(uv, 1), nrand(uv, 2));
        return _Attractor.xyz + (r - 0.5) * _Attractor.w;
    }

    // pass 0: position initialization
    float4 frag_init_position(v2f_img i) : SV_Target
    {
        return float4(attract_point(i.uv.y + 2), nrand(i.uv.y, 6));
    }

    // pass 1: velocity initialization
    float4 frag_init_velocity(v2f_img i) : SV_Target
    {
        return (float4)0;
    }

    // pass 2: position update
    float4 frag_update_position(v2f_img i) : SV_Target
    {
        // u=0: p <- head position
        // u>0: p <- one newer entry in the history array
        float2 uv_prev = float2(_PositionTex_TexelSize.x, 0);
        float4 p = tex2D(_PositionTex, i.uv - uv_prev);

        // velocity vector for the head point (u=0)
        float3 v0 = tex2D(_VelocityTex, i.uv).xyz;

        // velocity vector for the tail points (u>0)
        float3 np = (p.xyz + _NoiseOffset) * _SwirlParams.y;
        float3 v1 = _Flow + dfnoise_field(np, i.uv.y) * _SwirlParams.x;

        // applying the velocity vector
        float u_0 = i.uv.x < _PositionTex_TexelSize.x;
        p.xyz += lerp(v1, v0, u_0) * _Config.x;

        return p;
    }

    // pass 3: velocity update
    float4 frag_update_velocity(v2f_img i) : SV_Target
    {
        // only needs the leftmost pixel
        float2 uv = i.uv * float2(0, 1);

        // head point position/velocity
        float3 p = tex2D(_PositionTex, uv).xyz;
        float3 v = tex2D(_VelocityTex, uv).xyz;

        // force from the attactor
        float accel = lerp(_Acceleration.x, _Acceleration.y, nrand(uv, 7));
        float3 fa = (attract_point(uv) - p) * accel;

        // force from the noise vector field
        float3 np = (p + _NoiseOffset) * _NoiseParams.y;
        float3 fn = dfnoise_field(np, uv.y) * _NoiseParams.x;

        // applying drag and acceleration force
        v = v * _Acceleration.z + (fa + fn) * _Config.x;

        return float4(v, 0);
    }

    ENDCG

    SubShader
    {
        Pass
        {
            CGPROGRAM
            #pragma target 3.0
            #pragma vertex vert_img
            #pragma fragment frag_init_position
            ENDCG
        }
        Pass
        {
            CGPROGRAM
            #pragma target 3.0
            #pragma vertex vert_img
            #pragma fragment frag_init_velocity
            ENDCG
        }
        Pass
        {
            CGPROGRAM
            #pragma target 3.0
            #pragma vertex vert_img
            #pragma fragment frag_update_position
            ENDCG
        }
        Pass
        {
            CGPROGRAM
            #pragma target 3.0
            #pragma vertex vert_img
            #pragma fragment frag_update_velocity
            ENDCG
        }
    }
}
