//
// KinoRamp - Color ramp overlay
//
// Copyright (C) 2015 Keijiro Takahashi
//
// Permission is hereby granted, free of charge, to any person obtaining a copy of
// this software and associated documentation files (the "Software"), to deal in
// the Software without restriction, including without limitation the rights to
// use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of
// the Software, and to permit persons to whom the Software is furnished to do so,
// subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS
// FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR
// COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER
// IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN
// CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//
Shader "Hidden/Kino/Ramp"
{
    Properties
    {
        _MainTex ("-", 2D) = "black" {}
        _Color1 ("-", Color) = (0, 0, 1, 0)
        _Color2 ("-", Color) = (1, 0, 0, 0)
        _Direction ("-", Vector) = (0, 1, 0, 0)
    }

    CGINCLUDE

    #pragma multi_compile _MULTIPLY _SCREEN _OVERLAY _HARDLIGHT _SOFTLIGHT
    #pragma multi_compile _ _LINEAR
    #pragma multi_compile _ _DEBUG

    #include "UnityCG.cginc"

    sampler2D _MainTex;
    half4 _Color1;
    half4 _Color2;
    float2 _Direction;

    #if _LINEAR

    // Color space conversion between sRGB and linear space.
    // http://chilliant.blogspot.com/2012/08/srgb-approximations-for-hlsl.html

    half3 srgb_to_linear(half3 c)
    {
        return c * (c * (c * 0.305306011 + 0.682171111) + 0.012522878);
    }

    half3 linear_to_srgb(half3 c)
    {
        return max(1.055 * pow(c, 0.416666667) - 0.055, 0.0);
    }

    #endif

    half4 frag(v2f_img i) : SV_Target
    {
        half4 src = tex2D(_MainTex, i.uv);

        half3 c_a = src.rgb;
        half3 grad1 = _Color1.rgb;
        half3 grad2 = _Color2.rgb;

        #if _LINEAR
        c_a = linear_to_srgb(c_a);
        grad1 = linear_to_srgb(grad1);
        grad2 = linear_to_srgb(grad2);
        #endif

        float param = dot(i.uv - 0.5, _Direction);
        half3 c_b = lerp(grad1, grad2, param + 0.5);

        #if _DEBUG
        half3 c_f = c_b;

        #elif _MULTIPLY
        half3 c_f = c_a * c_b;

        #elif _SCREEN
        half3 c_f = 1.0 - (1.0 - c_a) * (1.0 - c_b);

        #elif _SOFTLIGHT
        half3 c_u = c_a * c_b * 2.0 + (1.0 - c_b * 2.0) * c_a * c_a;
        half3 c_d = (1.0 - c_b) * c_a * 2.0 + (c_b * 2.0 - 1.0) * sqrt(c_a);
        half3 c_f = lerp(c_u, c_d, c_b > 0.5);

        #else
        half3 c_u = c_a * c_b * 2.0;
        half3 c_d = 1.0 - (1.0 - c_a) * (1.0 - c_b) * 2.0;

        #if _OVERLAY
        half3 c_f = lerp(c_u, c_d, c_a > 0.5);

        #else // _HARDLIGHT
        half3 c_f = lerp(c_u, c_d, c_b > 0.5);

        #endif
        #endif

        #if _LINEAR
        c_f = srgb_to_linear(c_f);
        #endif

        return half4(c_f, src.a);
    }

    ENDCG

    SubShader
    {
        Pass
        {
            ZTest Always Cull Off ZWrite Off
            CGPROGRAM
            #pragma vertex vert_img
            #pragma fragment frag
            ENDCG
        }
    }
}
