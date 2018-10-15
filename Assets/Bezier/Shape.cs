using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using System.Linq;

namespace Bezier
{
    [ExecuteInEditMode]
    [RequireComponent(typeof(Graphic))]
    public class Shape : MonoBehaviour, IMeshModifier
    {
        #region Editor

        [Range(0, 7)]
        public int subdivisions = 4;

        public bool outline;

        [SerializeField]
        float _lineWidth = 1;
        public float lineWidth {
            get { return _lineWidth; }
            set { _lineWidth = value; }
        }

        public bool closedPath = true;

        public bool snapToSize = true;

        #endregion

        #region Public

        public int numTris;

        public RectTransform rectTransform {
            get { return transform as RectTransform; }
        }

        public Handle[] GetHandles()
        {
            List<Handle> handles = new List<Handle>();

            Handle handle;

            foreach (RectTransform child in transform)
                if (handle = child.GetComponent<Handle>())
                    handles.Add(handle);

            return handles.ToArray();
        }

        public Handle AddHandle(string name, Vector2 pos, float cornerRadius = 0)
        {
            GameObject go = new GameObject(name);

            Handle handle = go.AddComponent<Handle>();
            handle.pos = pos;

            if (cornerRadius > 0)
            {
                handle.mode = Handle.Mode.Rounded;
                handle.cornerRadius = cornerRadius;
            }

            go.transform.SetParent(transform, false);

            return handle;
        }

        // TODO http://www.iquilezles.org/www/articles/bezierbbox/bezierbbox.htm
        public void Envelope()
        {
            Vector2 min = new Vector2(float.MaxValue, float.MaxValue);
            Vector2 max = new Vector2(float.MinValue, float.MinValue);

            Handle[] handles = GetHandles();
            foreach (Handle handle in handles)
            {
                min = Vector2.Min(handle.pos, min);
                max = Vector2.Max(handle.pos, max);
            }

            RectTransform rt = transform as RectTransform;
            rt.sizeDelta = max - min;
            rt.anchoredPosition = (max + min) / 2;
            foreach (RectTransform child in transform)
                child.anchoredPosition -= rt.anchoredPosition;
        }

        bool _needsRebuild;
        public void SetNeedsRebuild()
        {
            _needsRebuild = true;
        }

        #endregion

        #region Private

        Vector2 _prevSize;
        int _prevNumHandles;
        float _prevLineWidth;

        Vector2[] _verts = new Vector2[0];
        int[] _indices = new int[0];

        IEnumerable<Point> GetAllPoints()
        {
            Handle[] handles = GetHandles();
            int N = (this.outline && !this.closedPath) ? handles.Length : handles.Length + 1;
            for (int n = 0; n < N; n++)
            {
                Handle handle = handles[n % handles.Length];

                if (handle.mode == Handle.Mode.Rounded)
                {
                    float c = 4f * (Mathf.Sqrt(2f) - 1f) / 3f;

                    Handle h0 = handles[(n + handles.Length - 1) % handles.Length];
                    Handle h1 = handles[(n + 1) % handles.Length];

                    Vector2 v0 = handle.pos - h0.pos;
                    Vector2 v1 = handle.pos - h1.pos;

                    float r0Max = v0.magnitude;
                    if (h0.mode == Handle.Mode.Rounded)
                        r0Max = v0.magnitude / 2;
                    else if (h0.control2 != Vector3.zero)
                        r0Max = 0;

                    float r1Max = v1.magnitude;
                    if (h1.mode == Handle.Mode.Rounded)
                        r1Max = v1.magnitude / 2;
                    else if (h1.control1 != Vector3.zero)
                        r1Max = 0;

                    float rMax = Mathf.Min(r0Max, r1Max);
                    float r = Mathf.Min(handle.cornerRadius, rMax);

                    Vector2 p0 = handle.pos - v0.normalized * r;
                    Vector2 p1 = handle.pos - v1.normalized * r;

                    Vector2 c0 = p0 + v0.normalized * c * r;
                    Vector2 c1 = p1 + v1.normalized * c * r;

                    yield return new Point(p0, p0, c0);
                    if (n < handles.Length)
                        yield return new Point(p1, c1, p1);
                }
                else
                    yield return handle.point;
            }
        }

