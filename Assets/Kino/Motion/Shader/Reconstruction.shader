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
Shader "Hidden/Kino/Motion/Reconstruction"
{
    Properties
    {
        _MainTex        ("", 2D) = ""{}
        _VelocityTex    ("", 2D) = ""{}
        _NeighborMaxTex ("", 2D) = ""{}
    }
    Subshader
    {
        // Pass 0: Velocity texture setup
        Pass
        {
            ZTest Always Cull Off ZWrite Off
            CGPROGRAM
            #include "Prefilter.cginc"
            #pragma vertex vert_img
            #pragma fragment frag_VelocitySetup
            #pragma target 3.0
            ENDCG
        }
        // Pass 1: TileMax filter (2 pixels width with normalization)
        Pass
        {
            ZTest Always Cull Off ZWrite Off
            CGPROGRAM
            #include "Prefilter.cginc"
            #pragma vertex vert_img
            #pragma fragment frag_TileMax1
            #pragma target 3.0
            ENDCG
        }
        // Pass 2: TileMax filter (2 pixels width)
        Pass
        {
            ZTest Always Cull Off ZWrite Off
            CGPROGRAM
            #include "Prefilter.cginc"
            #pragma vertex vert_img
            #pragma fragment frag_TileMax2
            #pragma target 3.0
            ENDCG
        }
        // Pass 3: TileMax filter (variable width)
        Pass
        {
            ZTest Always Cull Off ZWrite Off
            CGPROGRAM
            #include "Prefilter.cginc"
            #pragma vertex vert_img
            #pragma fragment frag_TileMaxV
            #pragma target 3.0
            ENDCG
        }
        // Pass 4: NeighborMax filter
        Pass
        {
            ZTest Always Cull Off ZWrite Off
            CGPROGRAM
            #include "Prefilter.cginc"
            #pragma vertex vert_img
            #pragma fragment frag_NeighborMax
            #pragma target 3.0
            ENDCG
        }
        // Pass 5: Reconstruction filter
        Pass
        {
            ZTest Always Cull Off ZWrite Off
            CGPROGRAM
            #include "Reconstruction.cginc"
            #pragma vertex vert_Multitex
            #pragma fragment frag_Reconstruction
            #pragma target 3.0
            ENDCG
        }
    }
}
