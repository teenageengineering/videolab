//
// Kino/Vision - Frame visualization utility
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
Shader "Hidden/Kino/Vision"
{
    Properties
    {
        _MainTex("", 2D) = ""{}
    }
    Subshader
    {
        // Depth with camera depth texture
        Pass
        {
            ZTest Always Cull Off ZWrite Off
            CGPROGRAM
            #define USE_CAMERA_DEPTH
            #include "Depth.cginc"
            #pragma multi_compile _ UNITY_COLORSPACE_GAMMA
            #pragma vertex vert_common
            #pragma fragment frag_depth
            #pragma target 3.0
            ENDCG
        }
        // Depth with camera depth normals texture
        Pass
        {
            ZTest Always Cull Off ZWrite Off
            CGPROGRAM
            #define USE_CAMERA_DEPTH_NORMALS
            #include "Depth.cginc"
            #pragma multi_compile _ UNITY_COLORSPACE_GAMMA
            #pragma vertex vert_common
            #pragma fragment frag_depth
            #pragma target 3.0
            ENDCG
        }
        // Depth with camera depth normals texture
        Pass
        {
            ZTest Always Cull Off ZWrite Off
            CGPROGRAM
            #define USE_CAMERA_DEPTH_NORMALS
            #include "Normals.cginc"
            #pragma multi_compile _ UNITY_COLORSPACE_GAMMA
            #pragma vertex vert_common
            #pragma fragment frag_normals
            #pragma target 3.0
            ENDCG
        }
        // Depth with G buffer
        Pass
        {
            ZTest Always Cull Off ZWrite Off
            CGPROGRAM
            #define USE_GBUFFER
            #include "Normals.cginc"
            #pragma multi_compile _ UNITY_COLORSPACE_GAMMA
            #pragma vertex vert_common
            #pragma fragment frag_normals
            #pragma target 3.0
            ENDCG
        }
        // Motion vectors overlay
        Pass
        {
            ZTest Always Cull Off ZWrite Off
            CGPROGRAM
            #include "MotionVectors.cginc"
            #pragma multi_compile _ UNITY_COLORSPACE_GAMMA
            #pragma vertex vert_common
            #pragma fragment frag_overlay
            #pragma target 3.0
            ENDCG
        }
        // Motion vectors arrows
        Pass
        {
            Blend SrcAlpha OneMinusSrcAlpha
            ZTest Always Cull Off ZWrite Off
            CGPROGRAM
            #include "MotionVectors.cginc"
            #pragma multi_compile _ UNITY_COLORSPACE_GAMMA
            #pragma vertex vert_arrows
            #pragma fragment frag_arrows
            #pragma target 3.0
            ENDCG
        }
    }
}
