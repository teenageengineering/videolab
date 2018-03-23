using UnityEngine;
using UnityEditor;

namespace Klak.Wiring
{
    [CanEditMultipleObjects]
    [CustomEditor(typeof(ColliderInput))]
    public class ColliderInputEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            DrawPropertiesExcluding(serializedObject, new string[] {"m_Script"});
        }
    }
}