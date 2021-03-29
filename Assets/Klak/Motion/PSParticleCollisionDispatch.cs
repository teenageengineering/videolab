using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Klak.Motion
{
    [RequireComponent(typeof(ParticleSystem))]
    public class PSParticleCollisionDispatch : MonoBehaviour
    {
        public UnityEvent ParticleCollisionEvent = new UnityEvent();
        public UnityEvent ParticleEnterTriggerEvent = new UnityEvent();
        public UnityEvent ParticleExitTriggerEvent = new UnityEvent();
        public UnityEvent ParticleInsideTriggerEvent = new UnityEvent();
        public UnityEvent ParticleOutsideTriggerEvent = new UnityEvent();

        private List<ParticleCollisionEvent> _collisionEvents = new List<ParticleCollisionEvent>();
        private GameObject _gameObject;
        public Vector3 _position;
        public Quaternion _rotation;
        public Vector3 _velocity;

        #region Particle System events

        void OnParticleCollision(GameObject other)
        {
            _gameObject = gameObject;
            ParticlePhysicsExtensions.GetCollisionEvents(_gameObject.GetComponent<ParticleSystem>(), other, _collisionEvents);

            for (int i = 0; i < _collisionEvents.Count; i++)
            {
                _position = _collisionEvents[i].intersection;
                _rotation = Quaternion.LookRotation(_collisionEvents[i].normal);
                _velocity = _collisionEvents[i].velocity;
                if (ParticleCollisionEvent != null)
                    ParticleCollisionEvent.Invoke();
            }
        }

        void OnParticleTrigger()
        {
            ParticleSystem ps = GetComponent<ParticleSystem>();

            List<ParticleSystem.Particle> enter = new List<ParticleSystem.Particle>();
            List<ParticleSystem.Particle> exit = new List<ParticleSystem.Particle>();
            List<ParticleSystem.Particle> inside = new List<ParticleSystem.Particle>();
            List<ParticleSystem.Particle> outside = new List<ParticleSystem.Particle>();


            int numEnter = ps.GetTriggerParticles(ParticleSystemTriggerEventType.Enter, enter);
            int numExit = ps.GetTriggerParticles(ParticleSystemTriggerEventType.Exit, exit);
            int numInside = ps.GetTriggerParticles(ParticleSystemTriggerEventType.Inside, inside);
            int numOutside = ps.GetTriggerParticles(ParticleSystemTriggerEventType.Outside, outside);

            if (numEnter > 0 && ParticleEnterTriggerEvent != null)
                ParticleEnterTriggerEvent.Invoke();

            if (numExit > 0 && ParticleExitTriggerEvent != null)
                ParticleExitTriggerEvent.Invoke();

            if (numInside > 0 && ParticleInsideTriggerEvent != null)
                ParticleInsideTriggerEvent.Invoke();

            if (numOutside > 0 && ParticleOutsideTriggerEvent != null)
                ParticleOutsideTriggerEvent.Invoke();
        }

        #endregion
    }
}
