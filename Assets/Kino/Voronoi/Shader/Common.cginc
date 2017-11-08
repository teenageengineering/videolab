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

half Luma(half3 c)
{
#if defined(UNITY_NO_LINEAR_COLORSPACE)
    return dot(c, unity_ColorSpaceLuminance.rgb);
#else
    half3 rec709 = half3(0.212, 0.701, 0.087);
    half3 linc = LinearToGammaSpace(c);
    return dot(lerp(c, linc, unity_ColorSpaceLuminance.a), rec709);
#endif
}

half3 ConvertToLinear(half3 c)
{
    half3 lc = GammaToLinearSpace(c);
    return lerp(c, lc, unity_ColorSpaceLuminance.a);
}
