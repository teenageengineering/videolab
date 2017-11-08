using UnityEngine;

namespace Klak.Wiring
{
    [AddComponentMenu("Klak/Wiring/Convertion/To Euler")]
    public class ToEuler : NodeBase
    {
        #region Node I/O

        [Inlet]
        public Quaternion rotation {
            set {
                if (!enabled) return;
                _vectorEvent.Invoke(value.eulerAngles);
            }
        }

        [SerializeField, Outlet]
        Vector3Event _vectorEvent = new Vector3Event();

        #endregion
    }
}
