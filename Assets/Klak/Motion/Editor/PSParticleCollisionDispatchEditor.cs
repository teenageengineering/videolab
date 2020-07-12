using UnityEngine;
using UnityEditor;

namespace Klak.Motion
{
    [CanEditMultipleObjects]
    [CustomEditor(typeof(PSParticleCollisionDispatch))]
    public class PSParticleCollisionDispatchEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            DrawPropertiesExcluding(serializedObject, new string[] {"m_Script"});

            serializedObject.ApplyModifiedProperties();
        }
    }
}