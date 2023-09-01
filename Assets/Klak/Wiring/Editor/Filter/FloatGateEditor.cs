// Klak - Creative coding library for Unity
// https://github.com/keijiro/Klak

using UnityEngine;
using UnityEditor;

namespace Klak.Wiring
{
    [CanEditMultipleObjects]
    [CustomEditor(typeof(FloatGate))]
    public class FloatGateEditor : Editor
    {
        SerializedProperty _state;
        SerializedProperty _outputEvent;

        static GUIContent _textInitialState = new GUIContent("Opened");

        void OnEnable()
        {
            _state = serializedObject.FindProperty("_state");
            _outputEvent = serializedObject.FindProperty("_outputEvent");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.PropertyField(_state, _textInitialState);

            EditorGUILayout.Space();

            EditorGUILayout.PropertyField(_outputEvent);

            serializedObject.ApplyModifiedProperties();
        }
    }
}