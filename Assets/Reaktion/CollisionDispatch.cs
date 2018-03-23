using UnityEngine;
using UnityEngine.Events;

namespace Reaction
{
    [RequireComponent(typeof(Collider))]
    public class CollisionDispatch : MonoBehaviour
    {
        public delegate void CollisionDelegate(Collision collision);
        public event CollisionDelegate onCollisionEnter;
        public event CollisionDelegate onCollisionStay;
        public event CollisionDelegate onCollisionExit;

        public delegate void TriggerDelegate(Collider collider);
        public event TriggerDelegate onTriggerEnter;
        public event TriggerDelegate onTriggerStay;
        public event TriggerDelegate onTriggerExit;

        #region Collider events

        void OnCollisionEnter(Collision collision)
        {
            if (onCollisionEnter != null)
                onCollisionEnter(collision);
        }

        void OnCollisionStay(Collision collision)
        {
            if (onCollisionStay != null)
                onCollisionStay(collision);
        }

        void OnCollisionExit(Collision collision)
        {
            if (onCollisionExit != null)
                onCollisionExit(collision);
        }

        void OnTriggerEnter(Collider collider)
        {
            if (onTriggerEnter != null)
                onTriggerEnter(collider);
        }

        void OnTriggerStay(Collider collider)
        {
            if (onTriggerStay != null)
                onTriggerStay(collider);
        }

        void OnTriggerExit(Collider collider)
        {
            if (onTriggerExit != null)
                onTriggerExit(collider);
        }

        #endregion
    }
}