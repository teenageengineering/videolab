//
// Kino/Vision - Frame visualization utility
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
    [AddComponentMenu("Kino Image Effects/Vision")]
    public partial class Vision : MonoBehaviour
    {
        #region Common property

        public enum Source {
            Depth, Normals, MotionVectors
        }

        [SerializeField]
        Source _source;

        [SerializeField, Range(0, 1)]
        float _blendRatio = 0.5f;

        [SerializeField]
        bool _preferDepthNormals;

        #endregion

        #region Properties for depth

        [SerializeField]
        float _depthRepeat = 1;

        #endregion

        #region Properties for normals

        [SerializeField]
        bool _validateNormals = false;

        #endregion

        #region Properties for motion vectors

        [SerializeField]
        float _motionOverlayAmplitude = 10;

        [SerializeField]
        float _motionVectorsAmplitude = 50;

        [SerializeField, Range(8, 64)]
        int _motionVectorsResolution = 16;

        #endregion

        #region Private properties and methods

        [SerializeField] Shader _shader;
        Material _material;
        ArrowArray _arrows;

        // Target camera
        Camera TargetCamera {
            get { return GetComponent<Camera>(); }
        }

        // Check if the G-buffer is available.
        bool IsGBufferAvailable {
            get { return TargetCamera.actualRenderingPath == RenderingPath.DeferredShading; }
        }

        // Rebuild arrows if needed.
        void PrepareArrows()
        {
            var row = _motionVectorsResolution;
            var col = row * Screen.width / Screen.height;

            if (_arrows.columnCount != col || _arrows.rowCount != row)
            {
                _arrows.DestroyMesh();
                _arrows.BuildMesh(col, row);
            }
        }

        // Draw arrows in an immediate-mode fashion.
        void DrawArrows(float aspect)
        {
            PrepareArrows();

            var sy = 1.0f / _motionVectorsResolution;
            var sx = sy / aspect;
            _material.SetVector("_Scale", new Vector2(sx, sy));

            _material.SetFloat("_Blend", _blendRatio);
            _material.SetFloat("_Amplitude", _motionVectorsAmplitude);

            _material.SetPass(5);
            Graphics.DrawMeshNow(_arrows.mesh, Matrix4x4.identity);
        }

        #endregion

        #region MonoBehaviour functions

        void OnEnable()
        {
            // Initialize the pairs of shaders/materials.
            _material = new Material(Shader.Find("Hidden/Kino/Vision"));
            _material.hideFlags = HideFlags.DontSave;

            // Build the array of arrows.
            _arrows = new ArrowArray();
            PrepareArrows();
        }

        void OnDisable()
        {
            DestroyImmediate(_material);
            _material = null;

            _arrows.DestroyMesh();
            _arrows = null;
        }

        void Update()
        {
            // Update depth texture mode.
            if (_source == Source.Depth)
                if (_preferDepthNormals)
                    TargetCamera.depthTextureMode |= DepthTextureMode.DepthNormals;
                else
                    TargetCamera.depthTextureMode |= DepthTextureMode.Depth;

            if (_source == Source.Normals)
                if (_preferDepthNormals || !IsGBufferAvailable)
                    TargetCamera.depthTextureMode |= DepthTextureMode.DepthNormals;

            if (_source == Source.MotionVectors)
                TargetCamera.depthTextureMode |= DepthTextureMode.Depth | DepthTextureMode.MotionVectors;
        }

        void OnRenderImage(RenderTexture source, RenderTexture destination)
        {
            if (_source == Source.Depth)
            {
                // Depth
                _material.SetFloat("_Blend", _blendRatio);
                _material.SetFloat("_Repeat", _depthRepeat);
                var pass = _preferDepthNormals ? 1 : 0;
                Graphics.Blit(source, destination, _material, pass);
            }
            else if (_source == Source.Normals)
            {
                // Normals
                _material.SetFloat("_Blend", _blendRatio);
                _material.SetFloat("_Validate", _validateNormals ? 1 : 0);
                var pass = (!_preferDepthNormals && IsGBufferAvailable) ? 3 : 2;
                Graphics.Blit(source, destination, _material, pass);
            }
            else if (_source == Source.MotionVectors)
            {
                // Motion vectors
                _material.SetFloat("_Blend", _blendRatio);
                _material.SetFloat("_Amplitude", _motionOverlayAmplitude);
                Graphics.Blit(source, destination, _material, 4);
                DrawArrows((float)source.width / source.height);
            }
        }

        #endregion
    }
}
