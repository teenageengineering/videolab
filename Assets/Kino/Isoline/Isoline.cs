//
// KinoIsoline - Isoline effect
//
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
using UnityEngine;

namespace Kino
{
    [ExecuteInEditMode]
    [RequireComponent(typeof(Camera))]
    [AddComponentMenu("Kino Image Effects/Isoline")]
    public class Isoline : MonoBehaviour
    {
        #region Public Properties

        // Line color
        [SerializeField, ColorUsage(true, true, 0, 8, 0.125f, 3)]
        Color _lineColor = Color.white;

        public Color lineColor {
            get { return _lineColor; }
            set { _lineColor = value; }
        }

        // Luminance blending ratio
        [SerializeField, Range(0, 1)]
        float _luminanceBlending = 1;

        public float luminanceBlending {
            get { return _luminanceBlending; }
            set { _luminanceBlending = value; }
        }

        // Depth fall-off
        [SerializeField]
        float _fallOffDepth = 40;

        public float fallOffDepth {
            get { return _fallOffDepth; }
            set { _fallOffDepth = value; }
        }

        // Background color
        [SerializeField]
        Color _backgroundColor = Color.black;

        public Color backgroundColor {
            get { return _backgroundColor; }
            set { _backgroundColor = value; }
        }

        // Slicing axis
        [SerializeField]
        Vector3 _axis = Vector3.one * 0.577f;

        public Vector3 axis {
            get { return _axis; }
            set { _axis = value; }
        }

        // Contour interval
        [SerializeField]
        float _interval = 0.25f;

        public float interval {
            get { return _interval; }
            set { _interval = value; }
        }

        // Offset
        [SerializeField]
        Vector3 _offset;

        public Vector3 offset {
            get { return _offset; }
            set { _offset = value; }
        }

        // Distortion frequency
        [SerializeField]
        float _distortionFrequency = 1;

        public float distortionFrequency {
            get { return _distortionFrequency; }
            set { _distortionFrequency = value; }
        }

        // Distortion amount
        [SerializeField]
        float _distortionAmount = 0;

        public float distortionAmount {
            get { return _distortionAmount; }
            set { _distortionAmount = value; }
        }

        // Modulation mode
        public enum ModulationMode {
            None, Frac, Sin, Noise
        }

        [SerializeField]
        ModulationMode _modulationMode = ModulationMode.None;

        public ModulationMode modulationMode {
            get { return _modulationMode; }
            set { _modulationMode = value; }
        }

        // Modulation axis
        [SerializeField]
        Vector3 _modulationAxis = Vector3.forward;

        public Vector3 modulationAxis {
            get { return _modulationAxis; }
            set { _modulationAxis = value; }
        }

        // Modulation frequency
        [SerializeField]
        float _modulationFrequency = 0.2f;

        public float modulationFrequency {
            get { return _modulationFrequency; }
            set { _modulationFrequency = value; }
        }

        // Modulation speed
        [SerializeField]
        float _modulationSpeed = 1;

        public float modulationSpeed {
            get { return _modulationSpeed; }
            set { _modulationSpeed = value; }
        }

        // Modulation exponent
        [SerializeField, Range(1, 50)]
        float _modulationExponent = 24;

        public float modulationExponent {
            get { return _modulationExponent; }
            set { _modulationExponent = value; }
        }

        #endregion

        #region Private Properties

        [SerializeField]
        Shader _shader;

        Material _material;
        float _modulationTime;

        #endregion

        #region MonoBehaviour Functions

        void OnEnable()
        {
            GetComponent<Camera>().depthTextureMode |= DepthTextureMode.Depth;
        }

        void Update()
        {
            _modulationTime += Time.deltaTime * _modulationSpeed;
        }

        void OnRenderImage(RenderTexture source, RenderTexture destination)
        {
            if (_material == null) {
                _material = new Material(_shader);
                _material.hideFlags = HideFlags.DontSave;
            }

            var matrix = GetComponent<Camera>().cameraToWorldMatrix;
            _material.SetMatrix("_InverseView", matrix);

            _material.SetColor("_Color", _lineColor);
            _material.SetFloat("_FallOffDepth", _fallOffDepth);
            _material.SetFloat("_Blend", _luminanceBlending);
            _material.SetColor("_BgColor", _backgroundColor);

            _material.SetVector("_Axis", _axis.normalized);
            _material.SetFloat("_Density", 1.0f / _interval);
            _material.SetVector("_Offset", _offset);

            _material.SetFloat("_DistFreq", _distortionFrequency);
            _material.SetFloat("_DistAmp", _distortionAmount);

            if (_distortionAmount > 0)
                _material.EnableKeyword("DISTORTION");
            else
                _material.DisableKeyword("DISTORTION");

            _material.DisableKeyword("MODULATION_FRAC");
            _material.DisableKeyword("MODULATION_SIN");
            _material.DisableKeyword("MODULATION_NOISE");

            if (_modulationMode == ModulationMode.Frac)
                _material.EnableKeyword("MODULATION_FRAC");
            else if (_modulationMode == ModulationMode.Sin)
                _material.EnableKeyword("MODULATION_SIN");
            else if (_modulationMode == ModulationMode.Noise)
                _material.EnableKeyword("MODULATION_NOISE");

            var modFreq = _modulationFrequency;
            if (_modulationMode == ModulationMode.Sin)
                modFreq *= Mathf.PI * 2;

            _material.SetVector("_ModAxis", _modulationAxis.normalized);
            _material.SetFloat("_ModFreq", modFreq);
            _material.SetFloat("_ModTime", _modulationTime);
            _material.SetFloat("_ModExp", _modulationExponent);

            Graphics.Blit(source, destination, _material);
        }

        #endregion
    }
}
