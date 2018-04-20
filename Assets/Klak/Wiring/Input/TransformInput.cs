using UnityEngine;

namespace Klak.Wiring
{
    [AddComponentMenu("Klak/Wiring/Input/Component/Transform Input")]
    public class TransformInput : NodeBase
    {
        #region Editable properties

        [SerializeField]
        Transform _transform;

        [SerializeField]
        bool _useLocalValues;

        #endregion

        #region Node I/O

        [SerializeField, Outlet]
        Vector3Event _positionEvent = new Vector3Event();

        [SerializeField, Outlet]
        QuaternionEvent _rotationEvent = new QuaternionEvent();

        [SerializeField, Outlet]
        Vector3Event _scaleEvent = new Vector3Event();

        #endregion

        #region MonoBehaviour functions

        Vector3 _prevPosition;
        Quaternion _prevRotation;
        Vector3 _prevScale;

        void UpdatePosition(Vector3 position)
        {
            if (position != _prevPosition)
            {
                _positionEvent.Invoke(position);
                _prevPosition = position;
            }
        }

        void UpdateRotation(Quaternion rotation)
        {
            if (rotation != _prevRotation)
            {
                _rotationEvent.Invoke(rotation);
                _prevRotation = rotation;
            }
        }

        void UpdateScale()
        {
            if (_transform.localScale != _prevScale)
            {
                _scaleEvent.Invoke(_transform.localScale);
                _prevScale = _transform.localScale;
            }
        }

        void Update()
        {
            if (_useLocalValues)
            {
                UpdatePosition(_transform.localPosition);
                UpdateRotation(_transform.localRotation);
            }
            else
            {
                UpdatePosition(_transform.position);
                UpdateRotation(_transform.rotation);
            }

            UpdateScale();
        }

        #endregion
    }
}
