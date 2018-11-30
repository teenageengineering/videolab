using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Serialization;

namespace Bezier
{
    [ExecuteInEditMode]
    [RequireComponent(typeof(RectTransform))]
    [RequireComponent(typeof(Graphic))]
    public class Shape : MonoBehaviour, IMeshModifier
    {
        #region Editor

        [SerializeField, FormerlySerializedAs("subdivisions"), Range(0, 7)]
        int _subdivisions = 4;
        public int subdivisions {
            get { return _subdivisions; }
            set { _subdivisions = value; }
        }

        [SerializeField, FormerlySerializedAs("outline")]
        bool _outline;
        public bool outline {
            get { return _outline; }
            set { _outline = value; }
        }

        [SerializeField]
        float _lineWidth = 1;
        public float lineWidth {
            get { return _lineWidth; }
            set { _lineWidth = value; }
        }

        [SerializeField, FormerlySerializedAs("closedPath")]
        bool _closedPath = true;
        public bool closedPath {
            get { return _closedPath; }
            set { _closedPath = value; }
        }

        [SerializeField, FormerlySerializedAs("snapToSize")]
        bool _snapToSize = true;
        public bool snapToSize {
            get { return _snapToSize; }
            set { _snapToSize = value; }
        }

        #endregion

        #region Public

        public int numTris;

        public RectTransform rectTransform {
            get { return transform as RectTransform; }
        }

