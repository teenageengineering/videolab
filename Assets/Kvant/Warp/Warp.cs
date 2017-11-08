//
// Kvant/Warp - Warp (hyperspace) light streaks effect
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

namespace Kvant
{
    [ExecuteInEditMode]
    [RequireComponent(typeof(MeshRenderer))]
    [AddComponentMenu("Kvant/Warp")]
    public class Warp : MonoBehaviour
    {
        #region Basic settings

        [SerializeField] WarpTemplate _template;

        [SerializeField, Range(0, 1)] float _throttle = 1.0f;

        public float throttle {
            get { return _throttle; }
            set { _throttle = value; }
        }

        [SerializeField] Vector3 _extent = Vector3.one * 100;

        public Vector3 extent {
            get { return _extent; }
            set { _extent = value; }
        }

        [SerializeField] float _lineRadius = 0.1f;

        public float lineRadius {
            get { return _lineRadius; }
            set { _lineRadius = value; }
        }

        [SerializeField] float _lineWidth = 10;

        public float lineWidth {
            get { return _lineWidth; }
            set { _lineWidth = value; }
        }

        [SerializeField, Range(0, 1)] float _lineWidthRandomness = 0.5f;

        public float lineWidthRandomness {
            get { return _lineWidthRandomness; }
            set { _lineWidthRandomness = value; }
        }

        [SerializeField] float _speed = 100;

        public float speed {
            get { return _speed; }
            set { _speed = value; }
        }

        [SerializeField, Range(0, 1)] float _speedRandomness = 0.5f;

        public float speedRandomness {
            get { return _speedRandomness; }
            set { _speedRandomness = value; }
        }

        [SerializeField] int _randomSeed = 0;

        #endregion

        #region Private members

        // References to the built-in assets
        [SerializeField] Material _defaultMaterial;

        // Variables for simulation
        float _time;
        float _deltaTime;

        // Custom properties applied to the mesh renderer.
        MaterialPropertyBlock _propertyBlock;

        // Update external components: mesh filter.
        void UpdateMeshFilter()
        {
            var meshFilter = GetComponent<MeshFilter>();

            // Add a new mesh filter if missing.
            if (meshFilter == null)
            {
                meshFilter = gameObject.AddComponent<MeshFilter>();
                meshFilter.hideFlags = HideFlags.NotEditable;
            }

            if (meshFilter.sharedMesh != _template.mesh)
                meshFilter.sharedMesh = _template.mesh;
        }

        // Update external components: mesh renderer.
        void UpdateMeshRenderer()
        {
            var meshRenderer = GetComponent<MeshRenderer>();

            if (_propertyBlock == null)
                _propertyBlock = new MaterialPropertyBlock();

            var pb = _propertyBlock;

            pb.SetFloat("_RandomSeed", _randomSeed);
            pb.SetFloat("_NormalizedTime", _time);
            pb.SetFloat("_NormalizedDeltaTime", _deltaTime);
            pb.SetFloat("_Throttle", _throttle);
            pb.SetVector("_Extent", _extent);
            pb.SetFloat("_LineRadius", _lineRadius);
            var lw = new Vector2(1 - _lineWidthRandomness, 1);
            pb.SetVector("_LineWidth", lw * _lineWidth);
            pb.SetFloat("_SpeedRandomness", _speedRandomness);

            meshRenderer.SetPropertyBlock(pb);

            // Set the default material if no material is set.
            if (meshRenderer.sharedMaterial == null)
                meshRenderer.sharedMaterial = _defaultMaterial;
        }

        #endregion

        #region MonoBehaviour functions

        void LateUpdate()
        {
            // Do nothing if no template is set.
            if (_template == null) return;

            // Advance time.
            var speed = _speed / _extent.z;
            if (Application.isPlaying)
            {
                _deltaTime = Time.deltaTime * speed;
                _time += _deltaTime;
            }
            else
            {
                _deltaTime = speed / 30;
                _time = 10 * speed;
            }

            // Update external components (mesh filter and renderer).
            UpdateMeshFilter();
            UpdateMeshRenderer();
        }

        void OnDrawGizmosSelected()
        {
            // Show the attractor position.
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireCube(Vector3.zero, _extent);
        }

        #endregion
    }
}
