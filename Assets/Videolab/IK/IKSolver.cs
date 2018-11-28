using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Videolab 
{
    public class IKSolver : MonoBehaviour 
    {
        #region Public

        public IKJoint rootJoint;
        public IKJoint endEffector;

        public Transform target;

        public float tolerance = 0;
        public int maxIterations = 4;

        public float blendWeight = 1;

        #endregion

        #region Private

        IKJoint[] _joints;
        float _chainLen;
        Vector3[] _solution;

        #endregion

        void Start()
        {
            if (!endEffector.transform.IsChildOf(rootJoint.transform))
            {
                Debug.Log("[IK.Solver] End effector is not a child of root joint.");

                return;
            }

            List<IKJoint> joints = new List<IKJoint>();

            IKJoint joint = rootJoint;
            while (true)
            {
                joints.Add(joint);
                _chainLen += joint.boneLength;

                if (joint == endEffector)
                    break;

                joint = joint.GetChildJoint();
            }
            _joints = joints.ToArray();

            _solution = new Vector3[_joints.Length];
        }

        void LateUpdate()
        {
            if (_joints == null)
                return;

            Vector3 startPos = rootJoint.transform.position;
            Vector3 targetPos = target.position;

            // initialize solution
            for (int i = 0; i < _solution.Length; i++)
                _solution[i] = _joints[i].transform.position;

            // target unreachable
            if (_chainLen < Vector3.Distance(targetPos, rootJoint.transform.position))
                targetPos = rootJoint.transform.position + _chainLen * Vector3.Normalize(targetPos - rootJoint.transform.position);

            // FABRIK iterations
            for (int k = 0; k < maxIterations; k++)
            {
                // inward
                _solution[_solution.Length - 1] = targetPos;
                for (int i = _solution.Length - 1; i > 0; i--)
                {
                    Vector3 v = Vector3.Normalize(_solution[i] - _solution[i - 1]);
                    _solution[i - 1] = _solution[i] - _joints[i - 1].boneLength * v;
                }

                // outward
                _solution[0] = startPos;
                for (int i = 1; i < _solution.Length; i++)
                {
                    Vector3 v = Vector3.Normalize(_solution[i] - _solution[i - 1]);
                    _solution[i] = _solution[i - 1] + _joints[i - 1].boneLength * v;
                }

                if (Vector3.Distance(_solution[_solution.Length - 1], targetPos) < (tolerance + 0.001))
                    break;
            }

            // update joints
            for (int i = 0; i < _solution.Length - 1; i++)
            {
                IKJoint joint = _joints[i];
                joint.transform.rotation = Quaternion.LookRotation(_solution[i + 1] - joint.transform.position) * Quaternion.Inverse(joint.refOrientation);
            }
        }
    }
}
