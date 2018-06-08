using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace IK
{
    public class Joint : MonoBehaviour 
    {
        public Joint[] GetJoints()
        {
            List<Joint> joints = new List<Joint>();

            Joint joint;

            foreach (Transform child in transform)
                if (joint = child.GetComponent<Joint>())
                    joints.Add(joint);

            return joints.ToArray();
        }
    }
}
