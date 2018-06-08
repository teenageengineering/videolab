using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace IK 
{
	public class Solver : MonoBehaviour 
	{
        public Joint rootJoint;
        public Joint endEffector;

        public Transform target;

        public float tolerance = 0;
        public int maxIterations = 4;

        public float blendWeight = 1;

		void LateUpdate()
		{
            if (!endEffector.transform.IsChildOf(rootJoint.transform))
            {
                Debug.Log("[IK.Solver] End effector is not a child of root joint.");

                return;
            }

            List<Vector3> solution = new List<Vector3>();

            Vector3 startPos = rootJoint.transform.position;
            Vector3 targetPos = target.position;
            
            float chainLen = 0;
            List<float> ds = new List<float>();
            Joint joint;

            joint = rootJoint;
            while (true)
            {
                solution.Add(joint.transform.position);

                float d = Vector3.Magnitude(joint.transform.localPosition);
                ds.Add(d);
                chainLen += d;

                if (joint == endEffector)
                    break;
                
                joint = joint.GetJoints()[0];
            }

            // target unreachable
            if (chainLen < Vector3.Distance(targetPos, rootJoint.transform.position))
                targetPos = rootJoint.transform.position + chainLen * Vector3.Normalize(targetPos - rootJoint.transform.position);

            for (int k = 0; k < maxIterations; k++)
            {
                //inward
                solution[solution.Count - 1] = targetPos;
                for (int i = solution.Count - 1; i > 0; i--)
                {
                    Vector3 v = Vector3.Normalize(solution[i] - solution[i - 1]);
                    solution[i - 1] = solution[i] - ds[i] * v;
                }

                //outward
                solution[0] = startPos;
                for (int i = 1; i < solution.Count; i++)
                {
                    Vector3 v = Vector3.Normalize(solution[i] - solution[i - 1]);
                    solution[i] = solution[i - 1] + ds[i] * v;
                }

                if (Vector3.Distance(solution[solution.Count - 1], targetPos) < (tolerance + 0.001))
                    break;
            }

            joint = rootJoint;
            for (int i = 1; i < solution.Count; i++)
            {
                Joint nextJoint = joint.GetJoints()[0];
                joint.transform.rotation = Quaternion.LookRotation(solution[i] - joint.transform.position);
                nextJoint.transform.position = Vector3.Lerp(nextJoint.transform.position, solution[i], blendWeight);

                joint = nextJoint;
            }
        }
	}
}
