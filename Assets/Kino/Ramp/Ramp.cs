//
// KinoRamp - Color ramp overlay
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
    [AddComponentMenu("Kino Image Effects/Ramp")]
    public class Ramp : MonoBehaviour
    {
        #region Public Properties

        // first color

        [SerializeField]
        Color _color1 = Color.blue;

        public Color color1 {
            get { return _color1; }
            set { _color1 = value; }
        }

        // second color

        [SerializeField]
        Color _color2 = Color.red;

        public Color color2 {
            get { return _color2; }
            set { _color2 = value; }
        }

        // ramp angle

        [SerializeField, Range(-180, 180)]
        float _angle = 90;

        public float angle {
            get { return _angle; }
            set { _angle = value; }
        }

        // blend opacity

        [SerializeField, Range(0, 1)]
        float _opacity = 1;

        public float opacity {
            get { return _opacity; }
            set { _opacity = value; }
        }

        // blend mode

        public enum BlendMode {
            Multiply, Screen, Overlay, HardLight, SoftLight
        }

        [SerializeField]
        BlendMode _blendMode = BlendMode.Overlay;

        public float blendMode {
            get { return (float)_blendMode; }
            set { _blendMode = (BlendMode)value; }
        }

        // debug (show ramp)

        [SerializeField]
        bool _debug;

        #endregion

        #region Private Properties

        [SerializeField] Shader _shader;
        Material _material;

        static string[] _blendModeKeywords = {
            "_MULTIPLY", "_SCREEN", "_OVERLAY", "_HARDLIGHT", "_SOFTLIGHT"
        };

        #endregion

        #region MonoBehaviour Functions

        void OnRenderImage(RenderTexture source, RenderTexture destination)
        {
            if (_material == null)
            {
                _material = new Material(_shader);
                _material.hideFlags = HideFlags.DontSave;
            }

            // color parameters
            Color c0;
            if (_blendMode == BlendMode.Multiply)
                c0 = Color.white;
            else if (_blendMode == BlendMode.Screen)
                c0 = Color.black;
            else
                c0 = Color.gray;

            var blend = _debug ? 1.0f : _opacity;
            _material.SetColor("_Color1", Color.Lerp(c0, _color1, blend));
            _material.SetColor("_Color2", Color.Lerp(c0, _color2, blend));

            // ramp direction vector
            var phi = Mathf.Deg2Rad * _angle;
            var dir = new Vector2(Mathf.Cos(phi), Mathf.Sin(phi));
            _material.SetVector("_Direction", dir);

            // setting shader keywords
            _material.shaderKeywords = null;
            _material.EnableKeyword(_blendModeKeywords[(int)_blendMode]);

            if (QualitySettings.activeColorSpace == ColorSpace.Linear)
                _material.EnableKeyword("_LINEAR");
            else
                _material.DisableKeyword("_LINEAR");

            if (_debug)
                _material.EnableKeyword("_DEBUG");
            else
                _material.DisableKeyword("_DEBUG");

            Graphics.Blit(source, destination, _material, 0);
        }

        #endregion
    }
}
