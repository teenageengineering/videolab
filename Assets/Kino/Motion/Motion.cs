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

namespace Kino
{
    [RequireComponent(typeof(Camera))]
    [AddComponentMenu("Kino Image Effects/Motion")]
    public partial class Motion : MonoBehaviour
    {
        #region Public properties

        /// The angle of rotary shutter. The larger the angle is, the longer
        /// the exposure time is.
        public float shutterAngle {
            get { return _shutterAngle; }
            set { _shutterAngle = value; }
        }

        [SerializeField, Range(0, 360)]
        [Tooltip("The angle of rotary shutter. Larger values give longer exposure.")]
        float _shutterAngle = 270;

        /// The amount of sample points, which affects quality and performance.
        public int sampleCount {
            get { return _sampleCount; }
            set { _sampleCount = value; }
        }

        [SerializeField]
        [Tooltip("The amount of sample points, which affects quality and performance.")]
        int _sampleCount = 8;

        /// The strength of multiple frame blending. The opacity of preceding
        /// frames are determined from this coefficient and time differences.
        public float frameBlending {
            get { return _frameBlending; }
            set { _frameBlending = value; }
        }

        [SerializeField, Range(0, 1)]
        [Tooltip("The strength of multiple frame blending")]
        float _frameBlending = 0;

        #endregion

        #region Private fields

        [SerializeField] Shader _reconstructionShader;
        [SerializeField] Shader _frameBlendingShader;

        ReconstructionFilter _reconstructionFilter;
        FrameBlendingFilter _frameBlendingFilter;

        #endregion

        #region MonoBehaviour functions

        void OnEnable()
        {
            _reconstructionFilter = new ReconstructionFilter();
            _frameBlendingFilter = new FrameBlendingFilter();
        }

        void OnDisable()
        {
            _reconstructionFilter.Release();
            _frameBlendingFilter.Release();

            _reconstructionFilter = null;
            _frameBlendingFilter = null;
        }

        void Update()
        {
            // Enable motion vector rendering if reuqired.
            if (_shutterAngle > 0)
                GetComponent<Camera>().depthTextureMode |=
                    DepthTextureMode.Depth | DepthTextureMode.MotionVectors;
        }

        void OnRenderImage(RenderTexture source, RenderTexture destination)
        {
            if (_shutterAngle > 0 && _frameBlending > 0)
            {
                // Reconstruction and frame blending
                var temp = RenderTexture.GetTemporary(
                    source.width, source.height, 0, source.format
                );

                _reconstructionFilter.ProcessImage(
                    _shutterAngle, _sampleCount, source, temp
                );

                _frameBlendingFilter.BlendFrames(
                    _frameBlending, temp, destination
                );
                _frameBlendingFilter.PushFrame(temp);

                RenderTexture.ReleaseTemporary(temp);
            }
            else if (_shutterAngle > 0)
            {
                // Reconstruction only
                _reconstructionFilter.ProcessImage(
                    _shutterAngle, _sampleCount, source, destination
                );
            }
            else if (_frameBlending > 0)
            {
                // Frame blending only
                _frameBlendingFilter.BlendFrames(
                    _frameBlending, source, destination
                );
                _frameBlendingFilter.PushFrame(source);
            }
            else
            {
                // Nothing to do!
                Graphics.Blit(source, destination);
            }
        }

        #endregion
    }
}
