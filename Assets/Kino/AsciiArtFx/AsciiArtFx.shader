Shader "Hidden/ASCII Art Fx"
{
    Properties 
    {
        _MainTex("Source Image", 2D) = "" {}
        _Color("Color Tint", Color) = (1, 1, 1, 1)
        _Alpha("Alpha Blending", Float) = 1
        _Scale("Scale Factor", Float) = 1
    }

CGINCLUDE

#include "UnityCG.cginc"

sampler2D _MainTex;
float4 _MainTex_TexelSize;
float4 _Color;
float _Scale;
float _Alpha;

struct v2f
{
    float4 position : SV_POSITION;
    float2 texcoord : TEXCOORD0;
};   

float character(float n, float2 p) 
{
#ifdef UNITY_HALF_TEXEL_OFFSET
    float2 offs = float2(2.5f, 2.5f);
#else
    float2 offs = float2(2, 2);
#endif
    p = floor(p * float2(4, -4) + offs);
    if (clamp(p.x, 0, 4) == p.x && clamp(p.y, 0, 4) == p.y)
    {
        float c = fmod(n / exp2(p.x + 5 * p.y), 2);
        if (int(c) == 1) return 1;
    }   
    return 0;
}

float4 frag(v2f i) : SV_Target
{
    float2 texel = _MainTex_TexelSize.xy * _Scale;
    float2 uv = i.texcoord.xy / texel;
    float3 c = tex2D(_MainTex, floor(uv / 8) * 8 * texel).rgb;

    float gray = (c.r + c.g + c.b) / 3;

    float n =  65536;              // .

    if (gray > 0.2f) n = 65600;    // :
    if (gray > 0.3f) n = 332772;   // *
    if (gray > 0.4f) n = 15255086; // o
    if (gray > 0.5f) n = 23385164; // &
    if (gray > 0.6f) n = 15252014; // 8
    if (gray > 0.7f) n = 13199452; // @
    if (gray > 0.8f) n = 11512810; // #

    float2 p = fmod(uv / 4, 2) - 1;
    c *= character(n, p);

    float4 src = tex2D(_MainTex, i.texcoord.xy);
    return lerp(src, float4(c * _Color.rgb, _Color.a), _Alpha);
}

ENDCG

    SubShader
    {
        Pass
        {
            ZTest Always Cull Off ZWrite Off
            Fog { Mode off }
            CGPROGRAM
            #pragma target 3.0
            #pragma vertex vert_img
            #pragma fragment frag
            ENDCG
        }
    }
}
