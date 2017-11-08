//
// Kino/Voronoi - Voronoi diagram image effect
//
// Copyright (C) 2016 Keijiro Takahashi
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
//
Shader "Hidden/Kino/Voronoi/Cone"
{
    CGINCLUDE

    #include "UnityCG.cginc"
    #include "Common.cginc"
    #include "SimplexNoise2D.cginc"

    sampler2D _Source;
    float _Aspect;
    float _RangeMin;
    float _RangeMax;
    float _RandomSeed;

    struct appdata
    {
        float4 vertex : POSITION;
        float2 uv : TEXCOORD0;
    };

    struct v2f
    {
        float4 vertex : SV_POSITION;
        float3 normal : NORMAL;
        half4 color : COLOR;
    };

    struct FragOutput
    {
        half4 color : COLOR0;
        half4 normal : COLOR1;
    };

    // PRNG function (0-1 range)
    float Random01(float seed1, float seed2)
    {
        float2 uv = float2(seed1, seed2 + _RandomSeed);
        return frac(sin(dot(uv, float2(12.9898, 78.233))) * 43758.5453);
    }

    // Sample point generator
    float2 SamplePoint(float id)
    {
        float rx = Random01(id, 0);
        float ry = Random01(id, 1);
        float nx = snoise(float2(id, _Time.x)) * 0.2;
        float ny = snoise(float2(_Time.x, id)) * 0.2;
        return float2(rx + nx, ry + ny);
    }

    v2f vert(appdata v)
    {
        // vertex id
        float id = v.uv.y;

        // sample point
        float2 spos = SamplePoint(id);

        // cone vertex position (without transfomation)
        float4 vpos = v.vertex * float4(2, _Aspect * 2, 1, 1);
        #if UNITY_UV_STARTS_AT_TOP
        vpos.xy += float2(-1, 1) + spos * float2(2, -2);
        #else
        vpos.xy += spos * 2 - 1;
        #endif

        // normal vector (remap position to 0-1 range)
        float3 vnrm = v.vertex.xyz * float3(0.5, 0.5, 1) + float3(0.5, 0.5, 0);

        // get a threshold randomly within the value range
        float thr = lerp(_RangeMin, _RangeMax, Random01(id, 2));

        // sample source color and reject the vertex if it's under the threshold
        half3 col = tex2Dlod(_Source, float4(spos, 0, 0)).rgb;
        vpos.xy += (Luma(col) < thr) * 10000;

        // shader output
        v2f o;
        o.vertex = vpos;
        o.normal = vnrm;
        o.color = half4(col, 0);
        return o;
    }

    FragOutput frag(v2f i)
    {
        FragOutput o;
        o.color = i.color;
        o.normal = float4(i.normal, 1);
        return o;
    }

    ENDCG
    SubShader
    {
        Pass
        {
            Cull Off
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 3.0
            ENDCG
        }
    }
}
