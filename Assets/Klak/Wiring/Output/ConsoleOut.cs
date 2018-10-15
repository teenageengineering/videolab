using UnityEngine;
using System.Reflection;

namespace Klak.Wiring
{
    [AddComponentMenu("Klak/Wiring/Output/Generic/Console Out")]
    public class ConsoleOut : NodeBase
    {
        #region Node I/O

        [Inlet]
        public void Bang() {
            LogInEditor(name + " Bang!");
        }

        [Inlet]
        public float number {
            set {
                LogInEditor(name + " " + value);
            }
        }

        [Inlet]
        public Vector3 vector {
            set {
                LogInEditor(name + " Vector3" + value);
            }
        }

        [Inlet]
        public Quaternion rotation {
            set {
                LogInEditor(name + " Quaternion" + value);
            }
        }

        [Inlet]
        public Color color {
            set {
                LogInEditor(name + " " + value);
            }
        }

        #endregion

        void LogInEditor(string msg)
        {
            #if UNITY_EDITOR
            Debug.Log(msg);
            #endif
        }
    }
}
