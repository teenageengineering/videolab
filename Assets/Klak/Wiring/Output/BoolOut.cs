using UnityEngine;
using System.Reflection;

namespace Klak.Wiring
{
    [AddComponentMenu("Klak/Wiring/Output/Generic/Bool Out")]
    public class BoolOut : NodeBase
    {
        #region Editable properties

        [SerializeField]
        Component _target;

        [SerializeField]
        string _propertyName;

        #endregion

        #region Node I/O

        [Inlet]
        public float input
        {
            set
            {
                if (!enabled || _target == null || _propertyInfo == null) return;
                _propertyInfo.SetValue(_target, System.Convert.ToBoolean(value), null);
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
