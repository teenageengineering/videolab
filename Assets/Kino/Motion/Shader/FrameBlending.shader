//
// Kino/Motion - Motion blur effect
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
Shader "Hidden/Kino/Motion/FrameBlending"
{
    Properties
    {
        _MainTex           ("", 2D) = ""{}
        _History1LumaTex   ("", 2D) = ""{}
        _History2LumaTex   ("", 2D) = ""{}
        _History3LumaTex   ("", 2D) = ""{}
        _History4LumaTex   ("", 2D) = ""{}
        _History1ChromaTex ("", 2D) = ""{}
        _History2ChromaTex ("", 2D) = ""{}
        _History3ChromaTex ("", 2D) = ""{}
        _History4ChromaTex ("", 2D) = ""{}
    }

    CGINCLUDE

    #include "Common.cginc"

    #if !SHADER_API_GLES

    // MRT output struct for the compressor
    struct CompressorOutput
    {
        half4 luma : SV_Target0;
        half4 chroma : SV_Target1;
    };

    // Frame compression fragment shader
    CompressorOutput frag_FrameCompress(v2f_img i)
    {
        float sw = _ScreenParams.x;     // Screen width
        float pw = _ScreenParams.z - 1; // Pixel width

        // RGB to YCbCr convertion matrix
        const half3 kY  = half3( 0.299   ,  0.587   ,  0.114   );
        const half3 kCB = half3(-0.168736, -0.331264,  0.5     );
        const half3 kCR = half3( 0.5     , -0.418688, -0.081312);

        // 0: even column, 1: odd column
        half odd = frac(i.uv.x * sw * 0.5) > 0.5;

        // Calculate UV for chroma componetns.
        // It's between the even and odd columns.
        float2 uv_c = i.uv.xy;
        uv_c.x = (floor(uv_c.x * sw * 0.5) * 2 + 1) * pw;

        // Sample the source texture.
        half3 rgb_y = tex2D(_MainTex, i.uv).rgb;
        half3 rgb_c = tex2D(_MainTex, uv_c).rgb;

        #if !UNITY_COLORSPACE_GAMMA
        rgb_y = LinearToGammaSpace(rgb_y);
        rgb_c = LinearToGammaSpace(rgb_c);
        #endif

        // Convertion and subsampling
        CompressorOutput o;
        o.luma = dot(kY, rgb_y);
        o.chroma = dot(lerp(kCB, kCR, odd), rgb_c) + 0.5;
        return o;
    }

    #else

    // MRT might not be supported. Replace it with a null shader.
    half4 frag_FrameCompress(v2f_img i) : SV_Target
    {
        return 0;
    }

    #endif

    // Sample luma-chroma textures and convert to RGB
    half3 DecodeHistory(float2 uvLuma, float2 uvCb, float2 uvCr, sampler2D lumaTex, sampler2D chromaTex)
    {
        half y = tex2D(lumaTex, uvLuma).r;
        half cb = tex2D(chromaTex, uvCb).r - 0.5;
        half cr = tex2D(chromaTex, uvCr).r - 0.5;
        return y + half3(1.402 * cr, -0.34414 * cb - 0.71414 * cr, 1.772 * cb);
    }

    // Frame blending fragment shader
    half4 frag_FrameBlending(v2f_multitex i) : SV_Target
    {
        float sw = _MainTex_TexelSize.z; // Texture width
        float pw = _MainTex_TexelSize.x; // Texel width

        // UV for luma
        float2 uvLuma = i.uv1;

        // UV for Cb (even columns)
        float2 uvCb = i.uv1;
        uvCb.x = (floor(uvCb.x * sw * 0.5) * 2 + 0.5) * pw;

        // UV for Cr (even columns)
        float2 uvCr = uvCb;
        uvCr.x += pw;

        // Sample from the source image
        half4 src = tex2D(_MainTex, i.uv0);

        // Sampling and blending
        #if UNITY_COLORSPACE_GAMMA
        half3 acc = src.rgb;
        #else
        half3 acc = LinearToGammaSpace(src.rgb);
        #endif

        acc += DecodeHistory(uvLuma, uvCb, uvCr, _History1LumaTex, _History1ChromaTex) * _History1Weight;
        acc += DecodeHistory(uvLuma, uvCb, uvCr, _History2LumaTex, _History2ChromaTex) * _History2Weight;
        acc += DecodeHistory(uvLuma, uvCb, uvCr, _History3LumaTex, _History3ChromaTex) * _History3Weight;
        acc += DecodeHistory(uvLuma, uvCb, uvCr, _History4LumaTex, _History4ChromaTex) * _History4Weight;
        acc /= 1 + _History1Weight + _History2Weight +_History3Weight +_History4Weight;

        #if !UNITY_COLORSPACE_GAMMA
        acc = GammaToLinearSpace(acc);
        #endif

        return half4(acc, src.a);
    }

    // Frame blending fragment shader (without chroma subsampling)
    half4 frag_FrameBlendingRaw(v2f_multitex i) : SV_Target
    {
        half4 src = tex2D(_MainTex, i.uv0);
        half3 acc = src.rgb;
        acc += tex2D(_History1LumaTex, i.uv0) * _History1Weight;
        acc += tex2D(_History2LumaTex, i.uv0) * _History2Weight;
        acc += tex2D(_History3LumaTex, i.uv0) * _History3Weight;
        acc += tex2D(_History4LumaTex, i.uv0) * _History4Weight;
        acc /= 1 + _History1Weight + _History2Weight +_History3Weight +_History4Weight;
        return half4(acc, src.a);
    }


    ENDCG

    Subshader
    {
        // Pass 0: Frame compression
        Pass
        {
            ZTest Always Cull Off ZWrite Off
            CGPROGRAM
            #pragma multi_compile _ UNITY_COLORSPACE_GAMMA
            #pragma vertex vert_img
            #pragma fragment frag_FrameCompress
            #pragma target 3.0
            ENDCG
        }
        // Pass 1: Frame blending
        Pass
        {
            ZTest Always Cull Off ZWrite Off
            CGPROGRAM
            #pragma multi_compile _ UNITY_COLORSPACE_GAMMA
            #pragma vertex vert_Multitex
            #pragma fragment frag_FrameBlending
            #pragma target 3.0
            ENDCG
        }
        // Pass 2: Frame blending (without chroma subsampling)
        Pass
        {
            ZTest Always Cull Off ZWrite Off
            CGPROGRAM
            #pragma vertex vert_Multitex
            #pragma fragment frag_FrameBlendingRaw
            #pragma target 3.0
            ENDCG
        }
    }
}
