using UnityEngine;

namespace Klak.Wiring
{
    [AddComponentMenu("Klak/Wiring/Output/Component/Rigidbody Out")]
    public class RigidbodyOut : NodeBase
    {
        #region Editable properties

        [SerializeField]
        Rigidbody _rigidbody;

        [SerializeField]
        ForceMode _forceMode = ForceMode.Force;

        [SerializeField]
        bool _useLocalPOI;

        #endregion

        Vector3 _force;
        Vector3 _position;

        #region Node I/O

        [Inlet]
        public Vector3 force {
            set {
                _force = value;
            }
        }

        [Inlet]
        public Vector3 pointOfImpact {
            set {
                _position = value;
            }
        }

        [Inlet]
        public void Bang()
        {
            Vector3 position = (_useLocalPOI) ? _rigidbody.centerOfMass + _position : _position;
            _rigidbody.AddForceAtPosition(_force, position, _forceMode);
        }

        #endregion
    }
}