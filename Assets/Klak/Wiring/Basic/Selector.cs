using UnityEngine;

namespace Klak.Wiring
{
    [AddComponentMenu("Klak/Wiring/Output/Selector")]
    public class Selector : NodeBase
    {
        #region Editable properties

        [SerializeField]
        GameObject[] _objectArray;

        [SerializeField]
        bool _selectFirstOnStart = false;

        #endregion

        #region Node I/O

        [Inlet]
        public float parameter {
            set {
                if (!enabled) return;

                int len = _objectArray.Length;
                int index = (int)value % len;

                for (int i = 0; i < len; i++)
                    _objectArray[i].SetActive(i == index);
            }
        }

        #endregion

        #region MonoBehaviour

        void Start()
        {
            if (_selectFirstOnStart) parameter = 0;
        }

        #endregion
    }
}
