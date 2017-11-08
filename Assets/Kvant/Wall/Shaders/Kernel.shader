//
// GPGPU kernels for Wall
//
// Position kernel outputs:
// .xyz = position
// .w   = random value (0-1)
//
// Rotation kernel outputs:
// .xyzw = rotation (quaternion)
//
// Scale kernel outputs:
// .xyz = scale factor
// .w   = random value (0-1)
//
Shader "Hidden/Kvant/Wall/Kernel"
{
    CGINCLUDE

    #include "UnityCG.cginc"
    #include "SimplexNoiseExt3D.cginc"

    #pragma multi_compile POSITION_Z POSITION_XYZ POSITION_RANDOM
    #pragma multi_compile ROTATION_AXIS ROTATION_RANDOM
    #pragma multi_compile SCALE_UNIFORM SCALE_XYZ

    float2 _ColumnRow;
    float2 _Extent;
    float2 _UVOffset;
    float3 _BaseScale;
    float2 _RandomScale;    // min, max
    float4 _PositionNoise;  // x freq, y freq, amp, time
    float4 _RotationNoise;  // x freq, y freq, amp, time
    float4 _ScaleNoise;     // x freq, y freq, amp, time
    float3 _RotationAxis;

    // PRNG function.
    float nrand(float2 uv, float salt)
    {
        uv += float2(salt, 0);
        return frac(sin(dot(uv, float2(12.9898, 78.233))) * 43758.5453);
    }

    // Snap UV coordinate to column*row grid.
    float2 snap_uv(float2 uv)
    {
        return floor(uv * _ColumnRow) / _ColumnRow;
    }

    // Quaternion multiplication.
    // http://mathworld.wolfram.com/Quaternion.html
    float4 qmul(float4 q1, float4 q2)
    {
        return float4(
            q2.xyz * q1.w + q1.xyz * q2.w + cross(q1.xyz, q2.xyz),
            q1.w * q2.w - dot(q1.xyz, q2.xyz)
        );
    }

    // Get a random rotation axis in a deterministic fashion.
    float3 get_rotation_axis(float2 uv)
    {
        // Uniformaly distributed points.
        // http://mathworld.wolfram.com/SpherePointPicking.html
        float u = nrand(uv, 0) * 2 - 1;
        float theta = nrand(uv, 1) * UNITY_PI * 2;
        float u2 = sqrt(1 - u * u);
        return float3(u2 * cos(theta), u2 * sin(theta), u);
    }

    // Pass 0: Position kernel
    float4 frag_position(v2f_img i) : SV_Target
    {
        // Offset UV
        float2 uv = snap_uv(i.uv + _UVOffset);

        // Base position
        float3 origin = float3((i.uv - 0.5) * _Extent, 0); // << Note: use original UV, not ofsetted UV..

        // Noise coordinate
        float3 nc = float3(uv * _PositionNoise.xy, _PositionNoise.w);

        // Displacement
    #if POSITION_Z
        float3 disp = float3(0, 0, snoise(nc));
    #elif POSITION_XYZ
        float3 disp = snoise_grad(nc) * 0.25;
    #else // POSITION_RANDOM
        float3 disp = get_rotation_axis(uv) * snoise(nc);
    #endif
        disp *= _PositionNoise.z;

        return float4(origin + disp, nrand(uv, 2));
    }

    // Pass 1: Rotation kernel
    float4 frag_rotation(v2f_img i) : SV_Target
    {
        // Offset UV
        float2 uv = snap_uv(i.uv + _UVOffset);

        // Noise coordinate
        float3 nc = float3(uv * _RotationNoise.xy, _RotationNoise.w);

        // Angle
        float angle = snoise(nc) * _RotationNoise.z;

        // Rotation axis
    #if ROTATION_AXIS
        float3 axis = _RotationAxis;
    #else // ROTATION_RANDOM
        float3 axis = get_rotation_axis(uv);
    #endif

        return float4(axis * sin(angle), cos(angle));
    }

    // Pass 2: Scale kernel
    float4 frag_scale(v2f_img i) : SV_Target
    {
        // Offset UV
        float2 uv = snap_uv(i.uv + _UVOffset);

        // Random scale factor
        float vari = lerp(_RandomScale.x, _RandomScale.y, nrand(uv, 3));

        // Noise coordinate
        float3 nc = float3(uv * _ScaleNoise.xy, _ScaleNoise.w);

        // Scale factors for each axis
    #if SCALE_UNIFORM
        float3 axes = (float3)snoise(nc + float3(417.1, 471.2, 0));
    #else // SCALE_XYZ
        float3 axes = snoise_grad(nc) * 0.25;
    #endif
        axes = max(1.0 - _ScaleNoise.z * (axes + 1) * 0.5, 0);

        return float4(_BaseScale * axes * vari, nrand(uv, 4));
    }

    ENDCG

    SubShader
    {
        Pass
        {
            CGPROGRAM
            #pragma target 3.0
            #pragma vertex vert_img
            #pragma fragment frag_position
            ENDCG
        }
        Pass
        {
            CGPROGRAM
            #pragma target 3.0
            #pragma vertex vert_img
            #pragma fragment frag_rotation
            ENDCG
        }
        Pass
        {
            CGPROGRAM
            #pragma target 3.0
            #pragma vertex vert_img
            #pragma fragment frag_scale
            ENDCG
        }
    }
}
