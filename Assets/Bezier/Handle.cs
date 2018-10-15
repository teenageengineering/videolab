using System;
using UnityEngine;

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

        public Mode mode;

        // properties for animation and Klak access

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

        float _prevCornerRadius;
        Vector3 _prevControl1;
        Vector3 _prevControl2;

        Vector2 _prevPos;
        Quaternion _prevRotation;
        Vector3 _prevScale;

        Mode _prevMode;

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

        void Awake()
        {
            this.rectTransform.sizeDelta = Vector2.one;
        }

        void Update()
        {
            if (this.cornerRadius != _prevCornerRadius)
            {
                if (this.cornerRadius < 0)
                    this.cornerRadius = 0;

                _prevCornerRadius = this.cornerRadius;
                UpdateParent();
            }

            if (this.control1 != _prevControl1)
            {
                _prevControl1 = this.control1;
                EnforceMode(false);
                UpdateParent();
            }

            if (this.control2 != _prevControl2)
            {
                _prevControl2 = this.control2;
                EnforceMode(true);
                UpdateParent();
            }

            if (this.pos != _prevPos)
            {
                _prevPos = this.pos;
                UpdateParent();
            }

            if (transform.localRotation != _prevRotation)
            {
                _prevRotation = transform.localRotation;
                UpdateParent();
            }

            if (transform.localScale != _prevScale)
            {
                _prevScale = transform.localScale;
                UpdateParent();
            }

            if (this.mode != _prevMode)
            {
                _prevMode = this.mode;
                EnforceMode(false);
                UpdateParent();
            }
        }

        public void OnValidate()
        {
            Update();
        }

        #endregion
    }
}
