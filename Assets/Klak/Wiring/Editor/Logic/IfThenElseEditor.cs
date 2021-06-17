using UnityEngine;
using UnityEditor;
using System;

namespace Klak.Wiring
{
    [CanEditMultipleObjects]
    [CustomEditor(typeof(IfThenElse))]
    public class IfThenElseEditor : Editor
    {
        SerializedProperty _continuousInput;
        SerializedProperty _condition1;
        SerializedProperty _useGate;
        SerializedProperty _gate;
        SerializedProperty _condition2;
        SerializedProperty _thenEvent;
        SerializedProperty _elseEvent;

        void OnEnable()
        {
            _continuousInput = serializedObject.FindProperty("_continuousInput");
            _condition1 = serializedObject.FindProperty("_condition1");
            _useGate = serializedObject.FindProperty("_useGate");
            _gate = serializedObject.FindProperty("_gate");
            _condition2 = serializedObject.FindProperty("_condition2");
            _thenEvent = serializedObject.FindProperty("_thenEvent");
            _elseEvent = serializedObject.FindProperty("_elseEvent");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            if (_useGate.boolValue)
            {
                EditorGUILayout.PropertyField(_continuousInput);
                EditorGUILayout.Space();
                EditorGUILayout.PropertyField(_condition1);
                EditorGUILayout.PropertyField(_gate);
                EditorGUILayout.PropertyField(_condition2);
                EditorGUILayout.Space();
                EditorGUILayout.PropertyField(_useGate);
            }
            
            else
            {
                EditorGUILayout.PropertyField(_continuousInput);
                EditorGUILayout.Space();
                EditorGUILayout.PropertyField(_condition1);
                EditorGUILayout.Space();
                EditorGUILayout.PropertyField(_useGate);
            }

            EditorGUILayout.Space();
            EditorGUILayout.PropertyField(_thenEvent);
            EditorGUILayout.PropertyField(_elseEvent);
            
            serializedObject.ApplyModifiedProperties();
        }
    }
}
