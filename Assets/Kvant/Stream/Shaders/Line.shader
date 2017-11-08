// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

//
// Line shader for Stream
//
// Vertex format:
// position.x  = texture switch (0/1)
// texcoord.xy = uv for ParticleTex
//
Shader "Hidden/Kvant/Stream/Line"
{
    Properties
    {
        _ParticleTex1 ("-", 2D)    = ""{}
        _ParticleTex2 ("-", 2D)    = ""{}
        [HDR] _Color  ("-", Color) = (1, 1, 1, 1)
        _Tail         ("-", Float) = 1
    }

    CGINCLUDE

    #pragma multi_compile_fog

    #include "UnityCG.cginc"

    struct appdata
    {
        float4 position : POSITION;
        float2 texcoord : TEXCOORD0;
    };

    struct v2f
    {
        float4 position : SV_POSITION;
        half4 color : COLOR;
        UNITY_FOG_COORDS(0)
    };

    sampler2D _ParticleTex1;
    float4 _ParticleTex1_TexelSize;

    sampler2D _ParticleTex2;
    float4 _ParticleTex2_TexelSize;

    half4 _Color;
    half _Tail;

    v2f vert(appdata v)
    {
        v2f o;

        float2 uv = v.texcoord.xy + _ParticleTex1_TexelSize.xy / 2;

        float4 p1 = tex2Dlod(_ParticleTex1, float4(uv, 0, 0));
        float4 p2 = tex2Dlod(_ParticleTex2, float4(uv, 0, 0));
        float sw = v.position.x;

        if (p1.w < 0)
        {
            o.position = UnityObjectToClipPos(float4(p2.xyz, 1));
        }
        else
        {
            float3 p = lerp(p2.xyz, p1.xyz, (1.0 - sw) * _Tail);
            o.position = UnityObjectToClipPos(float4(p, 1));
        }

        o.color = _Color;
        o.color.a *= sw;

        UNITY_TRANSFER_FOG(o, o.position);

        return o;
    }

    half4 frag(v2f i) : SV_Target
    {
        fixed4 c = i.color;
        UNITY_APPLY_FOG(i.fogCoord, c);
        return c;
    }

    ENDCG

    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" }
        Pass
        {
            Blend SrcAlpha OneMinusSrcAlpha
            CGPROGRAM
            #pragma target 3.0
            #pragma vertex vert
            #pragma fragment frag
            ENDCG
        }
    } 
}
