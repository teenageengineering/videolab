//
// KinoFringe - Chromatic aberration effect
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
Shader "Hidden/Kino/Fringe"
{
    Properties
    {
        _MainTex ("-", 2D) = "" {}
    }

    CGINCLUDE

    #pragma multi_compile _ AXIAL_SAMPLE_LOW AXIAL_SAMPLE_HIGH

    #include "UnityCG.cginc"

    sampler2D _MainTex;
    float4 _MainTex_TexelSize;

    float4 _CameraAspect; // (h/w, w/h, 1, 0)
    float _LateralShift;
    float _AxialStrength;
    float _AxialShift;

    // Poisson disk sample points
    #if AXIAL_SAMPLE_LOW
    static const uint SAMPLE_NUM = 8;
    static const float2 POISSON_SAMPLES[SAMPLE_NUM] =
    {
        float2( 0.373838022357f, 0.662882019975f ),
        float2( -0.335774814282f, -0.940070127794f ),
        float2( -0.9115721822f, 0.324130702404f ),
        float2( 0.837294074715f, -0.504677167232f ),
        float2( -0.0500874221246f, -0.0917990757772f ),
        float2( -0.358644570242f, 0.906381100284f ),
        float2( 0.961200130218f, 0.219135111748f ),
        float2( -0.896666615007f, -0.440304757692f )
    };
    #else
    static const uint SAMPLE_NUM = 16;
    static const float2 POISSON_SAMPLES[SAMPLE_NUM] =
    {
        float2( 0.0984258332809f, 0.918808284462f ),
        float2( 0.00259138629413f, -0.999838959623f ),
        float2( -0.987959729023f, -0.00429660140761f ),
        float2( 0.981234239267f, -0.140666219895f ),
        float2( -0.0212157973013f, -0.0443286928994f ),
        float2( -0.652058534734f, 0.695078086985f ),
        float2( -0.68090417832f, -0.681862769398f ),
        float2( 0.779643686501f, 0.603399060386f ),
        float2( 0.67941165083f, -0.731372789969f ),
        float2( 0.468821477499f, -0.251621416756f ),
        float2( 0.278991228738f, 0.39302189329f ),
        float2( -0.191188273806f, -0.527976638433f ),
        float2( -0.464789669525f, 0.216311272754f ),
        float2( -0.559833960421f, -0.256176089172f ),
        float2( 0.65988403582f, 0.170056284903f ),
        float2( -0.170289189543f, 0.551561042407f )
    };
    #endif

    // Poisson filter
    half3 poisson_filter(float2 uv)
    {
        half3 acc = 0;
        for (uint i = 0; i < SAMPLE_NUM; i++)
        {
            float2 disp = POISSON_SAMPLES[i];
            disp *= _CameraAspect.yz * _AxialShift * 0.02;
            acc += tex2D(_MainTex, uv + disp).rgb;
        }
        return acc / SAMPLE_NUM;
    }

    // Rec.709 Luminance
    half luminance(half3 rgb)
    {
        return dot(rgb, half3(0.2126, 0.7152, 0.0722));
    }

    // CA filter
    half4 frag(v2f_img i) : SV_Target
    {
        float2 spc = (i.uv - 0.5) * _CameraAspect.xz;
        float r2 = dot(spc, spc);

        float f_r = 1.0 + r2 * _LateralShift * -0.02;
        float f_b = 1.0 + r2 * _LateralShift * +0.02;

        half4 src = tex2D(_MainTex, i.uv);
        src.r = tex2D(_MainTex, (i.uv - 0.5) * f_r + 0.5).r;
        src.b = tex2D(_MainTex, (i.uv - 0.5) * f_b + 0.5).b;

        #if AXIAL_SAMPLE_LOW || AXIAL_SAMPLE_HIGH
        half3 blur = poisson_filter(i.uv);
        half ldiff = luminance(blur) - luminance(src.rgb);
        src.rb = max(src.rb, blur.rb * ldiff * _AxialStrength);
        #endif

        return src;
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