        public Graphic graphic {
            get { return GetComponent<Graphic>(); }
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

        public void SetHandlesActive(bool active)
        {
            Handle[] handles = GetHandles();
            foreach (Handle handle in handles)
                handle.gameObject.SetActive(active);
        }

        public void SetReferenceSize()
        {
            _prevSize = rectTransform.rect.size;
        }

        #endregion

        #region Private

        Vector2[] _verts = new Vector2[0];
        int[] _indices = new int[0];

        float MaxRadius(Handle h0, Vector2 v0, Handle h1, Vector2 v1)
        {
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

            return Mathf.Min(r0Max, r1Max);
        }

        // 1 - 4 * (sqrt(2) - 1) / 3
        const float c = 0.4477f;

        IEnumerable<Point> GetAllPoints()
        {
            Handle[] handles = GetHandles();
            Handle prevHandle = handles[handles.Length - 1];
            int N = (this.outline && !this.closedPath) ? handles.Length : handles.Length + 1;
            for (int n = 0; n < N; n++)
            {
                Handle handle = handles[n % handles.Length];

                if (handle.mode == Handle.Mode.Rounded)
                {
                    Handle nextHandle = handles[(n + 1) % handles.Length];

                    Vector2 v0 = handle.pos - prevHandle.pos;
                    Vector2 v1 = handle.pos - nextHandle.pos;

                    float maxRadius = MaxRadius(prevHandle, v0, nextHandle, v1);
                    float r = Mathf.Min(handle.cornerRadius, maxRadius);

                    v0 = v0.normalized * r;
                    Vector2 p0 = handle.pos - v0;
                    Vector2 c0 = handle.pos - v0 * c;
                    yield return new Point(p0, p0, c0);

                    if (n < handles.Length)
                    {
                        v1 = v1.normalized * r;
                        Vector2 p1 = handle.pos - v1;
                        Vector2 c1 = handle.pos - v1 * c;
                        yield return new Point(p1, c1, p1);
                    }
                }
                else
                    yield return handle.point;

                prevHandle = handle;
            }
        }

        void EditVertices()
        {
            Handle[] handles = GetHandles();

            if (handles.Length < 3)
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
                if (this.closedPath && _verts.Length > 2)
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

            for (int i = 0; i < _verts.Length; i++)
            {
                Vector2 v = _verts[i];
                Vector2 uv = new Vector2(v.x / this.rectTransform.rect.width + 0.5f, v.y / this.rectTransform.rect.height + 0.5f);
                vh.AddVert(v, graphic.color, uv);
            }

            for (int i = 0; i < _indices.Length; i += 3)
                vh.AddTriangle(_indices[i], _indices[i + 1], _indices[i + 2]);
        }

        void ScaleHandles(Vector2 scale)
        {
            Handle[] handles = GetHandles();
            for (int i = 0; i < handles.Length; i++)
            {
                Handle handle = handles[i];
                handle.pos = Vector2.Scale(handle.pos, scale);
                handle.control1 = Vector2.Scale(handle.control1, scale);
                handle.control2 = Vector2.Scale(handle.control2, scale);
            }

            // TODO: would be enough to scale verts
            SetNeedsRebuild();
        }

        #endregion

        #region MonoBehaviour

        int _prevSubdivisions;
        bool _prevOutline;
        float _prevLineWidth;
        bool _prevClosedPath;

        Vector2 _prevSize;
        int _prevNumHandles;

        void Update()
        {
            if (_prevSize == Vector2.zero)
                SetReferenceSize();

            Vector2 newSize = this.rectTransform.rect.size;
            if (newSize.x == 0) newSize.x = _prevSize.x;
            if (newSize.y == 0) newSize.y = _prevSize.y;

            if (newSize != _prevSize)
            {
                if (snapToSize)
                    ScaleHandles(new Vector2(newSize.x / _prevSize.x, newSize.y / _prevSize.y));
    
                _prevSize = newSize;
            }

            int numHandles = GetHandles().Length;
            if (numHandles != _prevNumHandles)
            {
                SetNeedsRebuild();
                _prevNumHandles = numHandles;
            }

            if (_subdivisions != _prevSubdivisions)
            {
                SetNeedsRebuild();
                _prevSubdivisions = _subdivisions;
            }

            if (_outline != _prevOutline)
            {
                SetNeedsRebuild();
                _prevOutline = _outline;
            }

            if (_lineWidth != _prevLineWidth)
            {
                SetNeedsRebuild();
                _prevLineWidth = _lineWidth;
            }

            if (_closedPath != _prevClosedPath)
            {
                SetNeedsRebuild();
                _prevClosedPath = _closedPath;
            }

            if (_needsRebuild)
            {
                EditVertices();
                EditTriangles();

                graphic.SetVerticesDirty();

                _needsRebuild = false;
            }
        }

        public void OnValidate()
        {
            if (!Application.isPlaying)
                Update();
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

        #region Helpers

        public static Shape CreateShape(string name)
        {
            GameObject go = new GameObject(name);
            go.AddComponent<Image>();
            Shape shape = go.AddComponent<Shape>();

            return shape;
        }

        public static Shape CreateRect(string name, Vector2 size, float cornerRadius = 0)
        {
            Shape shape = CreateShape(name);

            shape.rectTransform.sizeDelta = size;
            shape.SetReferenceSize();

            Vector3[] localCorners = new Vector3[4];
            shape.rectTransform.GetLocalCorners(localCorners);
            foreach (Vector3 corner in localCorners)
            {
                Handle handleObj = Handle.CreateHandle("Handle", corner, cornerRadius);
                handleObj.transform.SetParent(shape.transform, false);
            }

            return shape;
        }

        public static Shape CreateCircle(string name, float radius)
        {
            return CreateRect(name, new Vector2(radius * 2, radius * 2), float.MaxValue);
        }

        public static Shape CreatePolygon(string name, float radius, int numSides)
        {
            Shape shape = CreateShape(name);

            shape.rectTransform.sizeDelta = new Vector2(radius * 2, radius * 2);
            shape.SetReferenceSize();

            for (int i = 0; i < numSides; i++)
            {
                float phase = 2 * Mathf.PI * i / numSides;
                Vector2 pos = new Vector2(Mathf.Cos(phase), Mathf.Sin(phase));
                Handle handleObj = Handle.CreateHandle("Handle", pos * radius);
                handleObj.transform.SetParent(shape.transform, false);
            }

            return shape;
        }

        #endregion
    }
}
