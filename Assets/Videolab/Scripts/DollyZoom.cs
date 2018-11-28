using UnityEngine;
using System.Collections;

namespace Videolab
{
    [ExecuteInEditMode]
    [RequireComponent(typeof(Camera))]
    [AddComponentMenu("Image Effects/Dolly Zoom")]
    public class DollyZoom : MonoBehaviour
    {
        public Transform target;

        private Camera _camera;
        private float _initHeightAtDist;
        private bool _dzEnabled;

        float FrustumHeightAtDistance(float distance)
        {
            return 2.0f * distance * Mathf.Tan(_camera.fieldOfView * 0.5f * Mathf.Deg2Rad);
        }

        float FOVForHeightAndDistance(float height, float distance)
        {
            return 2.0f * Mathf.Atan(height * 0.5f / distance) * Mathf.Rad2Deg;
        }

        void StartDZ()
        {
            var distance = Vector3.Distance(transform.position, target.position);
            _initHeightAtDist = FrustumHeightAtDistance(distance);
            _dzEnabled = true;
        }

        void StopDZ()
        {
            _dzEnabled = false;
        }

        void Start()
        {
            _camera = GetComponent<Camera>();
            StartDZ();
        }

        void Update()
        {
            if (_dzEnabled) {
                var currDistance = Vector3.Distance(transform.position, target.position);
                _camera.fieldOfView = FOVForHeightAndDistance(_initHeightAtDist, currDistance);
            }
        }
    }
}