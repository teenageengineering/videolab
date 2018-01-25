// KinoBinary - Binary image effect for Unity
// https://github.com/keijiro/KinoBinary

using UnityEngine;
using UnityEditor;

namespace Kino
{
    [CanEditMultipleObjects]
    [CustomEditor(typeof(Binary))]
    public sealed class BinaryEditor : Editor
    {
        SerializedProperty _ditherType;
        SerializedProperty _ditherScale;
        SerializedProperty _color0;
        SerializedProperty _color1;
        SerializedProperty _opacity;

        static class Styles
        {
            public static readonly GUIContent color0 = new GUIContent("Color (dark)");
            public static readonly GUIContent color1 = new GUIContent("Color (light)");
        }

        void OnEnable()
        {
            _ditherType = serializedObject.FindProperty("_ditherType");
            _ditherScale = serializedObject.FindProperty("_ditherScale");
            _color0 = serializedObject.FindProperty("_color0");
            _color1 = serializedObject.FindProperty("_color1");
            _opacity = serializedObject.FindProperty("_opacity");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.PropertyField(_ditherType);
            EditorGUILayout.PropertyField(_ditherScale);
            EditorGUILayout.PropertyField(_color0, Styles.color0);
            EditorGUILayout.PropertyField(_color1, Styles.color1);
            EditorGUILayout.PropertyField(_opacity);

            serializedObject.ApplyModifiedProperties();
        }
    }
}
