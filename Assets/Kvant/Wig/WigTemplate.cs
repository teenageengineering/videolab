using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace Kvant
{
    public class WigTemplate : ScriptableObject
    {
        #region Public properties

        /// Number of segments (editable)
        public int segmentCount {
            get { return _segmentCount; }
        }

        [SerializeField] int _segmentCount = 8;

        /// Number of filaments (read only)
        public int filamentCount {
            get { return _foundation.width; }
        }

        /// Foundation texture (read only)
        public Texture2D foundation {
            get { return _foundation; }
        }

        [SerializeField] Texture2D _foundation;

        /// Tmplate mesh (read only)
        public Mesh mesh {
            get { return _mesh; }
        }

        [SerializeField] Mesh _mesh;

        #endregion

        #region Public methods

        #if UNITY_EDITOR

        // Asset initialization method
        public void Initialize(Mesh source)
        {
            if (_foundation != null)
            {
                Debug.LogError("Already initialized");
                return;
            }

            // Input vertices
            var inVertices = source.vertices;
            var inNormals = source.normals;

            // Output vertices
            var outVertices = new List<Vector3>();
            var outNormals = new List<Vector3>();

            // Enumerate unique vertices
            for (var i = 0; i < inVertices.Length; i++)
            {
                if (!outVertices.Any(_ => _ == inVertices[i]))
                {
                    outVertices.Add(inVertices[i]);
                    outNormals.Add(inNormals[i]);
                }
            }

            // Create a texture to store the foundation.
            var tex = new Texture2D(outVertices.Count, 2, TextureFormat.RGBAFloat, false);
            tex.name = "Wig Foundation";
            tex.filterMode = FilterMode.Point;
            tex.wrapMode = TextureWrapMode.Clamp;

            // Store the vertices into the texture.
            for (var i = 0; i < outVertices.Count; i++)
            {
                var v = outVertices[i];
                var n = outNormals[i];
                tex.SetPixel(i, 0, new Color(v.x, v.y, v.z, 1));
                tex.SetPixel(i, 1, new Color(n.x, n.y, n.z, 0));
            }

            // Finish up the texture.
            tex.Apply(false, true);
            _foundation = tex;

            // Build the initial template mesh.
            RebuildMesh();
        }

        #endif

        // Template mesh rebuild method
        public void RebuildMesh()
        {
            _mesh.Clear();

            // The number of vertices in the foundation == texture width
            var vcount = _foundation.width;
            var length = Mathf.Clamp(_segmentCount, 3, 64);

            // Create vertex array for the template.
            var vertices = new List<Vector3>();
            var normals = new List<Vector3>();
            var uvs = new List<Vector2>();

            for (var i1 = 0; i1 < vcount; i1++)
            {
                var u = (i1 + 0.5f) / vcount;

                for (var i2 = 0; i2 < length; i2++)
                {
                    var v = (i2 + 0.5f) / length;

                    for (var i3 = 0; i3 < 8; i3++)
                        uvs.Add(new Vector2(u, v));

                    vertices.Add(new Vector3(-1, -1, 0));
                    vertices.Add(new Vector3(+1, -1, 0));

                    vertices.Add(new Vector3(+1, -1, 0));
                    vertices.Add(new Vector3(+1, +1, 0));

                    vertices.Add(new Vector3(+1, +1, 0));
                    vertices.Add(new Vector3(-1, +1, 0));

                    vertices.Add(new Vector3(-1, +1, 0));
                    vertices.Add(new Vector3(-1, -1, 0));

                    normals.Add(new Vector3(0, -1, 0));
                    normals.Add(new Vector3(0, -1, 0));

                    normals.Add(new Vector3(1, 0, 0));
                    normals.Add(new Vector3(1, 0, 0));

                    normals.Add(new Vector3(0, 1, 0));
                    normals.Add(new Vector3(0, 1, 0));

                    normals.Add(new Vector3(-1, 0, 0));
                    normals.Add(new Vector3(-1, 0, 0));
                }
            }

            // Construct a index array of the mesh.
            var indices = new List<int>();
            var refi = 0;

            for (var i1 = 0; i1 < vcount; i1++)
            {
                for (var i2 = 0; i2 < length - 1; i2++)
                {
                    for (var i3 = 0; i3 < 8; i3 += 2)
                    {
                        indices.Add(refi + i3);
                        indices.Add(refi + i3 + 1);
                        indices.Add(refi + i3 + 8);

                        indices.Add(refi + i3 + 1);
                        indices.Add(refi + i3 + 9);
                        indices.Add(refi + i3 + 8);
                    }
                    refi += 8;
                }
                refi += 8;
            }

            // Reset the mesh asset.
            _mesh.SetVertices(vertices);
            _mesh.SetNormals(normals);
            _mesh.SetUVs(0, uvs);
            _mesh.SetIndices(indices.ToArray(), MeshTopology.Triangles, 0);
            _mesh.bounds = new Bounds(Vector3.zero, Vector3.one * 1000);
            ;
            _mesh.UploadMeshData(true);
        }

        #endregion

        #region ScriptableObject functions

        void OnEnable()
        {
            if (_mesh == null)
            {
                _mesh = new Mesh();
                _mesh.name = "Wig Template";
            }
        }

        #endregion
    }
}
