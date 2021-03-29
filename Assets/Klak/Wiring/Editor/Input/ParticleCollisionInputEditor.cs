using UnityEngine;
using UnityEditor;
using System;

namespace Klak.Wiring
{
    [CanEditMultipleObjects]
    [CustomEditor(typeof(ParticleCollisionInput))]
    public class ParticleCollisionInputEditor : Editor
    {
        SerializedProperty _collider;
        SerializedProperty _ps;
        SerializedProperty _particleCollisionEvent;
        SerializedProperty _particleCollisionPosition;
        SerializedProperty _particleCollisionRotation;
        SerializedProperty _particleCollisionVelocity;
        SerializedProperty _particleEnterTriggerEvent;
        SerializedProperty _particleExitTriggerEvent;
        SerializedProperty _particleInsideTriggerEvent;
        SerializedProperty _particleOutsideTriggerEvent;

        SerializedProperty _basedOn;

        void OnEnable()
        {
            _collider = serializedObject.FindProperty("_collider");
            _ps = serializedObject.FindProperty("_particleSystem");
            _basedOn = serializedObject.FindProperty("_basedOn");
            _particleCollisionEvent = serializedObject.FindProperty("_particleCollisionEvent");
            _particleCollisionPosition = serializedObject.FindProperty("_particleCollisionPosition");
            _particleCollisionRotation = serializedObject.FindProperty("_particleCollisionRotation");
            _particleCollisionVelocity = serializedObject.FindProperty("_particleCollisionVelocity");
            _particleEnterTriggerEvent = serializedObject.FindProperty("_particleEnterTriggerEvent");
            _particleExitTriggerEvent = serializedObject.FindProperty("_particleExitTriggerEvent");
            _particleInsideTriggerEvent = serializedObject.FindProperty("_particleInsideTriggerEvent");
            _particleOutsideTriggerEvent = serializedObject.FindProperty("_particleOutsideTriggerEvent");

        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            if (_basedOn.enumValueIndex == 1)
            {
                EditorGUILayout.PropertyField(_basedOn);
                EditorGUILayout.Space();
                EditorGUILayout.PropertyField(_ps);
                EditorGUILayout.Space();
                EditorGUILayout.PropertyField(_particleCollisionEvent);
                EditorGUILayout.PropertyField(_particleCollisionPosition);
                EditorGUILayout.PropertyField(_particleCollisionRotation);
                EditorGUILayout.PropertyField(_particleCollisionVelocity);
                EditorGUILayout.PropertyField(_particleEnterTriggerEvent);
                EditorGUILayout.PropertyField(_particleExitTriggerEvent);
                EditorGUILayout.PropertyField(_particleInsideTriggerEvent);
                EditorGUILayout.PropertyField(_particleOutsideTriggerEvent);

            }
            else
            {
                EditorGUILayout.PropertyField(_basedOn);
                EditorGUILayout.Space();
                EditorGUILayout.PropertyField(_collider);
                EditorGUILayout.Space();
                EditorGUILayout.PropertyField(_particleCollisionEvent);
                EditorGUILayout.PropertyField(_particleCollisionPosition);
                EditorGUILayout.PropertyField(_particleCollisionRotation);
                EditorGUILayout.PropertyField(_particleCollisionVelocity);
            }

            serializedObject.ApplyModifiedProperties();

        }
    }
}
