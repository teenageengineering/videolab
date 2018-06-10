using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace IK
{
    public class Joint : MonoBehaviour 
    {
        float _boneLength;
        public float boneLength {
            get { 
                return _boneLength;
            }
        }

        Quaternion _refOrientation;
        public Quaternion refOrientation {
            get { 
                return _refOrientation;
            }
        }
        
        public Joint GetChildJoint()
        {
            Joint joint;
            foreach (Transform child in transform)
                if (joint = child.GetComponent<Joint>())
                    return joint;

            return null;
        }

        void Awake()
        {
            Joint childJoint = GetChildJoint();
            if (childJoint)
            {
                _boneLength = Vector3.Magnitude(childJoint.transform.localPosition);
                _refOrientation = Quaternion.LookRotation(transform.position - childJoint.transform.position);
            }
        }
    }
}
