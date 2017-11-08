//
// Kino/Slitscan - Slit-scan image effect
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
using UnityEngine;

namespace Kino
{
    public partial class Slitscan
    {
        //
        // Frame storage class
        //
        // Compresses and stores a frame image. The memory footprint of the
        // image is reduced to 9 bit/pixel with using the chroma subsampling
        // technique. If the frame buffer is using 1920 x 1080 x 32 bpp,
        // the footprint is reduced from 7.91 MiB to 2.22 MiB.
        //
        class Frame
        {
            public RenderTexture yTexture;
            public RenderTexture cgTexture;
            public RenderTexture coTexture;

            public void Prepare(int width, int height)
            {
                if (yTexture != null)
                    if (yTexture.width != width || yTexture.height != height)
                        Release();

                if (yTexture == null)
                    yTexture = AllocateTemporaryRT(width, height);

                if (cgTexture == null)
                    cgTexture = AllocateTemporaryRT(width / 4, height / 4);

                if (coTexture == null)
                    coTexture = AllocateTemporaryRT(width / 4, height / 4);
            }

            public void Release()
            {
                if (yTexture != null)
                    RenderTexture.ReleaseTemporary(yTexture);

                if (cgTexture != null)
                    RenderTexture.ReleaseTemporary(cgTexture);

                if (coTexture != null)
                    RenderTexture.ReleaseTemporary(coTexture);

                yTexture = null;
                cgTexture = null;
                coTexture = null;
            }

            static RenderTexture AllocateTemporaryRT(int width, int height)
            {
                var rt = RenderTexture.GetTemporary(width, height, 0, RenderTextureFormat.R8);
                rt.filterMode = FilterMode.Bilinear;
                rt.wrapMode = TextureWrapMode.Clamp;
                return rt;
            }
        }
    }
}
