using UnityEngine;

namespace Klak.Wiring
{
    [AddComponentMenu("Klak/Wiring/Input/Generic/Vector Value")]
    public class VectorValue : NodeBase
    {
        [SerializeField]
        Vector3 _vectorValue;
        public Vector3 vectorValue {
            get { return _vectorValue; }
            set { _vectorValue = value; }
        }

        #region Node I/O

        [SerializeField, Outlet]
        Vector3Event _valueEvent = new Vector3Event();

        #endregion

        Vector3 _prevValue;

        #region Monobehaviour

        void Update()
        {
            if (_vectorValue != _prevValue)
            {
                _valueEvent.Invoke(_vectorValue); 
                _prevValue = _vectorValue;
            }
        }

        #endregion
    }
}