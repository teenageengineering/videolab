using UnityEngine;
using Klak.Math;

namespace Klak.Wiring
{
    [AddComponentMenu("Klak/Wiring/Input/Gyro Input")]
    public class GyroInput : NodeBase
    {
        #region Node I/O

        [SerializeField, Outlet]
        QuaternionEvent _attitudeEvent = new QuaternionEvent();

        [SerializeField, Outlet]
        Vector3Event _accelerationEvent = new Vector3Event();

        [SerializeField, Outlet]
        Vector3Event _gravityEvent = new Vector3Event();

        #endregion

        #region MonoBehaviour functions

        void Start()
        {
            Input.gyro.enabled = true;
        }

        void Update()
        {
            if (!Input.gyro.enabled)
                return;

            _attitudeEvent.Invoke(Input.gyro.attitude);

            _accelerationEvent.Invoke(Input.gyro.userAcceleration);

            _gravityEvent.Invoke(Input.gyro.gravity);
        }

        #endregion
    }
}
