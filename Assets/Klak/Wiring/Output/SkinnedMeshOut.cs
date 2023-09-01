using UnityEngine;

namespace Klak.Wiring
{
    [AddComponentMenu("Klak/Wiring/Output/Component/Skinned Mesh Out")]
    public class SkinnedMeshOut : NodeBase
    {
        #region Editable properties

        [SerializeField]
        SkinnedMeshRenderer _skinnedMesh;

        [SerializeField]
        int _blendShapeIndex;

        #endregion

        int blendShapeCount;

        #region Node I/O

        [Inlet]
        public float weight {
            set {   
                if (!enabled || _skinnedMesh == null) return;
                float w = Mathf.Clamp(value * 100, 0, 100);
                _skinnedMesh.SetBlendShapeWeight(_blendShapeIndex, w);
            }
        }

        #endregion
    }
}