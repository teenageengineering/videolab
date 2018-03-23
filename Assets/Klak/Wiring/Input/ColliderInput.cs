using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using Reaction;

namespace Klak.Wiring
{
    [AddComponentMenu("Klak/Wiring/Input/Dynamics/Collider Input")]
    public class ColliderIput : NodeBase
    {
        #region Editable properties

        [SerializeField]
        Collider _collider;

        #endregion

        #region Node I/O

        [SerializeField, Outlet]
        VoidEvent _collisionEnterEvent = new VoidEvent();

        [SerializeField, Outlet]
        VoidEvent _collisionStayEvent = new VoidEvent();

        [SerializeField, Outlet]
        VoidEvent _collisionExitEvent = new VoidEvent();

        [SerializeField, Outlet]
        VoidEvent _triggerEnterEvent = new VoidEvent();

        [SerializeField, Outlet]
        VoidEvent _triggerStayEvent = new VoidEvent();

        [SerializeField, Outlet]
        VoidEvent _triggerExitEvent = new VoidEvent();

        #endregion

        #region Collision Dispatcher events

        void OnCollisionEnter(Collision collision)
        {
            _collisionEnterEvent.Invoke();
        }

        void OnCollisionStay(Collision collision)
        {
            _collisionStayEvent.Invoke();
        }

        void OnCollisionExit(Collision collision)
        {
            _collisionExitEvent.Invoke();
        }

        void OnTriggerEnter(Collider collider)
        {
            _triggerEnterEvent.Invoke();
        }

        void OnTriggerStay(Collider collider)
        {
            _triggerStayEvent.Invoke();
        }

        void OnTriggerExit(Collider collider)
        {
            _triggerExitEvent.Invoke();
        }

        #endregion

        #region Private members

        CollisionDispatch _dispatch;
        CollisionDispatch dispatch {
            get {
                if (_dispatch == null)
                    _dispatch = _collider.gameObject.AddComponent<CollisionDispatch>();

                return _dispatch;
            }
        }

        #endregion

        #region MonoBehaviour functions

        void OnEnable()
        {
            dispatch.onCollisionEnter += OnCollisionEnter;
            dispatch.onCollisionStay += OnCollisionStay;
            dispatch.onCollisionExit += OnCollisionExit;
            dispatch.onTriggerEnter += OnTriggerEnter;
            dispatch.onTriggerStay += OnTriggerStay;
            dispatch.onTriggerExit += OnTriggerExit;
        }

        void OnDisable()
        {
            dispatch.onCollisionEnter -= OnCollisionEnter;
            dispatch.onCollisionStay -= OnCollisionStay;
            dispatch.onCollisionExit -= OnCollisionExit;
            dispatch.onTriggerEnter -= OnTriggerEnter;
            dispatch.onTriggerStay -= OnTriggerStay;
            dispatch.onTriggerExit -= OnTriggerExit;
        }

        #endregion
    }
}