using UnityEngine;
using System.Reflection;

namespace Klak.Wiring
{
    [AddComponentMenu("Klak/Wiring/Input/Generic/Bool Input")]
    public class BoolInput : NodeBase
    {
        #region Editable properties

        [SerializeField]
        Component _target;

        [SerializeField]
        string _propertyName;

        #endregion

        #region Node I/O

        [SerializeField, Outlet]
        FloatEvent _valueEvent = new FloatEvent();

        #endregion

        #region Private members

        PropertyInfo _propertyInfo;
        float _value = float.MaxValue;

        void OnEnable()
        {
            if (_target == null || string.IsNullOrEmpty(_propertyName)) return;
            _propertyInfo = _target.GetType().GetProperty(_propertyName);
        }

        void Update()
        {
            if (_target == null || string.IsNullOrEmpty(_propertyName)) return;
            float value = System.Convert.ToSingle(_propertyInfo.GetValue(_target, null));
            if (value != _value)
                _valueEvent.Invoke(value);

            _value = value;
        }

        #endregion
    }
}