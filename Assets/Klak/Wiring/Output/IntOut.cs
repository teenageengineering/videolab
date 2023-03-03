using UnityEngine;
using System.Reflection;

namespace Klak.Wiring
{
    [AddComponentMenu("Klak/Wiring/Output/Generic/Int Out")]
    public class IntOut : NodeBase
    {
        #region Editable properties

        [SerializeField]
        Component _target;

        [SerializeField]
        string _propertyName;

        [SerializeField]
        string _particleSystemModuleName = "<none>";

        #endregion

        #region Node I/O

        [Inlet]
        public float input
        {
            set
            {
                if (!enabled || _target == null || _propertyInfo == null) return;

                if (_target.GetType() == typeof(ParticleSystem) && _particleSystemModuleName != "<none>")
                {
                    _propertyInfo.SetValue(_boxedStruct, System.Convert.ToInt32(value), null);
                }
                    
                else _propertyInfo.SetValue(_target, System.Convert.ToInt32(value), null);
                
            }
        }

        #endregion

        #region Private members

        PropertyInfo _propertyInfo;
        PropertyInfo _moduleInfo;
        object _boxedStruct;

        void OnEnable()
        {
            if (_target == null || string.IsNullOrEmpty(_propertyName)) return;

            else
            {
                if (_target.GetType() == typeof(ParticleSystem) && _particleSystemModuleName != "<none>")
                {
                    _moduleInfo = _target.GetType().GetProperty(_particleSystemModuleName);
                    _propertyInfo = _moduleInfo.PropertyType.GetProperty(_propertyName);
                    _boxedStruct = _moduleInfo.GetValue(_target);
                }

                else
                {
                    _propertyInfo = _target.GetType().GetProperty(_propertyName);
                }
            }            
        }

        #endregion
    }
}