        void EditVertices()
        {
            Handle[] handles = GetHandles();

            if (handles.Length < 2)
            {
                if (this.outline && handles.Length > 0)
                    this.closedPath = false;
                else
                    return;
            }

            var pointsEnum = GetAllPoints().GetEnumerator();
            pointsEnum.MoveNext();
            Point prevPt = pointsEnum.Current;

            List<Vector2> verts = new List<Vector2>();
            Vector2 prevVert = prevPt.p;

            while (pointsEnum.MoveNext())
            {
                Point pt = pointsEnum.Current;

                int f = 1 << this.subdivisions;
                // approximate with straight line?
                if (((prevPt.c[1] - prevPt.p).magnitude < Curve.precision) && ((pt.c[0] - pt.p).magnitude < Curve.precision))
                    f = 1;

                // interpolate
                for (int i = 0; i < f; i++)
                {
                    float t = (float)(i + 1) / f;
                    Vector2 vert = Curve.Eval(prevPt, pt, t);

                    // trim overlapping verts
                    if ((vert - prevVert).magnitude < Curve.precision)
                        continue;

                    if (this.outline)
                    {
                        Vector2 d = (vert - prevVert).normalized;
                        d = new Vector2(-d.y, d.x) * this.lineWidth / 2;

                        verts.Add(prevVert + d);
                        verts.Add(prevVert - d);

                        verts.Add(vert + d);
                        verts.Add(vert - d);
                    }
                    else
                        verts.Add(prevVert);

                    prevVert = vert;
                }

                prevPt = pt;
            }

            _verts = verts.ToArray();
        }

        void EditTriangles()
        {
            if (this.outline)
            {
                List<int> indices = new List<int>();

                for (int i = 0; i < _verts.Length - 2; i += 2)
                {
                    indices.Add(i); indices.Add(i + 1); indices.Add(i + 2);
                    indices.Add(i + 1); indices.Add(i + 2); indices.Add(i + 3);
                }

                // closing joint
                if (this.closedPath)
                {
                    indices.Add(_verts.Length - 2); indices.Add(_verts.Length - 1); indices.Add(0);
                    indices.Add(_verts.Length - 1); indices.Add(0); indices.Add(1);
                }

                _indices = indices.ToArray();
            }
            else
            {
                Triangulator triangulator = new Triangulator(_verts);
                _indices = triangulator.Triangulate();
            }

            this.numTris = _indices.Length / 3;
        }

        void EditMesh(VertexHelper vh)
        {
            vh.Clear();

            Graphic graphic = GetComponent<Graphic>();
            for (int i = 0; i < _verts.Length; i++)
            {
                Vector2 v = _verts[i];
                Vector2 uv = new Vector2(v.x / this.rectTransform.rect.width + 0.5f, v.y / this.rectTransform.rect.height + 0.5f);
                vh.AddVert(v, graphic.color, uv);
            }

            for (int i = 0; i < _indices.Length; i += 3)
                vh.AddTriangle(_indices[i], _indices[i + 1], _indices[i + 2]);
        }

        void ScaleHandles()
        {
            if (_prevSize == Vector2.zero)
                return;

            Vector2 scale = new Vector2(this.rectTransform.rect.size.x / _prevSize.x, this.rectTransform.rect.size.y / _prevSize.y);

            Handle[] handles = GetHandles();
            for (int i = 0; i < handles.Length; i++)
            {
                Handle handle = handles[i];
                handle.pos = Vector2.Scale(handle.pos, scale);
                handle.control1 = Vector2.Scale(handle.control1, scale);
                handle.control2 = Vector2.Scale(handle.control2, scale);
            }
        }

        #endregion

        #region MonoBehaviour

        void Update()
        {
            if (this.rectTransform.rect.size != _prevSize)
            {
                if (snapToSize)
                    ScaleHandles();
                SetNeedsRebuild();
                _prevSize = this.rectTransform.rect.size;
            }

            int numHandles = GetHandles().Length;
            if (numHandles != _prevNumHandles)
            {
                SetNeedsRebuild();
                _prevNumHandles = numHandles;
            }

            if (_lineWidth != _prevLineWidth)
            {
                SetNeedsRebuild();
                _prevLineWidth = _lineWidth;
            }

            if (_needsRebuild)
            {
                EditVertices();
                EditTriangles();

                Graphic graphic = GetComponent<Graphic>();
                graphic.SetVerticesDirty();

                _needsRebuild = false;
            }
        }

        public void OnValidate()
        {
            SetNeedsRebuild();
        }

        #endregion

        #region IMeshModifier

        public void ModifyMesh(VertexHelper vh)
        {
            if (!enabled)
                return;

            EditMesh(vh);
        }

        public void ModifyMesh(Mesh mesh)
        {
            if (!enabled)
                return;

            using (VertexHelper vh = new VertexHelper(mesh))
            {
                EditMesh(vh);
                vh.FillMesh(mesh);
            }
        }

        #endregion
    }
}
