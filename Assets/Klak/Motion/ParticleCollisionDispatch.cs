using UnityEngine;
using UnityEngine.Events;

namespace Klak.Motion
{
    [RequireComponent(typeof(Collider))]
    public class ParticleCollisionDispatch : MonoBehaviour
    {
        public UnityEvent ParticleCollisionEvent = new UnityEvent();

        #region Collider events

        void OnParticleCollision(GameObject other)
        {
            if (ParticleCollisionEvent != null)
                ParticleCollisionEvent.Invoke();
        }

        #endregion
    }
}