using UnityEngine;

namespace Klak.Wiring
{
    [AddComponentMenu("Klak/Wiring/Convertion/Component Vector")]
    public class ComponentVector : NodeBase
    {
        #region Editable properties

        [SerializeField]
        Vector3 _vector;
        public Vector3 vector {
            get { return _vector; }
            set { _vector = value; }
        }

        #endregion

        #region Node I/O

        [Inlet]
        public float x {
            set {
                if (!enabled) return;
                _vector.x = value;
            }
        }

        [Inlet]
        public float y {
            set {
                if (!enabled) return;
                _vector.y = value;
            }
        }

        [Inlet]
        public float z {
            set {
                if (!enabled) return;
                _vector.z = value;
            }
        }

        [SerializeField, Outlet]
        Vector3Event _vectorEvent = new Vector3Event();

        #endregion

        Vector3 _prevVector;

        #region Monobehaviour

        void Update()
        {
            if (_vector != _prevVector)
            {
                _vectorEvent.Invoke(_vector); 
                _prevVector = _vector;
            }
        }

        #endregion
    }
}
