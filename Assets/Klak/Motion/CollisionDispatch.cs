using UnityEngine;
using UnityEngine.Events;

namespace Klak.Motion
{
    [RequireComponent(typeof(Collider))]
    public class CollisionDispatch : MonoBehaviour
    {
        public UnityEvent CollisionEnterEvent = new UnityEvent();
        public UnityEvent CollisionStayEvent = new UnityEvent();
        public UnityEvent CollisionExitEvent = new UnityEvent();

        public UnityEvent TriggerEnterEvent = new UnityEvent();
        public UnityEvent TriggerStayEvent = new UnityEvent();
        public UnityEvent TriggerExitEvent = new UnityEvent();

        #region Collider events

        void OnCollisionEnter(Collision collision)
        {
            if (CollisionEnterEvent != null)
                CollisionEnterEvent.Invoke();
        }

        void OnCollisionStay(Collision collision)
        {
            if (CollisionStayEvent != null)
                CollisionStayEvent.Invoke();
        }

        void OnCollisionExit(Collision collision)
        {
            if (CollisionExitEvent != null)
                CollisionExitEvent.Invoke();
        }

        void OnTriggerEnter(Collider collider)
        {
            if (TriggerEnterEvent != null)
                TriggerEnterEvent.Invoke();
        }

        void OnTriggerStay(Collider collider)
        {
            if (TriggerStayEvent != null)
                TriggerStayEvent.Invoke();
        }

        void OnTriggerExit(Collider collider)
        {
            if (TriggerExitEvent != null)
                TriggerExitEvent.Invoke();
        }

        #endregion
    }
}