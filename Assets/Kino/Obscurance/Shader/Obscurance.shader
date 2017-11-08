//
// Kino/Obscurance - Screen space ambient obscurance image effect
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
Shader "Hidden/Kino/Obscurance"
{
    Properties
    {
        _MainTex("", 2D) = ""{}
        _OcclusionTexture("", 2D) = ""{}
    }
    SubShader
    {
        // 0: Occlusion estimation with CameraDepthTexture
        Pass
        {
            ZTest Always Cull Off ZWrite Off
            CGPROGRAM
            #define SOURCE_DEPTH
            #include "Obscurance.cginc"
            #pragma vertex vert
            #pragma fragment frag_ao
            #pragma target 3.0
            ENDCG
        }
        // 1: Occlusion estimation with CameraDepthNormalsTexture
        Pass
        {
            ZTest Always Cull Off ZWrite Off
            CGPROGRAM
            #define SOURCE_DEPTHNORMALS
            #include "Obscurance.cginc"
            #pragma vertex vert
            #pragma fragment frag_ao
            #pragma target 3.0
            ENDCG
        }
        // 2: Occlusion estimation with G-Buffer
        Pass
        {
            ZTest Always Cull Off ZWrite Off
            CGPROGRAM
            #define SOURCE_GBUFFER
            #include "Obscurance.cginc"
            #pragma vertex vert
            #pragma fragment frag_ao
            #pragma target 3.0
            ENDCG
        }
        // 3: Separable blur (horizontal pass) with CameraDepthNormalsTexture
        Pass
        {
            ZTest Always Cull Off ZWrite Off
            CGPROGRAM
            #define SOURCE_DEPTHNORMALS
            #define BLUR_HORIZONTAL
            #define BLUR_SAMPLE_CENTER_NORMAL
            #include "SeparableBlur.cginc"
            #pragma vertex vert
            #pragma fragment frag_blur
            #pragma target 3.0
            ENDCG
        }
        // 4: Separable blur (horizontal pass) with G-Buffer
        Pass
        {
            ZTest Always Cull Off ZWrite Off
            CGPROGRAM
            #define SOURCE_GBUFFER
            #define BLUR_HORIZONTAL
            #define BLUR_SAMPLE_CENTER_NORMAL
            #include "SeparableBlur.cginc"
            #pragma vertex vert
            #pragma fragment frag_blur
            #pragma target 3.0
            ENDCG
        }
        // 5: Separable blur (vertical pass)
        Pass
        {
            ZTest Always Cull Off ZWrite Off
            CGPROGRAM
            #define BLUR_VERTICAL
            #include "SeparableBlur.cginc"
            #pragma vertex vert
            #pragma fragment frag_blur
            #pragma target 3.0
            ENDCG
        }
        // 6: Final composition
        Pass
        {
            ZTest Always Cull Off ZWrite Off
            CGPROGRAM
            #include "Composition.cginc"
            #pragma vertex vert
            #pragma fragment frag_composition
            #pragma target 3.0
            ENDCG
        }
        // 7: Final composition (ambient only mode)
        Pass
        {
            Blend Zero OneMinusSrcColor, Zero OneMinusSrcAlpha
            ZTest Always Cull Off ZWrite Off
            CGPROGRAM
            #include "Composition.cginc"
            #pragma vertex vert_composition_gbuffer
            #pragma fragment frag_composition_gbuffer
            #pragma target 3.0
            ENDCG
        }
    }
}
