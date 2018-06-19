using UnityEngine;
using UnityEditor;

namespace Klak.Wiring
{
    [CustomEditor(typeof(StringOut))]
    public class StringOutEditor : GenericOutEditor<string>
    {
        SerializedProperty _format;

        public override void OnEnable()
        {
            base.OnEnable();

            _format = serializedObject.FindProperty("_format");
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            EditorGUILayout.Space();

            EditorGUILayout.PropertyField(_format);

            serializedObject.ApplyModifiedProperties();
        }
    }
}
