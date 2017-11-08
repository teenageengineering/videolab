//
// GPGPU kernels for Stream
//
// Texture format:
// .xyz = particle position
// .w   = particle life
//
Shader "Hidden/Kvant/Stream/Kernel"
{
    Properties
    {
        _MainTex     ("-", 2D)     = ""{}
        _EmitterPos  ("-", Vector) = (0, 0, 20, 0)
        _EmitterSize ("-", Vector) = (40, 40, 40, 0)
        _Direction   ("-", Vector) = (0, 0, -1, 0.2)
        _SpeedParams ("-", Vector) = (5, 10, 0, 0)
        _NoiseParams ("-", Vector) = (0.2, 0.1, 1)  // (frequency, amplitude, animation)
        _Config      ("-", Vector) = (1, 2, 0, 0)   // (throttle, life, random seed, dT)
    }

    CGINCLUDE

    #pragma multi_compile NOISE_OFF NOISE_ON

    #include "UnityCG.cginc"
    #include "ClassicNoise3D.cginc"

    sampler2D _MainTex;

    float3 _EmitterPos;
    float3 _EmitterSize;
    float4 _Direction;
    float2 _SpeedParams;
    float4 _NoiseParams;
    float4 _Config;

    // PRNG function.
    float nrand(float2 uv, float salt)
    {
        uv += float2(salt, _Config.z);
        return frac(sin(dot(uv, float2(12.9898, 78.233))) * 43758.5453);
    }

    // Get a new particle.
    float4 new_particle(float2 uv)
    {
        float t = _Time.x;

        // Random position.
        float3 p = float3(nrand(uv, t + 1), nrand(uv, t + 2), nrand(uv, t + 3));
        p = (p - (float3)0.5) * _EmitterSize + _EmitterPos;

        // Life duration.
        float l = _Config.y * (0.5 + nrand(uv, t + 0));

        // Throttling: discard particle emission by adding offset.
        float4 offs = float4(1e8, 1e8, 1e8, -1e8) * (uv.x > _Config.x);

        return float4(p, l) + offs;
    }

    // Position dependant velocity field.
    float3 get_velocity(float3 p, float2 uv)
    {
        // Random vector.
        float3 v = float3(nrand(uv, 4), nrand(uv, 5), nrand(uv, 6));
        v = (v - (float3)0.5) * 2;

        // Apply the spread parameter.
        v = lerp(_Direction.xyz, v, _Direction.w);

        // Apply the speed parameter.
        v = normalize(v) * lerp(_SpeedParams.x, _SpeedParams.y, nrand(uv, 7));

#ifdef NOISE_ON
        // Add noise vector.
        p = (p + _Time.y * _NoiseParams.z) * _NoiseParams.x;
        float nx = cnoise(p + float3(138.2, 0, 0));
        float ny = cnoise(p + float3(0, 138.2, 0));
        float nz = cnoise(p + float3(0, 0, 138.2));
        v += float3(nx, ny, nz) * _NoiseParams.y;
#endif
        return v;
    }

    // Pass 0: Initialization
    float4 frag_init(v2f_img i) : SV_Target 
    {
        return new_particle(i.uv);
    }

    // Pass 1: Update
    float4 frag_update(v2f_img i) : SV_Target 
    {
        float4 p = tex2D(_MainTex, i.uv);
        if (p.w > 0)
        {
            float dt = _Config.w;
            p.xyz += get_velocity(p.xyz, i.uv) * dt; // position
            p.w -= dt;                               // life
            return p;
        }
        else
        {
            return new_particle(i.uv);
        }
    }

    ENDCG

    SubShader
    {
        // Pass 0: Initialization
        Pass
        {
            CGPROGRAM
            #pragma target 3.0
            #pragma vertex vert_img
            #pragma fragment frag_init
            ENDCG
        }
        // Pass 1: Update
        Pass
        {
            CGPROGRAM
            #pragma target 3.0
            #pragma vertex vert_img
            #pragma fragment frag_update
            ENDCG
        }
    }
}
