using UnityEngine;
using System.Reflection;

namespace Klak.Wiring
{
    [AddComponentMenu("Klak/Wiring/Input/Generic/Vector Input")]
    public class VectorInput : NodeBase
    {
        #region Editable properties

        [SerializeField]
        Component _target;

        [SerializeField]
        string _propertyName;

        #endregion

        #region Node I/O

        [SerializeField, Outlet]
        Vector3Event _valueEvent = new Vector3Event();

        #endregion

        #region Private members

        PropertyInfo _propertyInfo;
        Vector3 _value;

        void OnEnable()
        {
            if (_target == null || string.IsNullOrEmpty(_propertyName)) return;
            _propertyInfo = _target.GetType().GetProperty(_propertyName);
        }

        void Update()
        {
            if (_target == null || string.IsNullOrEmpty(_propertyName)) return;
            Vector3 value = (Vector3)_propertyInfo.GetValue(_target, null);
            if (value != _value)
                _valueEvent.Invoke(value);

            _value = value;
        }

        #endregion
    }
}
