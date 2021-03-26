using UnityEngine;
using UnityEditor;

namespace Klak.Motion
{
    [CanEditMultipleObjects]
    [CustomEditor(typeof(ParticleCollisionDispatch))]
    public class ParticleCollisionDispatchEditor : Editor
    {
        SerializedProperty _particleCollisionEvent;

        private void OnEnable()
        {
            _particleCollisionEvent = serializedObject.FindProperty("ParticleCollisionEvent");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.PropertyField(_particleCollisionEvent);

            serializedObject.ApplyModifiedProperties();
        }
    }
}
