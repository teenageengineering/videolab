using UnityEngine;

namespace Klak.Wiring
{
    [AddComponentMenu("Klak/Wiring/Input/Generic/Float Value")]
    public class FloatValue : NodeBase
    {
        [SerializeField]
        float _floatValue;
        public float floatValue {
            get { return _floatValue; }
            set { _floatValue = value; }
        }

        #region Node I/O

        [SerializeField, Outlet]
        FloatEvent _valueEvent = new FloatEvent();

        #endregion

        float _prevValue;

        #region Monobehaviour

        void Update()
        {
            if (_floatValue != _prevValue)
            {
                _valueEvent.Invoke(_floatValue); 
                _prevValue = _floatValue;
            }
        }

        #endregion
    }
}