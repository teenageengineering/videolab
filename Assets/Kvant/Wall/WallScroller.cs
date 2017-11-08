//
// Scroller script for Wall
//
using UnityEngine;
using UnityEngine.Serialization;

namespace Kvant
{
    [RequireComponent(typeof(Wall))]
    [AddComponentMenu("Kvant/Wall Scroller")]
    public class WallScroller : MonoBehaviour
    {
        [SerializeField, FormerlySerializedAs("yawAngle")]
        float _yawAngle;

        public float yawAngle {
            get { return _yawAngle; }
            set { _yawAngle = value; }
        }

        [SerializeField, FormerlySerializedAs("speed")]
        float _speed;

        public float speed {
            get { return _speed; }
            set { _speed = value; }
        }

        void Update()
        {
            var r = _yawAngle * Mathf.Deg2Rad;
            var dir = new Vector2(Mathf.Cos(r), Mathf.Sin(r));
            GetComponent<Wall>().offset += dir * _speed * Time.deltaTime;
        }
    }
}
