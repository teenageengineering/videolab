using UnityEngine;
using UnityEditor;

namespace Klak.Motion
{
    [CanEditMultipleObjects]
    [CustomEditor(typeof(PSParticleCollisionDispatch))]
    public class PSParticleCollisionDispatchEditor : Editor
    {
        SerializedProperty _particleCollisionEvent;
        SerializedProperty _particleEnterTriggerEvent;
        SerializedProperty _particleExitTriggerEvent;
        SerializedProperty _particleInsideTriggerEvent;
        SerializedProperty _particleOutsideTriggerEvent;

        private void OnEnable()
        {
            _particleCollisionEvent = serializedObject.FindProperty("ParticleCollisionEvent");
            _particleEnterTriggerEvent = serializedObject.FindProperty("ParticleEnterTriggerEvent");
            _particleExitTriggerEvent = serializedObject.FindProperty("ParticleExitTriggerEvent");
            _particleInsideTriggerEvent = serializedObject.FindProperty("ParticleInsideTriggerEvent");
            _particleOutsideTriggerEvent = serializedObject.FindProperty("ParticleOutsideTriggerEvent");
        }


        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.PropertyField(_particleCollisionEvent);
            EditorGUILayout.PropertyField(_particleEnterTriggerEvent);
            EditorGUILayout.PropertyField(_particleExitTriggerEvent);
            EditorGUILayout.PropertyField(_particleInsideTriggerEvent);
            EditorGUILayout.PropertyField(_particleOutsideTriggerEvent);

            serializedObject.ApplyModifiedProperties();
        }
    }
}
