//
// Surface shader for Swarm
//
// Texture format:
//
// _PositionTex.xyz = position
// _PositionTex.w   = random number
//
Shader "Hidden/Kvant/Swarm/Surface"
{
    Properties
    {
        _PositionTex ("-", 2D) = ""{}
        _Color1 ("-", Color) = (1, 1, 1, 1)
        _Color2 ("-", Color) = (1, 1, 1, 1)
    }

    CGINCLUDE

    #pragma multi_compile COLOR_RANDOM COLOR_SMOOTH

    sampler2D _PositionTex;
    float4 _PositionTex_TexelSize;

    half4 _Color1;
    half4 _Color2;
    half _Metallic;
    half _Smoothness;

    float2 _LineWidth; // min, max
    float _Throttle;
    float _Flip;

    float2 _BufferOffset;

    struct Input {
        half color;
    };

    // pseudo random number generator
    float nrand(float2 uv, float salt)
    {
        uv += float2(salt, 0);
        return frac(sin(dot(uv, float2(12.9898, 78.233))) * 43758.5453);
    }

    void vert(inout appdata_full v, out Input data)
    {
        UNITY_INITIALIZE_OUTPUT(Input, data);

        float4 uv = float4(v.texcoord + _BufferOffset, 0, 0);
        float4 duv = float4(_PositionTex_TexelSize.x, 0, 0, 0);

        // line number
        float ln = uv.y;

        // adjacent vertices
        float3 p1 = tex2Dlod(_PositionTex, uv - duv * 2).xyz;
        float3 p2 = tex2Dlod(_PositionTex, uv          ).xyz;
        float3 p3 = tex2Dlod(_PositionTex, uv + duv * 2).xyz;

        // binormal vector
        float3 bn = normalize(cross(p3 - p2, p2 - p1)) * _Flip;

        // line width
        float lw = lerp(_LineWidth.x, _LineWidth.y, nrand(ln, 10));
        lw *= saturate((_Throttle - ln) / _PositionTex_TexelSize.y);

        v.vertex.xyz = p2 + bn * lw * v.vertex.x;
        v.normal = normalize(cross(bn, p2 - p1));

#if COLOR_RANDOM
        data.color = nrand(ln, 11);
#else
        data.color = ln;
#endif
    }

    ENDCG

    SubShader
    {
        Tags { "RenderType"="Opaque" }

        CGPROGRAM

        #pragma surface surf Standard vertex:vert nolightmap addshadow
        #pragma target 3.0

        void surf(Input IN, inout SurfaceOutputStandard o)
        {
            o.Albedo = lerp(_Color1, _Color2, IN.color);
            o.Metallic = _Metallic;
            o.Smoothness = _Smoothness;
        }

        ENDCG
    }
}
