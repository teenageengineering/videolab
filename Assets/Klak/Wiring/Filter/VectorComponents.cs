using UnityEngine;

namespace Klak.Wiring
{
    [AddComponentMenu("Klak/Wiring/Convertion/Vector Components")]
    public class VectorComponents : NodeBase
    {
        #region Node I/O

        [Inlet]
        public Vector3 vector {
            set {
                if (!enabled) 
                    return;
                
                _xEvent.Invoke(value.x);
                _yEvent.Invoke(value.y);
                _zEvent.Invoke(value.z);

                _lengthEvent.Invoke(value.magnitude);
            }
        }

        [SerializeField, Outlet]
        FloatEvent _xEvent = new FloatEvent();

        [SerializeField, Outlet]
        FloatEvent _yEvent = new FloatEvent();

        [SerializeField, Outlet]
        FloatEvent _zEvent = new FloatEvent();

        [SerializeField, Outlet]
        FloatEvent _lengthEvent = new FloatEvent();

        #endregion
    }
}
