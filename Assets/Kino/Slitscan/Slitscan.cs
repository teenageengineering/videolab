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
    [ExecuteInEditMode]
    [RequireComponent(typeof(Camera))]
    public partial class Slitscan : MonoBehaviour
    {
        #region Editable properties

        const int kMaxSliceCount = 128;

        [SerializeField, Range(16, kMaxSliceCount)]
        int _sliceCount = 128;

        public int sliceCount {
            get { return _sliceCount; }
            set { _sliceCount = value; }
        }

        #endregion

        #region Private members

        [SerializeField] Mesh _mesh;
        [SerializeField] Shader _shader;

        Material _material;

        Frame[] _history;
        int _lastFrame = -1;

        RenderBuffer[] _mrt;

        #endregion

        #region Private functions

        void AppendFrame(RenderTexture source)
        {
            // Advance the counter.
            _lastFrame = (_lastFrame + 1) % kMaxSliceCount;

            // Prepare the frame storage.
            var frame = _history[_lastFrame];
            frame.Prepare(source.width, source.height);

            // Extract luminance.
            Graphics.Blit(source, frame.yTexture, _material, 0);

            // Extract chrominance.
            _mrt[0] = frame.cgTexture.colorBuffer;
            _mrt[1] = frame.coTexture.colorBuffer;
            Graphics.SetRenderTarget(_mrt, frame.cgTexture.depthBuffer);
            Graphics.Blit(source, _material, 1);
        }

        Frame GetFrameRelative(int offset)
        {
            var i = (_lastFrame + offset + kMaxSliceCount) % kMaxSliceCount;

            if (_history[i].yTexture != null)
                return _history[i];
            else
                return _history[_lastFrame];
        }

        #endregion

        #region MonoBehaviour functions

        void OnEnable()
        {
            if (_material == null)
            {
                _material = new Material(Shader.Find("Hidden/Kino/Slitscan"));
                _material.hideFlags = HideFlags.HideAndDontSave;
            }

            if (_history == null)
            {
                _history = new Frame[kMaxSliceCount];
                for (var i = 0; i < kMaxSliceCount; i++)
                    _history[i] = new Frame();
            }

            if (_mrt == null)
                _mrt = new RenderBuffer[2];
        }

        void OnDisable()
        {
            foreach (var frame in _history)
                frame.Release();
        }

        void OnDestroy()
        {
            if (Application.isPlaying)
                Destroy(_material);
            else
                DestroyImmediate(_material);

            _material = null;
        }

        void OnRenderImage(RenderTexture source, RenderTexture destination)
        {
            // Append this frame to the history buffer.
            AppendFrame(source);

            // Simply blit the source if there is nothing to do.
            var slices = (_sliceCount / 4) * 4;
            if (slices <= 0)
            {
                Graphics.Blit(source, destination);
                return;
            }

            // Draw slices on the destination.
            RenderTexture.active = destination;

            _material.SetFloat("_SliceWidth", 1.0f / slices);

            for (var i = 0; i < slices; i += 4)
            {
                var frame0 = GetFrameRelative(i + 1);
                var frame1 = GetFrameRelative(i + 2);
                var frame2 = GetFrameRelative(i + 3);
                var frame3 = GetFrameRelative(i + 4);

                _material.SetTexture("_MainTex", source);

                _material.SetTexture("_YTexture0", frame0.yTexture);
                _material.SetTexture("_YTexture1", frame1.yTexture);
                _material.SetTexture("_YTexture2", frame2.yTexture);
                _material.SetTexture("_YTexture3", frame3.yTexture);

                _material.SetTexture("_CgTexture0", frame0.cgTexture);
                _material.SetTexture("_CgTexture1", frame1.cgTexture);
                _material.SetTexture("_CgTexture2", frame2.cgTexture);
                _material.SetTexture("_CgTexture3", frame3.cgTexture);

                _material.SetTexture("_CoTexture0", frame0.coTexture);
                _material.SetTexture("_CoTexture1", frame1.coTexture);
                _material.SetTexture("_CoTexture2", frame2.coTexture);
                _material.SetTexture("_CoTexture3", frame3.coTexture);

                _material.SetFloat("_SliceNumber", i);
                _material.SetPass(2);

                Graphics.DrawMeshNow(_mesh, Matrix4x4.identity);
            }
        }

        #endregion
    }
}
