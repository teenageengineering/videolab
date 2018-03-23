using UnityEngine;
using UnityEditor;

namespace Klak.Wiring
{
    [CanEditMultipleObjects]
    [CustomEditor(typeof(RigidbodyOut))]
    public class RigidbodyOutEditor : Editor
    {
        SerializedProperty _rigidbody;
        SerializedProperty _forceMode;
        SerializedProperty _useLocalPOI;

        void OnEnable()
        {
            _rigidbody = serializedObject.FindProperty("_rigidbody");
            _forceMode = serializedObject.FindProperty("_forceMode");
            _useLocalPOI = serializedObject.FindProperty("_useLocalPOI");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.PropertyField(_rigidbody);
            EditorGUILayout.PropertyField(_forceMode);
            EditorGUILayout.PropertyField(_useLocalPOI);

            serializedObject.ApplyModifiedProperties();
        }
    }
}