using UnityEngine;
using Klak.Motion;

namespace Klak.Wiring
{
    [AddComponentMenu("Klak/Wiring/Input/Component/Collider Input")]
    public class ColliderInput : NodeBase
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

        void OnCollisionEnter()
        {
            _collisionEnterEvent.Invoke();
        }

        void OnCollisionStay()
        {
            _collisionStayEvent.Invoke();
        }

        void OnCollisionExit()
        {
            _collisionExitEvent.Invoke();
        }

        void OnTriggerEnter()
        {
            _triggerEnterEvent.Invoke();
        }

        void OnTriggerStay()
        {
            _triggerStayEvent.Invoke();
        }

        void OnTriggerExit()
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
            if (_collider == null)
                return;

            dispatch.CollisionEnterEvent.AddListener(OnCollisionEnter);
            dispatch.CollisionStayEvent.AddListener(OnCollisionStay);
            dispatch.CollisionExitEvent.AddListener(OnCollisionExit);
            dispatch.TriggerEnterEvent.AddListener(OnTriggerEnter);
            dispatch.TriggerStayEvent.AddListener(OnTriggerStay);
            dispatch.TriggerExitEvent.AddListener(OnTriggerExit);
        }

        void OnDisable()
        {
            if (_collider == null)
                return;

            dispatch.CollisionEnterEvent.RemoveListener(OnCollisionEnter);
            dispatch.CollisionStayEvent.RemoveListener(OnCollisionStay);
            dispatch.CollisionExitEvent.RemoveListener(OnCollisionExit);
            dispatch.TriggerEnterEvent.RemoveListener(OnTriggerEnter);
            dispatch.TriggerStayEvent.RemoveListener(OnTriggerStay);
            dispatch.TriggerExitEvent.RemoveListener(OnTriggerExit);
        }

        #endregion
    }
}