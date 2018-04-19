using UnityEngine;

namespace Klak.Wiring
{
    [AddComponentMenu("Klak/Wiring/Convertion/Component Vector")]
    public class ComponentVector : NodeBase
    {
        Vector3 _vector;

        #region Node I/O

        [Inlet]
        public float x {
            set {
                if (!enabled) return;
                _vector.x = value;
                _vectorEvent.Invoke(_vector); 
            }
        }

        [Inlet]
        public float y {
            set {
                if (!enabled) return;
                _vector.y = value;
                _vectorEvent.Invoke(_vector); 
            }
        }

        [Inlet]
        public float z {
            set {
                if (!enabled) return;
                _vector.z = value;
                _vectorEvent.Invoke(_vector); 
            }
        }

        [SerializeField, Outlet]
        Vector3Event _vectorEvent = new Vector3Event();

        #endregion
    }
}
