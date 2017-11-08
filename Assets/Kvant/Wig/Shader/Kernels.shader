Shader "Hidden/Kvant/Wig/Kernels"
{
    Properties
    {
        [HideInInspector] _PositionBuffer("", 2D) = ""{}
        [HideInInspector] _VelocityBuffer("", 2D) = ""{}
        [HideInInspector] _BasisBuffer("", 2D) = ""{}
        [HideInInspector] _FoundationData("", 2D) = ""{}
    }

    CGINCLUDE

    #include "Common.cginc"
    #include "SimplexNoiseGrad3D.cginc"

    float _DeltaTime;

    float2 _SegmentLength; // (length, randomness)

    float _Spring;
    float _Damping;
    float3 _Gravity;

    float3 _NoiseParams; // (amplitude, frequency, speed)

    float SegmentLength(float2 uv)
    {
        float r = frac((uv.x + _RandomSeed) * 632.8133);
        return _SegmentLength.x * (1 - r * _SegmentLength.y);
    }

    // Divergence-free noise field
    float3 DFNoiseField(float3 p)
    {
        p *= _NoiseParams.y;
        p += float3(0.9, 1.0, 1.1) * (_NoiseParams.z * _Time.y);
        float3 n1 = snoise_grad(p);
        float3 n2 = snoise_grad(p + float3(15.3, 13.1, 17.4));
        return cross(n1, n2) * _NoiseParams.x;
    }

    float4 frag_InitPosition(v2f_img i) : SV_Target
    {
        float3 origin = SampleFoundationPosition(i.uv);
        float3 normal = SampleFoundationNormal(i.uv);

        float dv = _PositionBuffer_TexelSize.y;
        float v = i.uv.y - dv * 0.5;

        float l = SegmentLength(i.uv) / dv;

        return float4(origin + normal * (l * v), 1);
    }

    float4 frag_InitVelocity(v2f_img i) : SV_Target
    {
        return 0;
    }

    float4 frag_InitBasis(v2f_img i) : SV_Target
    {
        // Make a random basis around the foundation normal vector.
        float r = frac((i.uv.x + _RandomSeed) * 1314.92);
        float3 ax = float3(1, r * 2 - 1, 0);
        float3 az = SampleFoundationNormal(i.uv);
        float3 ay = normalize(cross(az, ax));
        ax = normalize(cross(ay, az));
        return EncodeBasis(ax, az);
    }

    float4 frag_UpdatePosition(v2f_img i) : SV_Target
    {
        if (i.uv.y < _PositionBuffer_TexelSize.y * 2)
        {
            // P0 and P1: Simply move with the foundation without physics.
            return frag_InitPosition(i);
        }
        else
        {
            // Newtonian motion
            float3 p = SamplePosition(i.uv);
            p += SampleVelocity(i.uv) * _DeltaTime;

            // Segment length constraint
            float3 pp = SamplePosition(i.uv, -1);
            p = pp + normalize(p - pp) * SegmentLength(i.uv);

            return float4(p, 1);
        }
    }

    float4 frag_UpdateVelocity(v2f_img i) : SV_Target
    {
        float3 p = SamplePosition(i.uv);
        float3 v = SampleVelocity(i.uv);

        // Damping
        v *= exp(-_Damping * _DeltaTime);

        // Target position
        float3 pp2 = SamplePosition(i.uv, -4);
        float3 pp1 = SamplePosition(i.uv, -1);
        float3 pt = pp1 + normalize(pp1 - pp2) * SegmentLength(i.uv);

        // Acceleration (spring model)
        v += (pt - p) * _DeltaTime * _Spring;

        // Gravity
        v += _Gravity * _DeltaTime;

        // Force from noise field
        v += DFNoiseField(p) * _DeltaTime;

        return float4(v, 0);
    }

    float4 frag_UpdateBasis(v2f_img i) : SV_Target
    {
        // Use the parent normal vector from the previous frame.
        float3 ax = StereoInverseProjection(SampleBasis(i.uv, -1).xy);

        // Tangent vector
        float3 p0 = SamplePosition(i.uv, -1);
        float3 p1 = SamplePosition(i.uv, 1);
        float3 az = normalize(p1 - p0);

        // Reconstruct the orthonormal basis.
        float3 ay = normalize(cross(az, ax));
        ax = normalize(cross(ay, az));

        return EncodeBasis(ax, az);
    }

    ENDCG

    SubShader
    {
        // Pass 0 - Position buffer initialization
        Pass
        {
            CGPROGRAM
            #pragma vertex vert_img
            #pragma fragment frag_InitPosition
            #pragma target 3.0
            ENDCG
        }
        // Pass 1 - Velocity buffer initialization
        Pass
        {
            CGPROGRAM
            #pragma vertex vert_img
            #pragma fragment frag_InitVelocity
            #pragma target 3.0
            ENDCG
        }
        // Pass 2 - Basis buffer initialization
        Pass
        {
            CGPROGRAM
            #pragma vertex vert_img
            #pragma fragment frag_InitBasis
            #pragma target 3.0
            ENDCG
        }
        // Pass 3 - Position buffer update
        Pass
        {
            CGPROGRAM
            #pragma vertex vert_img
            #pragma fragment frag_UpdatePosition
            #pragma target 3.0
            ENDCG
        }
        // Pass 4 - Velocity buffer update
        Pass
        {
            CGPROGRAM
            #pragma vertex vert_img
            #pragma fragment frag_UpdateVelocity
            #pragma target 3.0
            ENDCG
        }
        // Pass 5 - Basis buffer update
        Pass
        {
            CGPROGRAM
            #pragma vertex vert_img
            #pragma fragment frag_UpdateBasis
            #pragma target 3.0
            ENDCG
        }
    }
}
