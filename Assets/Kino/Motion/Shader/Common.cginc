// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

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

#include "UnityCG.cginc"

// Workaround for #827031 (PS4/PS Vita)
#if defined(SHADER_API_PSSL)
#define sampler2D_half sampler2D_float
#endif

// Main source texture
sampler2D _MainTex;
float4 _MainTex_TexelSize;

// Camera depth texture
sampler2D_float _CameraDepthTexture;
float4 _CameraDepthTexture_TexelSize;

// Camera motion vectors texture
sampler2D_half _CameraMotionVectorsTexture;
float4 _CameraMotionVectorsTexture_TexelSize;

// Pakced velocity texture (2/10/10/10)
sampler2D_half _VelocityTex;
float4 _VelocityTex_TexelSize;

// NeighborMax texture
sampler2D_half _NeighborMaxTex;
float4 _NeighborMaxTex_TexelSize;

// Velocity scale factor
float _VelocityScale;

// TileMax filter parameters
int _TileMaxLoop;
float2 _TileMaxOffs;

// Maximum blur radius (in pixels)
half _MaxBlurRadius;
float _RcpMaxBlurRadius;

// Filter parameters/coefficients
half _LoopCount;

// History buffer for frame blending
sampler2D _History1LumaTex;
sampler2D _History2LumaTex;
sampler2D _History3LumaTex;
sampler2D _History4LumaTex;

sampler2D _History1ChromaTex;
sampler2D _History2ChromaTex;
sampler2D _History3ChromaTex;
sampler2D _History4ChromaTex;

half _History1Weight;
half _History2Weight;
half _History3Weight;
half _History4Weight;

// Linearize depth value sampled from the camera depth texture.
float LinearizeDepth(float z)
{
    float isOrtho = unity_OrthoParams.w;
    float isPers = 1 - unity_OrthoParams.w;
    z *= _ZBufferParams.x;
    return (1 - isOrtho * z) / (isPers * z + _ZBufferParams.y);
}

// Vertex shader for multiple texture blitting
struct v2f_multitex
{
    float4 pos : SV_POSITION;
    float2 uv0 : TEXCOORD0;
    float2 uv1 : TEXCOORD1;
};

v2f_multitex vert_Multitex(appdata_full v)
{
    v2f_multitex o;
    o.pos = UnityObjectToClipPos(v.vertex);
    o.uv0 = v.texcoord.xy;
    o.uv1 = v.texcoord.xy;
#if UNITY_UV_STARTS_AT_TOP
    if (_MainTex_TexelSize.y < 0.0)
        o.uv1.y = 1.0 - v.texcoord.y;
#endif
    return o;
}
