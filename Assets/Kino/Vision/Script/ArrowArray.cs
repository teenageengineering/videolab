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
using System.Collections.Generic;

namespace Kino
{
    public partial class Vision
    {
        class ArrowArray 
        {
            public Mesh mesh { get { return _mesh; } }

            Mesh _mesh;

            public int columnCount { get; private set; }
            public int rowCount { get; private set; }

            public void BuildMesh(int columns, int rows)
            {
                // base shape
                var arrow = new Vector3[6]
                {
                    new Vector3( 0, 0, 0),
                    new Vector3( 0, 1, 0),
                    new Vector3( 0, 1, 0),
                    new Vector3(-1, 1, 0),
                    new Vector3( 0, 1, 0),
                    new Vector3( 1, 1, 0)
                };

                // make the vertex array
                var vcount = 6 * columns * rows;
                var vertices = new List<Vector3>(vcount);
                var uvs = new List<Vector2>(vcount);

                for (var iy = 0; iy < rows; iy++)
                {
                    for (var ix = 0; ix < columns; ix++)
                    {
                        var uv = new Vector2(
                            (0.5f + ix) / columns,
                            (0.5f + iy) / rows
                        );

                        for (var i = 0; i < 6; i++)
                        {
                            vertices.Add(arrow[i]);
                            uvs.Add(uv);
                        }
                    }
                }

                // make the index array
                var indices = new int[vcount];

                for (var i = 0; i < vcount; i++)
                    indices[i] = i;

                // initialize the mesh object
                _mesh = new Mesh();
                _mesh.hideFlags = HideFlags.DontSave;

                _mesh.SetVertices(vertices);
                _mesh.SetUVs(0, uvs);
                _mesh.SetIndices(indices, MeshTopology.Lines, 0);

                ;
                _mesh.UploadMeshData(true);

                // update the properties
                columnCount = columns;
                rowCount = rows;
            }

            public void DestroyMesh()
            {
                if (_mesh != null) DestroyImmediate(_mesh);
                _mesh = null;
            }
        }
    }
}
