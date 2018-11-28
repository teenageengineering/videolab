using UnityEngine;
using System.Reflection;

namespace Klak.Wiring
{
    [AddComponentMenu("Klak/Wiring/Output/Generic/String Out")]
    public class StringOut : NodeBase
    {
        #region Editable properties

        [SerializeField]
        Component _target;

        [SerializeField]
        string _propertyName;

        [SerializeField]
        [Tooltip("Standard numeric format string.")]
        string _format = "0.00";

        #endregion

        #region Node I/O

        [Inlet]
        public float input {
            set {
                if (!enabled || _target == null || _propertyInfo == null) return;
                string stringValue = value.ToString(_format);
                _propertyInfo.SetValue(_target, stringValue, null);
            }
        }

        #endregion

        #region Private members

        PropertyInfo _propertyInfo;

        void OnEnable()
        {
            if (_target == null || string.IsNullOrEmpty(_propertyName)) return;
            _propertyInfo = _target.GetType().GetProperty(_propertyName);
        }

        #endregion
    }
}