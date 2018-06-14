// Mesh deformation towards the normal vector.
// By Keijiro Takahashi, 2014

using UnityEngine;
using System.Collections;

namespace Kvant
{
    public class NormalDeformer : MonoBehaviour
    {
        // Public properties.

        [SerializeField]
        float _deformAmount = 1.0f;
        public float deformAmount {
            get { return _deformAmount; }
            set { _deformAmount = value; }
        }

        [SerializeField]
        float _noiseScale = 1.0f;
        public float noiseScale {
            get { return _noiseScale; }
            set { _noiseScale = value; }
        }

        public bool smooth = true;
        public bool fractal = true;
        public int octave = 3;
        public Vector3 noiseVelocity = Vector3.right * 0.2f;
        public Vector3 noiseOffset;

        // Main mesh object.
        Mesh mesh;

        // Source mesh information.
        int[] index;
        Vector3[] position;
        Vector3[] normal;

        // Temporary vertex arrays.
        Vector3[] temp1;
        Vector3[] temp2;

        void Awake ()
        {
            var meshFilter = GetComponent<MeshFilter> ();
            var source = meshFilter.sharedMesh;

            // Copy the basic information.
            index = source.GetIndices(0);
            position = source.vertices;
            normal = source.normals;

            // Make a new mesh.
            mesh = new Mesh ();
            mesh.MarkDynamic ();

            if (smooth)
            {
                // Smooth shading: the mash has same number of vertices.
                temp1 = new Vector3[position.Length];
                mesh.vertices = temp1;

                // Simply copy the UVs.
                mesh.uv = source.uv;
                mesh.uv2 = source.uv2;

                // Simply copy the indices.
                mesh.SetIndices(index, MeshTopology.Triangles, 0);
            }
            else
            {
                // Flat shading: split the vertices for each triangle.
                temp1 = new Vector3[position.Length];
                temp2 = new Vector3[index.Length];
                mesh.vertices = temp2;

                // Make uv array for the split triangles.
                var sourceUv = source.uv;
                if (sourceUv != null && sourceUv.Length > 0)
                {
                    var uv = new Vector2[index.Length];
                    for (var i = 0; i < index.Length; i++)
                        uv[i] = sourceUv[index[i]];
                    mesh.uv = uv;
                }

                // Make 2nd-uv array for the split triangles.
                var sourceUv2 = source.uv2;
                if (sourceUv2 != null && sourceUv2.Length > 0)
                {
                    var uv2 = new Vector2[index.Length];
                    for (var i = 0; i < index.Length; i++)
                        uv2[i] = sourceUv2[index[i]];
                    mesh.uv2 = uv2;
                }

                // Make an index array for the split triangles.
                var index2 = new int[index.Length];
                for (var i = 0; i < index.Length; i++)
                    index2[i] = i;

                mesh.SetIndices(index2, MeshTopology.Triangles, 0);
            }

            // Set the new mesh to the renderer.
            meshFilter.sharedMesh = mesh;
        }

        void Update ()
        {
            // Move the offset vector.
            noiseOffset += noiseVelocity * Time.deltaTime;

            // Move the vertices.
            if (fractal)
            {
                for (var i = 0; i < position.Length; i++)
                {
                    var v = position[i];
                    var c = (v + noiseOffset) * noiseScale;
                    var d = Math.Fractal(c, octave) * deformAmount;
                    temp1[i] = v + normal[i] * d;
                }
            }
            else
            {
                for (var i = 0; i < position.Length; i++)
                {
                    var v = position[i];
                    var c = (v + noiseOffset) * noiseScale;
                    var d = Math.Fractal4Coeffs(c, 1, 2, 4, 8) * deformAmount * 0.03125f;
                    temp1[i] = v + normal[i] * d;
                }
            }

            if (smooth)
            {
                // Simply copy the moved vertices.
                mesh.vertices = temp1;
            }
            else
            {
                // Split the moved vertices.
                for (var i = 0; i < index.Length; i++)
                    temp2[i] = temp1[index[i]];
                mesh.vertices = temp2;
            }

            // Rebuild the mesh.
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
        }
    }
}
