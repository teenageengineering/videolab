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
using UnityEngine;
using UnityEngine.Rendering;

namespace Kino
{
    public partial class Motion
    {
        //
        // Multiple frame blending filter
        //
        // This filter acts like a finite impluse response filter; stores
        // succeeding four frames and calculate the weighted average of them.
        //
        // To save memory, it compresses frame images with the 4:2:2 chroma
        // subsampling scheme. This requires MRT support. If the current
        // environment doesn't support MRT, it tries to use one of the 16-bit
        // texture format instead. Unfortunately, some GPUs don't support
        // 16-bit color render targets. So, in the worst case, it ends up with
        // using 32-bit raw textures.
        //
        class FrameBlendingFilter
        {
            #region Public methods

            public FrameBlendingFilter()
            {
                _useCompression = CheckSupportCompression();
                _rawTextureFormat = GetPreferredRenderTextureFormat();

                _material = new Material(Shader.Find("Hidden/Kino/Motion/FrameBlending"));
                _material.hideFlags = HideFlags.DontSave;

                _frameList = new Frame[4];
            }

            public void Release()
            {
                DestroyImmediate(_material);
                _material = null;

                foreach (var frame in _frameList) frame.Release();
                _frameList = null;
            }

            public void PushFrame(RenderTexture source)
            {
                // Push only when actual update (do nothing while pausing)
                var frameCount = Time.frameCount;
                if (frameCount == _lastFrameCount) return;

                // Update the frame record.
                var index = frameCount % _frameList.Length;
                if (_useCompression)
                    _frameList[index].MakeRecord(source, _material);
                else
                    _frameList[index].MakeRecordRaw(source, _rawTextureFormat);
                _lastFrameCount = frameCount;
            }

            public void BlendFrames(
                float strength, RenderTexture source, RenderTexture destination
            )
            {
                var t = Time.time;

                var f1 = GetFrameRelative(-1);
                var f2 = GetFrameRelative(-2);
                var f3 = GetFrameRelative(-3);
                var f4 = GetFrameRelative(-4);

                _material.SetTexture("_History1LumaTex", f1.lumaTexture);
                _material.SetTexture("_History2LumaTex", f2.lumaTexture);
                _material.SetTexture("_History3LumaTex", f3.lumaTexture);
                _material.SetTexture("_History4LumaTex", f4.lumaTexture);

                _material.SetTexture("_History1ChromaTex", f1.chromaTexture);
                _material.SetTexture("_History2ChromaTex", f2.chromaTexture);
                _material.SetTexture("_History3ChromaTex", f3.chromaTexture);
                _material.SetTexture("_History4ChromaTex", f4.chromaTexture);

                _material.SetFloat("_History1Weight", f1.CalculateWeight(strength, t));
                _material.SetFloat("_History2Weight", f2.CalculateWeight(strength, t));
                _material.SetFloat("_History3Weight", f3.CalculateWeight(strength, t));
                _material.SetFloat("_History4Weight", f4.CalculateWeight(strength, t));

                Graphics.Blit(source, destination, _material, _useCompression ? 1 : 2);
            }

            #endregion

            #region Frame record struct

            struct Frame
            {
                public RenderTexture lumaTexture;
                public RenderTexture chromaTexture;
                public float time;

                RenderBuffer[] _mrt;

                public float CalculateWeight(float strength, float currentTime)
                {
                    if (time == 0) return 0;
                    var coeff = Mathf.Lerp(80.0f, 16.0f, strength);
                    return Mathf.Exp((time - currentTime) * coeff);
                }

                public void Release()
                {
                    if (lumaTexture != null) RenderTexture.ReleaseTemporary(lumaTexture);
                    if (chromaTexture != null) RenderTexture.ReleaseTemporary(chromaTexture);

                    lumaTexture = null;
                    chromaTexture = null;
                }

                public void MakeRecord(RenderTexture source, Material material)
                {
                    Release();

                    lumaTexture = RenderTexture.GetTemporary(source.width, source.height, 0, RenderTextureFormat.R8, RenderTextureReadWrite.Linear);
                    chromaTexture = RenderTexture.GetTemporary(source.width, source.height, 0, RenderTextureFormat.R8, RenderTextureReadWrite.Linear);

                    lumaTexture.filterMode = FilterMode.Point;
                    chromaTexture.filterMode = FilterMode.Point;

                    if (_mrt == null) _mrt = new RenderBuffer[2];

                    _mrt[0] = lumaTexture.colorBuffer;
                    _mrt[1] = chromaTexture.colorBuffer;

                    Graphics.SetRenderTarget(_mrt, lumaTexture.depthBuffer);
                    Graphics.Blit(source, material, 0);

                    time = Time.time;
                }

                public void MakeRecordRaw(RenderTexture source, RenderTextureFormat format)
                {
                    Release();

                    lumaTexture = RenderTexture.GetTemporary(source.width, source.height, 0, format);
                    lumaTexture.filterMode = FilterMode.Point;

                    Graphics.Blit(source, lumaTexture);

                    time = Time.time;
                }
            }

            #endregion

            #region Private members

            bool _useCompression;
            RenderTextureFormat _rawTextureFormat;

            Material _material;

            Frame[] _frameList;
            int _lastFrameCount;

            // Check if the platform has the capability of compression.
            static bool CheckSupportCompression()
            {
                return
                    // Exclude OpenGL ES 2.0 because most of them don't support
                    // more than eight textures (we needs nine).
                    SystemInfo.graphicsDeviceType != GraphicsDeviceType.OpenGLES2 &&
                    SystemInfo.SupportsRenderTextureFormat(RenderTextureFormat.R8) &&
                    SystemInfo.supportedRenderTargetCount > 1;
            }

            // Determine which 16-bit render texture format is available.
            static RenderTextureFormat GetPreferredRenderTextureFormat()
            {
                RenderTextureFormat[] formats = {
                    RenderTextureFormat.RGB565,
                    RenderTextureFormat.ARGB1555,
                    RenderTextureFormat.ARGB4444
                };

                foreach (var f in formats)
                    if (SystemInfo.SupportsRenderTextureFormat(f)) return f;

                return RenderTextureFormat.Default;
            }

            // Retrieve a frame record with relative indexing.
            // Use a negative index to refer to previous frames.
            Frame GetFrameRelative(int offset)
            {
                var index = (Time.frameCount + _frameList.Length + offset) % _frameList.Length;
                return _frameList[index];
            }

            #endregion
        }
    }
}
