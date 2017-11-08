using UnityEngine;

namespace Klak.Wiring
{
    [AddComponentMenu("Klak/Wiring/Input/Transform Input")]
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

        void Update()
        {
            if (_useLocalValues)
                _positionEvent.Invoke(_transform.localPosition);
            else
                _positionEvent.Invoke(_transform.position);

            if (_useLocalValues)
                _rotationEvent.Invoke(_transform.localRotation);
            else
                _rotationEvent.Invoke(_transform.rotation);

            _scaleEvent.Invoke(_transform.localScale);
        }

        #endregion
    }
}
