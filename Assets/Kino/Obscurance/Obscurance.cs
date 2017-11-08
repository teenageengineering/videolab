//
// Kino/Obscurance - Screen space ambient obscurance image effect
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
    [ExecuteInEditMode]
    [RequireComponent(typeof(Camera))]
    [AddComponentMenu("Kino Image Effects/Obscurance")]
    public partial class Obscurance : MonoBehaviour
    {
        #region Public Properties

        /// Degree of darkness produced by the effect.
        public float intensity {
            get { return _intensity; }
            set { _intensity = value; }
        }

        [SerializeField, Range(0, 4), Tooltip(
            "Degree of darkness produced by the effect.")]
        float _intensity = 1;

        /// Radius of sample points, which affects extent of darkened areas.
        public float radius {
            get { return Mathf.Max(_radius, 1e-4f); }
            set { _radius = value; }
        }

        [SerializeField, Tooltip(
            "Radius of sample points, which affects extent of darkened areas.")]
        float _radius = 0.3f;

        /// Number of sample points, which affects quality and performance.
        public SampleCount sampleCount {
            get { return _sampleCount; }
            set { _sampleCount = value; }
        }

        public enum SampleCount { Lowest, Low, Medium, High, Custom }

        [SerializeField, Tooltip(
            "Number of sample points, which affects quality and performance.")]
        SampleCount _sampleCount = SampleCount.Medium;

        /// Determines the sample count when SampleCount.Custom is used.
        /// In other cases, it returns the preset value of the current setting.
        public int sampleCountValue {
            get {
                switch (_sampleCount) {
                    case SampleCount.Lowest: return 3;
                    case SampleCount.Low:    return 6;
                    case SampleCount.Medium: return 12;
                    case SampleCount.High:   return 20;
                }
                return Mathf.Clamp(_sampleCountValue, 1, 256);
            }
            set { _sampleCountValue = value; }
        }

        [SerializeField]
        int _sampleCountValue = 24;

        /// Halves the resolution of the effect to increase performance.
        public bool downsampling {
            get { return _downsampling; }
            set { _downsampling = value; }
        }

        [SerializeField, Tooltip(
            "Halves the resolution of the effect to increase performance.")]
        bool _downsampling = false;

        /// Source buffer used for obscurance estimation.
        public OcclusionSource occlusionSource {
            get {
                var isGBuffer = _occlusionSource == OcclusionSource.GBuffer;
                if (isGBuffer && !IsGBufferAvailable)
                    // An unavailable source was chosen:
                    // fallback to DepthNormalsTexture.
                    return OcclusionSource.DepthNormalsTexture;
                else
                    return _occlusionSource;
            }
            set { _occlusionSource = value; }
        }

        public enum OcclusionSource {
            DepthTexture, DepthNormalsTexture, GBuffer
        }

        [SerializeField, Tooltip(
            "Source buffer used for obscurance estimation")]
        OcclusionSource _occlusionSource = OcclusionSource.GBuffer;

        /// Enables the ambient-only mode in that the effect only affects
        /// ambient lighting. This mode is only available with G-buffer source
        /// and HDR rendering.
        public bool ambientOnly {
            get {
            #if UNITY_5_6_OR_NEWER
                return _ambientOnly && targetCamera.allowHDR &&
                    occlusionSource == OcclusionSource.GBuffer;
            #else
                return _ambientOnly && targetCamera.hdr &&
                    occlusionSource == OcclusionSource.GBuffer;
            #endif
            }
            set { _ambientOnly = value; }
        }

        [SerializeField, Tooltip(
            "If checked, the effect only affects ambient lighting.")]
        bool _ambientOnly = false;

        #endregion

        #region Private Properties

        // AO shader material
        Material aoMaterial {
            get {
                if (_aoMaterial == null) {
                    var shader = Shader.Find("Hidden/Kino/Obscurance");
                    _aoMaterial = new Material(shader);
                    _aoMaterial.hideFlags = HideFlags.DontSave;
                }
                return _aoMaterial;
            }
        }

        [SerializeField] Shader _aoShader;
        Material _aoMaterial;

        // Command buffer for the AO pass
        CommandBuffer aoCommands {
            get {
                if (_aoCommands == null) {
                    _aoCommands = new CommandBuffer();
                    _aoCommands.name = "Kino.Obscurance";
                }
                return _aoCommands;
            }
        }

        CommandBuffer _aoCommands;

        // Target camera
        Camera targetCamera {
            get { return GetComponent<Camera>(); }
        }

        // Property observer
        PropertyObserver propertyObserver {
            get { return _propertyObserver; }
        }

        PropertyObserver _propertyObserver = new PropertyObserver();

        // Check if the G-buffer is available
        bool IsGBufferAvailable {
            get {
                var path = targetCamera.actualRenderingPath;
                return path == RenderingPath.DeferredShading;
            }
        }

        // Reference to the quad mesh in the built-in assets
        // (used in MRT blitting)
        [SerializeField] Mesh _quadMesh;

        #endregion

        #region Effect Passes

        // Build commands for the AO pass (used in the ambient-only mode).
        void BuildAOCommands()
        {
            var cb = aoCommands;

            var tw = targetCamera.pixelWidth;
            var th = targetCamera.pixelHeight;
            var ts = downsampling ? 2 : 1;
            var format = RenderTextureFormat.ARGB32;
            var rwMode = RenderTextureReadWrite.Linear;
            var filter = FilterMode.Bilinear;

            // AO buffer
            var m = aoMaterial;
            var rtMask = Shader.PropertyToID("_OcclusionTexture1");
            cb.GetTemporaryRT(
                rtMask, tw / ts, th / ts, 0, filter, format, rwMode
            );

            // AO estimation
            cb.Blit(null, rtMask, m, 2);

            // Blur buffer
            var rtBlur = Shader.PropertyToID("_OcclusionTexture2");

            // Separable blur (horizontal pass)
            cb.GetTemporaryRT(rtBlur, tw, th, 0, filter, format, rwMode);
            cb.Blit(rtMask, rtBlur, m, 4);
            cb.ReleaseTemporaryRT(rtMask);

            // Separable blur (vertical pass)
            rtMask = Shader.PropertyToID("_OcclusionTexture");
            cb.GetTemporaryRT(rtMask, tw, th, 0, filter, format, rwMode);
            cb.Blit(rtBlur, rtMask, m, 5);
            cb.ReleaseTemporaryRT(rtBlur);

            // Combine AO to the G-buffer.
            var mrt = new RenderTargetIdentifier[] {
                BuiltinRenderTextureType.GBuffer0,      // Albedo, Occ
                BuiltinRenderTextureType.CameraTarget   // Ambient
            };
            cb.SetRenderTarget(mrt, BuiltinRenderTextureType.CameraTarget);
            cb.DrawMesh(_quadMesh, Matrix4x4.identity, m, 0, 7);

            cb.ReleaseTemporaryRT(rtMask);
        }

        // Execute the AO pass immediately (used in the forward mode).
        void ExecuteAOPass(RenderTexture source, RenderTexture destination)
        {
            var tw = source.width;
            var th = source.height;
            var ts = downsampling ? 2 : 1;
            var format = RenderTextureFormat.ARGB32;
            var rwMode = RenderTextureReadWrite.Linear;
            var useGBuffer = occlusionSource == OcclusionSource.GBuffer;

            // AO buffer
            var m = aoMaterial;
            var rtMask = RenderTexture.GetTemporary(
                tw / ts, th / ts, 0, format, rwMode
            );

            // AO estimation
            Graphics.Blit(source, rtMask, m, (int)occlusionSource);

            // Separable blur (horizontal pass)
            var rtBlur = RenderTexture.GetTemporary(tw, th, 0, format, rwMode);
            Graphics.Blit(rtMask, rtBlur, m, useGBuffer ? 4 : 3);
            RenderTexture.ReleaseTemporary(rtMask);

            // Separable blur (vertical pass)
            rtMask = RenderTexture.GetTemporary(tw, th, 0, format, rwMode);
            Graphics.Blit(rtBlur, rtMask, m, 5);
            RenderTexture.ReleaseTemporary(rtBlur);

            // Composition
            m.SetTexture("_OcclusionTexture", rtMask);
            Graphics.Blit(source, destination, m, 6);
            RenderTexture.ReleaseTemporary(rtMask);

            // Explicitly detach the temporary texture.
            // (This is needed to avoid conflict with CommandBuffer)
            m.SetTexture("_OcclusionTexture", null);
        }

        // Update the common material properties.
        void UpdateMaterialProperties()
        {
            var m = aoMaterial;
            m.SetFloat("_Intensity", intensity);
            m.SetFloat("_Radius", radius);
            m.SetFloat("_Downsample", downsampling ? 0.5f : 1);
            m.SetInt("_SampleCount", sampleCountValue);
        }

        #endregion

        #region MonoBehaviour Functions

        void OnEnable()
        {
            // Register the command buffer if in the ambient-only mode.
            if (ambientOnly) targetCamera.AddCommandBuffer(
                CameraEvent.BeforeReflections, aoCommands
            );

            // Enable depth textures which the occlusion source requires.
            if (occlusionSource == OcclusionSource.DepthTexture)
                targetCamera.depthTextureMode |= DepthTextureMode.Depth;

            if (occlusionSource != OcclusionSource.GBuffer)
                targetCamera.depthTextureMode |= DepthTextureMode.DepthNormals;
        }

        void OnDisable()
        {
            // Remove the command buffer from the camera.
            if (_aoCommands != null) targetCamera.RemoveCommandBuffer(
                CameraEvent.BeforeReflections, _aoCommands
            );
        }

        void OnDestroy()
        {
            if (Application.isPlaying)
                Destroy(_aoMaterial);
            else
                DestroyImmediate(_aoMaterial);
        }

        void OnPreRender()
        {
            if (propertyObserver.CheckNeedsReset(this, targetCamera))
            {
                // Reinitialize all the resources by disabling/enabling itself.
                // This is not very efficient way but just works...
                OnDisable();
                OnEnable();

                // Build the command buffer if in the ambient-only mode.
                if (ambientOnly)
                {
                    aoCommands.Clear();
                    BuildAOCommands();
                }

                propertyObserver.Update(this, targetCamera);
            }

            // Update the material properties (later used in the AO commands).
            if (ambientOnly) UpdateMaterialProperties();
        }

        [ImageEffectOpaque]
        void OnRenderImage(RenderTexture source, RenderTexture destination)
        {
            if (ambientOnly)
            {
                // Do nothing in the ambient-only mode.
                Graphics.Blit(source, destination);
            }
            else
            {
                // Execute the AO pass.
                UpdateMaterialProperties();
                ExecuteAOPass(source, destination);
            }
        }

        #endregion
    }
}
