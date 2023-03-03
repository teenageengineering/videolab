// Klak - Creative coding library for Unity
// https://github.com/keijiro/Klak

using UnityEngine;

namespace Klak.Wiring
{
    [AddComponentMenu("Klak/Wiring/Filter/Float Gate")]
    public class FloatGate : NodeBase
    {
        #region Editable properties

        [SerializeField]
        bool _state;

        #endregion

        #region Node I/O

        [Inlet]
        public float input
        {
            set
            {
                if (_state) _outputEvent.Invoke(value);
            }
        }
        

        [Inlet]
        public void Open()
        {
            _state = true;
        }

        [Inlet]
        public void Close()
        {
            _state = false;
        }

        [SerializeField, Outlet]
        FloatEvent _outputEvent = new FloatEvent();

        #endregion
    }
}
