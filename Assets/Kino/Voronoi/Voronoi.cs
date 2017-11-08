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
using UnityEngine;
using System.Collections;

namespace Kino
{
    [ExecuteInEditMode]
    public class Voronoi : MonoBehaviour
    {
        #region Editable properties

        /// Line color
        public Color lineColor {
            get { return _lineColor; }
            set { _lineColor = value; }
        }

        [SerializeField]
        Color _lineColor = Color.white;

        /// Cell color
        public Color cellColor {
            get { return _cellColor; }
            set { _cellColor = value; }
        }

        [SerializeField, ColorUsage(false)]
        Color _cellColor = Color.white;

        /// Background color
        public Color backgroundColor {
            get { return _backgroundColor; }
            set { _backgroundColor = value; }
        }

        [SerializeField, ColorUsage(false)]
        Color _backgroundColor = Color.black;

        /// Minimum value of the input range
        public float rangeMin {
            get { return _rangeMin; }
            set { _rangeMin = value; }
        }

        [SerializeField, Range(0, 1)]
        float _rangeMin = 0;

        /// Maximum value of the input range
        public float rangeMax {
            get { return _rangeMax; }
            set { _rangeMax = value; }
        }

        [SerializeField, Range(0, 1)]
        float _rangeMax = 1;

        /// Coefficient for the exponential gradient curve
        public float cellExponent {
            get { return _cellExponent; }
            set { _cellExponent = value; }
        }

        [SerializeField, Range(1, 10)]
        float _cellExponent = 1;

        /// Determines how many time it repeats the process
        public int iteration {
            get { return _iteration; }
            set { _iteration = value; }
        }

        [SerializeField]
        int _iteration = 4;

        /// Opacity level (blend ratio)
        public float opacity {
            get { return _opacity; }
            set { _opacity = value; }
        }

        [SerializeField, Range(0, 1)]
        float _opacity = 1;

        #endregion
        
        #region Private properties

        // internal references to related assets
        [SerializeField] VoronoiMesh _mesh;
        [SerializeField] Shader _coneShader;
        [SerializeField] Shader _contourShader;

        // cone shader material
        Material coneMaterial {
            get {
                if (_coneMaterial == null) {
                    var shader = Shader.Find("Hidden/Kino/Voronoi/Cone");
                    _coneMaterial = new Material(shader);
                    _coneMaterial.hideFlags = HideFlags.DontSave;
                }
                return _coneMaterial;
            }
        }

        Material _coneMaterial;

        // contour shader material
        Material contourMaterial {
            get {
                if (_contourMaterial == null) {
                    var shader = Shader.Find("Hidden/Kino/Voronoi/Contour");
                    _contourMaterial = new Material(shader);
                    _contourMaterial.hideFlags = HideFlags.DontSave;
                }
                return _contourMaterial;
            }
        }

        Material _contourMaterial;

        #endregion

        #region MonoBehaviour Functions

        void OnRenderImage(RenderTexture source, RenderTexture destination)
        {
            bool isLinear =
                QualitySettings.activeColorSpace == ColorSpace.Linear;

            // temporary color buffer
            var rtColor = RenderTexture.GetTemporary(
                source.width, source.height,
                24, RenderTextureFormat.Default
            );

            // temporary normal buffer
            var rtNormal = RenderTexture.GetTemporary(
                source.width, source.height,
                0, RenderTextureFormat.DefaultHDR
            );

            // bind them as a multi-render target
            var mrt = new RenderBuffer[2] {
                rtColor.colorBuffer, rtNormal.colorBuffer
            };
            Graphics.SetRenderTarget(mrt, rtColor.depthBuffer);

            // clear
            GL.Clear(true, true, Color.black);

            // set up the cone shader
            coneMaterial.SetTexture("_Source", source);
            var aspect = (float)source.width / source.height;
            coneMaterial.SetFloat("_Aspect", aspect);
            coneMaterial.SetFloat("_RangeMin", _rangeMin);
            coneMaterial.SetFloat("_RangeMax", _rangeMax);

            // draw cones repeatedly
            for (var i = 0; i < _iteration; i++)
            {
                coneMaterial.SetPass(0);
                coneMaterial.SetFloat("_RandomSeed", i * 10);
                Graphics.DrawMeshNow(_mesh.sharedMesh, Matrix4x4.identity);
            }

            // set up the contour shader
            contourMaterial.SetTexture("_ColorTexture", rtColor);
            contourMaterial.SetTexture("_NormalTexture", rtNormal);
            contourMaterial.SetColor("_LineColor", _lineColor);

            if (isLinear)
            {
                contourMaterial.SetColor("_CellColorGamma", _cellColor.gamma);
                contourMaterial.SetColor("_BgColorGamma", _backgroundColor.gamma);
            }
            else
            {
                contourMaterial.SetColor("_CellColorGamma", _cellColor);
                contourMaterial.SetColor("_BgColorGamma", _backgroundColor);
            }

            contourMaterial.SetFloat("_CellExponent", _cellExponent);
            contourMaterial.SetFloat("_Blend", _opacity);

            // contour filter
            Graphics.Blit(source, destination, contourMaterial, 0);

            // dispose temporary buffers
            RenderTexture.ReleaseTemporary(rtColor);
            RenderTexture.ReleaseTemporary(rtNormal);
        }

        #endregion
    }
}
