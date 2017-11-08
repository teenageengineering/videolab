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
Shader "Hidden/Kino/Voronoi/Contour"
{
    Properties
    {
        _MainTex("", 2D) = ""{}
        _ColorTexture("", 2D) = ""{}
        _NormalTexture("", 2D) = ""{}
        _LineColor("", Color) = (1, 1, 1)
    }

    CGINCLUDE

    #include "UnityCG.cginc"
    #include "Common.cginc"

    sampler2D _MainTex;
    sampler2D _ColorTexture;
    sampler2D _NormalTexture;
    float2 _NormalTexture_TexelSize;

    half4 _LineColor;
    half3 _CellColorGamma;
    half3 _BgColorGamma;

    half _CellExponent;
    half _Blend;

    half4 frag(v2f_img i) : SV_Target
    {
        float4 disp = _NormalTexture_TexelSize.xyxy * float4(1, 1, -1, 0);

        // four sample points for the roberts cross operator
        float2 uv0 = i.uv;           // TL
        float2 uv1 = i.uv + disp.xy; // BR
        float2 uv2 = i.uv + disp.xw; // TR
        float2 uv3 = i.uv + disp.wy; // BL

        // sample normal vector values from the g-buffer
        float3 n0 = tex2D(_NormalTexture, uv0);
        float3 n1 = tex2D(_NormalTexture, uv1);
        float3 n2 = tex2D(_NormalTexture, uv2);
        float3 n3 = tex2D(_NormalTexture, uv3);

        // roberts cross operator
        float3 ng1 = n1 - n0;
        float3 ng2 = n3 - n2;
        float ng = sqrt(dot(ng1, ng1) + dot(ng2, ng2));

        // thresholding
        float edge = saturate((ng - _NormalTexture_TexelSize.x * 2) * 100);

        // cell level
        half cl = pow(Luma(tex2D(_ColorTexture, i.uv).rgb), _CellExponent);

        // cell color
        half3 cc = ConvertToLinear(lerp(_BgColorGamma, _CellColorGamma, cl));

        // combined color
        half3 combined = lerp(cc, _LineColor.rgb, edge * _LineColor.a);

        // blend with the source image
        half4 source = tex2D(_MainTex, i.uv);
        return half4(lerp(source.rgb, combined, _Blend), source.a);
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
            #pragma target 3.0
            ENDCG
        }
    }
}
