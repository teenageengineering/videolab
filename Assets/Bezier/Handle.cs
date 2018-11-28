using UnityEngine;
using UnityEngine.Serialization;

namespace Bezier
{
    [ExecuteInEditMode]
    [RequireComponent(typeof(RectTransform))]
    public class Handle : MonoBehaviour {

        #region Editor

        public enum Mode {
            Free,
            Aligned,
            Mirrored,
            Rounded
        }

        [SerializeField, FormerlySerializedAs("mode")]
        Mode _mode;
        public Mode mode {
            get { return _mode; }
            set { _mode = value; }
        }

        [SerializeField]
        float _cornerRadius;
        public float cornerRadius {
            get { return _cornerRadius; }
            set { _cornerRadius = value; }
        }

        [SerializeField]
        Vector2 _control1;
        public Vector3 control1 {
            get { return _control1; }
            set { _control1 = value; }
        }

        [SerializeField]
        Vector2 _control2;
        public Vector3 control2 {
            get { return _control2; }
            set { _control2 = value; }
        }

        #endregion

        #region Public

        public Vector2 pos {
            get { return this.rectTransform.anchoredPosition; }
            set { this.rectTransform.anchoredPosition = value; }
        }

        public RectTransform rectTransform {
            get { return transform as RectTransform; }
        }

        public Point point { 
            get {
                Vector2 p = transform.localPosition;
                Vector2 c1 = Vector2.Scale(transform.localRotation * this.control1, transform.localScale);
                Vector2 c2 = Vector2.Scale(transform.localRotation * this.control2, transform.localScale);
                return new Point(p, p + c1, p + c2);
            }
        }

        #endregion

        #region Private

        void EnforceMode(bool control2Free)
        {
            if (this.mode == Mode.Free)
                return;

            Vector2 freeControl = (control2Free) ? this.control2 : this.control1;
            Vector2 enforcedControl = (control2Free) ? this.control1 : this.control2;

            Vector2 tangent = freeControl;
            if (this.mode == Mode.Aligned)
                tangent = tangent.normalized * enforcedControl.magnitude;
            enforcedControl = -tangent;

            if (control2Free)
                this.control1 = enforcedControl;
            else
                this.control2 = enforcedControl;
        }

        void UpdateParent()
        {
            if (transform.parent)
            {
                Shape parentShape = transform.parent.GetComponent<Shape>();
                if (parentShape)
                    parentShape.SetNeedsRebuild();
            }
        }

        #endregion

        #region MonoBehaviour

        Mode _prevMode;
        float _prevCornerRadius;
        Vector3 _prevControl1;
        Vector3 _prevControl2;

        Vector2 _prevPos;
        Quaternion _prevRotation;
        Vector3 _prevScale;


        void Awake()
        {
            this.rectTransform.sizeDelta = Vector2.one;
        }

        void Update()
        {
            if (this.mode != _prevMode)
            {
                EnforceMode(false);
                UpdateParent();

                _prevMode = this.mode;
            }

            if (this.cornerRadius != _prevCornerRadius)
            {
                if (this.cornerRadius < 0)
                    this.cornerRadius = 0;

                UpdateParent();

                _prevCornerRadius = this.cornerRadius;
            }

            if (this.control1 != _prevControl1)
            {
                EnforceMode(false);
                UpdateParent();

                _prevControl1 = this.control1;
            }

            if (this.control2 != _prevControl2)
            {
                EnforceMode(true);
                UpdateParent();

                _prevControl2 = this.control2;
            }

            if (this.pos != _prevPos)
            {
                UpdateParent();

                _prevPos = this.pos;
            }

            if (transform.localRotation != _prevRotation)
            {
                UpdateParent();

                _prevRotation = transform.localRotation;
            }

            if (transform.localScale != _prevScale)
            {
                UpdateParent();

                _prevScale = transform.localScale;
            }
        }

        public void OnValidate()
        {
            if (!Application.isPlaying)
                Update();
        }

        #endregion

        #region Helpers

        public static Handle CreateHandle(string name, Vector2 pos, float cornerRadius = 0)
        {
            GameObject go = new GameObject(name);

            Handle handle = go.AddComponent<Handle>();
            handle.pos = pos;

            if (cornerRadius > 0)
            {
                handle.mode = Handle.Mode.Rounded;
                handle.cornerRadius = cornerRadius;
            }

            return handle;
        }

        #endregion
    }
}
