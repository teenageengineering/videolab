using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Videolab
{
    public class IKJoint : MonoBehaviour 
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
        
        public IKJoint GetChildJoint()
        {
            IKJoint joint;
            foreach (Transform child in transform)
                if (joint = child.GetComponent<IKJoint>())
                    return joint;

            return null;
        }

        void Awake()
        {
            IKJoint childJoint = GetChildJoint();
            if (childJoint)
            {
                _boneLength = Vector3.Magnitude(childJoint.transform.localPosition);
                _refOrientation = Quaternion.LookRotation(childJoint.transform.localPosition);
            }
        }
    }
}
