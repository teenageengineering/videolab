using UnityEngine;

namespace Klak.Wiring
{
    [AddComponentMenu("Klak/Wiring/Input/Generic/String Value")]
    public class StringValue : NodeBase
    {
        [SerializeField]
        public string _textValue;

        #region Node I/O

        [SerializeField, Outlet]
        StringEvent _stringEvent = new StringEvent();

        #endregion

        string _prevValue = null;

        #region Monobehaviour

        void Update()
        {
            if (_textValue != _prevValue)
            {
                _stringEvent.Invoke(_textValue); 
                _prevValue = _textValue;
            }
        }

        #endregion
    }
}