using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Klak.Motion
{
    [RequireComponent(typeof(Collider))]
    public class ParticleCollisionDispatch : MonoBehaviour
    {
        public UnityEvent ParticleCollisionEvent = new UnityEvent();

        private List<ParticleCollisionEvent> _collisionEvents = new List<ParticleCollisionEvent>();
        private GameObject _gameObject;
        public Vector3 _position;
        public Quaternion _rotation;
        public Vector3 _velocity;

        #region Collider events

        void OnParticleCollision(GameObject other)
        {
            _gameObject = gameObject;
            ParticlePhysicsExtensions.GetCollisionEvents(other.GetComponent<ParticleSystem>(), _gameObject, _collisionEvents);

            for (int i = 0; i < _collisionEvents.Count; i++)
            {
                _position = _collisionEvents[i].intersection;
                _rotation = Quaternion.LookRotation(_collisionEvents[i].normal);
                _velocity = _collisionEvents[i].velocity;
                if (ParticleCollisionEvent != null)
                    ParticleCollisionEvent.Invoke();
            }
        }

        #endregion
    }
}
