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
using System.Collections.Generic;
using System.Linq;

namespace Kvant
{
    public class WarpTemplate : ScriptableObject
    {
        #region Public properties

        /// Instance count (read only)
        public int instanceCount {
            get { return _instanceCount; }
        }

        [SerializeField] int _instanceCount;

        /// Tmplate mesh (read only)
        public Mesh mesh {
            get { return _mesh; }
        }

        [SerializeField] Mesh _mesh;

        #endregion

        #region Private members

        [SerializeField] Mesh _sourceShape;

        #endregion

        #region Public methods

        #if UNITY_EDITOR

        // Template mesh rebuilding method
        public void RebuildMesh()
        {
            // Source mesh
            var vtx_in = _sourceShape.vertices;
            var idx_in = _sourceShape.triangles;

            // Calculate the instance count.
            _instanceCount = 65535 / vtx_in.Length;

            // Working buffers
            var vtx_out = new List<Vector3>();
            var uv0_out = new List<Vector3>();
            var idx_out = new List<int>();

            // Repeat the source mesh.
            for (var i = 0; i < _instanceCount; i++)
            {
                foreach (var idx in idx_in)
                    idx_out.Add(idx + vtx_out.Count);

                vtx_out.AddRange(vtx_in);

                var uv0 = new Vector3((i + 0.5f) / _instanceCount, Random.value, Random.value);
                uv0_out.AddRange(Enumerable.Repeat(uv0, vtx_in.Length));
            }

            // Reset the mesh asset.
            _mesh.Clear();
            _mesh.SetVertices(vtx_out);
            _mesh.SetUVs(0, uv0_out);
            _mesh.SetIndices(idx_out.ToArray(), MeshTopology.Triangles, 0);
            _mesh.bounds = new Bounds(Vector3.zero, Vector3.one * 1000);
            ;
            _mesh.UploadMeshData(true);
        }

        #endif

        #endregion

        #region ScriptableObject functions

        void OnEnable()
        {
            if (_mesh == null) {
                _mesh = new Mesh();
                _mesh.name = "Warp Template";
            }
        }

        #endregion
    }
}
