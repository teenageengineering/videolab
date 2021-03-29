using UnityEngine;
using Klak.Motion;

namespace Klak.Wiring
{
    [AddComponentMenu("Klak/Wiring/Input/Component/Particle Collision Input")]
    public class ParticleCollisionInput : NodeBase
    {
        #region Editable properties

        [SerializeField]
        Collider _collider;

        [SerializeField]
        ParticleSystem _particleSystem;

        [SerializeField]
        Mode _basedOn = Mode.Collider;

        #endregion

        #region Node I/O

        [SerializeField, Outlet]
        VoidEvent _particleCollisionEvent = new VoidEvent();

        [SerializeField, Outlet]
        Vector3Event _particleCollisionPosition = new Vector3Event();

        [SerializeField, Outlet]
        QuaternionEvent _particleCollisionRotation = new QuaternionEvent();

        [SerializeField, Outlet]
        Vector3Event _particleCollisionVelocity = new Vector3Event();

        [SerializeField, Outlet]
        VoidEvent _particleEnterTriggerEvent = new VoidEvent();

        [SerializeField, Outlet]
        VoidEvent _particleExitTriggerEvent = new VoidEvent();

        [SerializeField, Outlet]
        VoidEvent _particleInsideTriggerEvent = new VoidEvent();

        [SerializeField, Outlet]
        VoidEvent _particleOutsideTriggerEvent = new VoidEvent();


        #endregion

        #region Collision Dispatcher events

        void OnParticleCollision()

        {
            _particleCollisionEvent.Invoke();
            if (_basedOn == Mode.Collider)
            {
                _particleCollisionPosition.Invoke(_dispatch._position);
                _particleCollisionRotation.Invoke(_dispatch._rotation);
                _particleCollisionVelocity.Invoke(_dispatch._velocity);
            }
            else
            {
                _particleCollisionPosition.Invoke(_psdispatch._position);
                _particleCollisionRotation.Invoke(_psdispatch._rotation);
                _particleCollisionVelocity.Invoke(_psdispatch._velocity);
            }
           
        }

        void OnParticleEnterTrigger()

        {
            _particleEnterTriggerEvent.Invoke();
        }

        void OnParticleExitTrigger()

        {
            _particleExitTriggerEvent.Invoke();
        }

        void OnParticleInsideTrigger()

        {
            _particleInsideTriggerEvent.Invoke();
        }

        void OnParticleOutsideTrigger()

        {
            _particleOutsideTriggerEvent.Invoke();
        }


        #endregion

        #region Private members

        enum Mode { Collider, ParticleSystem };

        bool _switcher;

        ParticleCollisionDispatch _dispatch;
        ParticleCollisionDispatch dispatch {
            get {
                if (_dispatch == null)
                    _dispatch = _collider.gameObject.AddComponent<ParticleCollisionDispatch>();

                return _dispatch;
            }
        }

        PSParticleCollisionDispatch _psdispatch;
        PSParticleCollisionDispatch psdispatch {
            get
            {
                if (_psdispatch == null)
                    _psdispatch = _particleSystem.gameObject.AddComponent<PSParticleCollisionDispatch>();

                return _psdispatch;
            }
        }

        #endregion

        #region MonoBehaviour functions

        void OnEnable()
        {
            if (_collider != null && _basedOn == Mode.Collider)
            {
                dispatch.ParticleCollisionEvent.AddListener(OnParticleCollision);
                _switcher = true;
            }
 

            if (_particleSystem != null && _basedOn == Mode.ParticleSystem)
            {   psdispatch.ParticleCollisionEvent.AddListener(OnParticleCollision);
                psdispatch.ParticleEnterTriggerEvent.AddListener(OnParticleEnterTrigger);
                psdispatch.ParticleExitTriggerEvent.AddListener(OnParticleExitTrigger);
                psdispatch.ParticleInsideTriggerEvent.AddListener(OnParticleInsideTrigger);
                psdispatch.ParticleOutsideTriggerEvent.AddListener(OnParticleOutsideTrigger);
                _switcher = false;
            }
        }

        void OnDisable()
        {
            if (_collider != null)
                dispatch.ParticleCollisionEvent.RemoveListener(OnParticleCollision);


            if (_particleSystem != null)
            {
                psdispatch.ParticleCollisionEvent.RemoveListener(OnParticleCollision);
                psdispatch.ParticleEnterTriggerEvent.RemoveListener(OnParticleEnterTrigger);
                psdispatch.ParticleExitTriggerEvent.RemoveListener(OnParticleExitTrigger);
                psdispatch.ParticleInsideTriggerEvent.RemoveListener(OnParticleInsideTrigger);
                psdispatch.ParticleOutsideTriggerEvent.RemoveListener(OnParticleOutsideTrigger);
            }
        }

        void Update()
        {

            if (_basedOn == Mode.ParticleSystem && _particleSystem != null && _switcher)
            {
                psdispatch.ParticleCollisionEvent.AddListener(OnParticleCollision);
                psdispatch.ParticleEnterTriggerEvent.AddListener(OnParticleEnterTrigger);
                psdispatch.ParticleExitTriggerEvent.AddListener(OnParticleExitTrigger);
                psdispatch.ParticleInsideTriggerEvent.AddListener(OnParticleInsideTrigger);
                psdispatch.ParticleOutsideTriggerEvent.AddListener(OnParticleOutsideTrigger);
                dispatch.ParticleCollisionEvent.RemoveListener(OnParticleCollision);
                _switcher = false;
            }

            if (_basedOn == Mode.Collider && _collider != null && !_switcher)
            {
                psdispatch.ParticleCollisionEvent.RemoveListener(OnParticleCollision);
                psdispatch.ParticleEnterTriggerEvent.RemoveListener(OnParticleEnterTrigger);
                psdispatch.ParticleExitTriggerEvent.RemoveListener(OnParticleExitTrigger);
                psdispatch.ParticleInsideTriggerEvent.RemoveListener(OnParticleInsideTrigger);
                psdispatch.ParticleOutsideTriggerEvent.RemoveListener(OnParticleOutsideTrigger);
                dispatch.ParticleCollisionEvent.AddListener(OnParticleCollision);
                _switcher = true;
            }
        }

        #endregion
    }
}
