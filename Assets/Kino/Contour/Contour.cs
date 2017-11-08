//
// KinoContour - Contour line effect
//
// Copyright (C) 2015 Keijiro Takahashi
//
// Permission is hereby granted, free of charge, to any person obtaining a copy of
// this software and associated documentation files (the "Software"), to deal in
// the Software without restriction, including without limitation the rights to
// use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of
// the Software, and to permit persons to whom the Software is furnished to do so,
// subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS
// FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR
// COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER
// IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN
// CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//
using UnityEngine;

namespace Kino
{
    [ExecuteInEditMode]
    [RequireComponent(typeof(Camera))]
    [AddComponentMenu("Kino Image Effects/Contour")]
    public class Contour : MonoBehaviour
    {
        #region Public Properties

        // Line color
        [SerializeField]
        Color _lineColor = Color.black;

        public Color lineColor {
            get { return _lineColor; }
            set { _lineColor = value; }
        }

        // Background color
        [SerializeField]
        Color _backgroundColor = new Color(1, 1, 1, 0);

        public Color backgroundColor {
            get { return _backgroundColor; }
            set { _backgroundColor = value; }
        }

        // Low threshold
        [SerializeField, Range(0, 1)]
        float _lowThreshold = 0.05f;

        public float lowThreshold {
            get { return _lowThreshold; }
            set { _lowThreshold = value; }
        }

        // High threshold
        [SerializeField, Range(0, 1)]
        float _highThreshold = 0.5f;

        public float highThreshold {
            get { return _highThreshold; }
            set { _highThreshold = value; }
        }

        // Depth sensitivity
        [SerializeField, Range(0, 2)]
        float _depthSensitivity = 1;

        public float depthSensitivity {
            get { return _depthSensitivity; }
            set { _depthSensitivity = value; }
        }

        // Normal sensitivity
        [SerializeField, Range(0, 1)]
        float _normalSensitivity = 0;

        public float normalSensitivity {
            get { return _normalSensitivity; }
            set { _normalSensitivity = value; }
        }

        // Depth fall-off
        [SerializeField]
        float _fallOffDepth = 40;

        public float fallOffDepth {
            get { return _fallOffDepth; }
            set { _fallOffDepth = value; }
        }

        #endregion

        #region Private Properties

        [SerializeField] Shader _shader;

        Material _material;

        #endregion

        #region MonoBehaviour Functions

        void OnEnable()
        {
            GetComponent<Camera>().depthTextureMode |= DepthTextureMode.Depth;
        }

        void OnRenderImage(RenderTexture source, RenderTexture destination)
        {
            if (_material == null)
            {
                _material = new Material(_shader);
                _material.hideFlags = HideFlags.DontSave;
            }

            _material.SetColor("_Color", _lineColor);
            _material.SetColor("_BgColor", _backgroundColor);

            var hi = Mathf.Max(_lowThreshold, _highThreshold);
            _material.SetFloat("_LowThreshold", _lowThreshold);
            _material.SetFloat("_HighThreshold", hi);

            _material.SetFloat("_DepthSensitivity", _depthSensitivity);
            _material.SetFloat("_NormalSensitivity", _normalSensitivity);

            _material.SetFloat("_FallOffDepth", _fallOffDepth);

            if (_depthSensitivity > 0)
                _material.EnableKeyword("USE_DEPTH");
            else
                _material.DisableKeyword("USE_DEPTH");

            if (_normalSensitivity > 0)
                _material.EnableKeyword("USE_NORMAL");
            else
                _material.DisableKeyword("USE_NORMAL");

            Graphics.Blit(source, destination, _material);
        }

        #endregion
    }
}
