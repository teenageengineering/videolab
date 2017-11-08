//
// Kino/Bokeh - Depth of field effect
//
// Copyright (C) 2016 Unity Technologies
// Copyright (C) 2015 Keijiro Takahashi
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

#include "Common.cginc"

sampler2D_float _CameraDepthTexture;

// Camera parameters
float _Distance;
float _LensCoeff;  // f^2 / (N * (S1 - f) * film_width * 2)

// Fragment shader: CoC visualization
half4 frag_CoC(v2f i) : SV_Target
{
    half4 src = tex2D(_MainTex, i.uv);
    float depth = LinearEyeDepth(SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, i.uvAlt));

    // Calculate the radiuses of CoC.
    float coc = (depth - _Distance) * _LensCoeff / depth;
    coc *= 80;

    // Visualize CoC (white -> red -> gray)
    half3 rgb = lerp(half3(1, 0, 0), half3(1, 1, 1), saturate(-coc));
    rgb = lerp(rgb, half3(0.4, 0.4, 0.4), saturate(coc));

    // Black and white image overlay
    rgb *= dot(src.rgb, 0.5 / 3) + 0.5;

    // Gamma correction
    rgb = lerp(rgb, GammaToLinearSpace(rgb), unity_ColorSpaceLuminance.w);

    return half4(rgb, src.a);
}
