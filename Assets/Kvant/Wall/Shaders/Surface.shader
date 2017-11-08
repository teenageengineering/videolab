//
// Opaque surface shader for Wall
//
// Vertex format:
// position.xyz = vertex position
// texcoord0.xy = uv for texturing
// texcoord1.xy = uv for position/rotation/scale texture
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
Shader "Kvant/Wall/Surface"
{
    Properties
    {
        _PositionTex  ("-", 2D) = "black"{}
        _RotationTex  ("-", 2D) = "red"{}
        _ScaleTex     ("-", 2D) = "white"{}

        [Enum(Single, 0, Random, 1)]
        _ColorMode    ("-", Float) = 0
        _Color        ("-", Color) = (1, 1, 1, 1)
        _Color2       ("-", Color) = (0.5, 0.5, 0.5, 1)

        _Metallic     ("-", Range(0,1)) = 0.5
        _Smoothness   ("-", Range(0,1)) = 0.5

        _MainTex      ("-", 2D) = "white"{}
        _NormalMap    ("-", 2D) = "bump"{}
        _NormalScale  ("-", Range(0,2)) = 1
        _OcclusionMap ("-", 2D) = "white"{}
        _OcclusionStr ("-", Range(0,1)) = 1

        [Toggle]
        _RandomUV     ("-", Float) = 0
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }

        CGPROGRAM

        #pragma surface surf Standard vertex:vert nolightmap addshadow
        #pragma shader_feature _ALBEDOMAP
        #pragma shader_feature _NORMALMAP
        #pragma shader_feature _OCCLUSIONMAP
        #pragma target 3.0

        sampler2D _PositionTex;
        sampler2D _RotationTex;
        sampler2D _ScaleTex;
        float2 _BufferOffset;

        half _ColorMode;
        half4 _Color;
        half4 _Color2;

        half _Metallic;
        half _Smoothness;

        sampler2D _MainTex;
        sampler2D _NormalMap;
        half _NormalScale;
        sampler2D _OcclusionMap;
        half _OcclusionStr;

        half _RandomUV;

        // Quaternion multiplication.
        // http://mathworld.wolfram.com/Quaternion.html
        float4 qmul(float4 q1, float4 q2)
        {
            return float4(
                q2.xyz * q1.w + q1.xyz * q2.w + cross(q1.xyz, q2.xyz),
                q1.w * q2.w - dot(q1.xyz, q2.xyz)
            );
        }

        // Rotate a vector with a rotation quaternion.
        // http://mathworld.wolfram.com/Quaternion.html
        float3 rotate_vector(float3 v, float4 r)
        {
            float4 r_c = r * float4(-1, -1, -1, 1);
            return qmul(r, qmul(float4(v, 0), r_c)).xyz;
        }

        struct Input
        {
            float2 uv_MainTex;
            half4 color : COLOR;
        };

        void vert(inout appdata_full v)
        {
            float4 uv = float4(v.texcoord1.xy + _BufferOffset, 0, 0);

            float4 p = tex2Dlod(_PositionTex, uv);
            float4 r = tex2Dlod(_RotationTex, uv);
            float4 s = tex2Dlod(_ScaleTex, uv);

            v.vertex.xyz = rotate_vector(v.vertex.xyz * s.xyz, r) + p.xyz;
            v.normal = rotate_vector(v.normal, r);
        #if _NORMALMAP
            v.tangent.xyz = rotate_vector(v.tangent.xyz, r);
        #endif
            v.color = lerp(_Color, _Color2, p.w * _ColorMode);
            v.texcoord.xy += float2(p.w, s.w) * _RandomUV;
        }

        void surf(Input IN, inout SurfaceOutputStandard o)
        {
        #if _ALBEDOMAP
            half4 c = tex2D(_MainTex, IN.uv_MainTex);
            o.Albedo = IN.color.rgb * c.rgb;
            o.Alpha = IN.color.a * c.a;
        #else
            o.Albedo = IN.color.rgb;
            o.Alpha = IN.color.a;
        #endif

        #if _NORMALMAP
            half4 n = tex2D(_NormalMap, IN.uv_MainTex);
            o.Normal = UnpackScaleNormal(n, _NormalScale);
        #endif

        #if _OCCLUSIONMAP
            half4 occ = tex2D(_OcclusionMap, IN.uv_MainTex);
            o.Occlusion = lerp((half4)1, occ, _OcclusionStr);
        #endif

            o.Metallic = _Metallic;
            o.Smoothness = _Smoothness;
        }

        ENDCG
    }
    CustomEditor "Kvant.WallMaterialEditor"
}
