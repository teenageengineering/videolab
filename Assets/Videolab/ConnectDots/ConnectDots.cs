using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace VideoLab
{
    public class ConnectDots : MonoBehaviour {

        public List<Transform> transforms;
        public Material material;
        public float lineWidth;

        LineRenderer _lineRenderer;

        void Start()
        {
            _lineRenderer = gameObject.AddComponent<LineRenderer>();
        }

        void Update() 
        {
            var points = new Vector3[transforms.Count];
            for(int i = 0; i < transforms.Count; i++) {
                Transform t = transforms[i];
                points[i] = t.position;
            }

            _lineRenderer.positionCount = transforms.Count;
            _lineRenderer.SetPositions(points);

            _lineRenderer.material = material;
            _lineRenderer.startWidth = _lineRenderer.startWidth = lineWidth;
        }
    }
}
