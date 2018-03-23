using UnityEngine;
using UnityEditor;

namespace Klak.Motion
{
    [CanEditMultipleObjects]
    [CustomEditor(typeof(CollisionDispatch))]
    public class CollisionDispatchEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            DrawPropertiesExcluding(serializedObject, new string[] {"m_Script"});
        }
    }
}