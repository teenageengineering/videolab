using UnityEngine;
using UnityEditor;

namespace Klak.Wiring
{
    [CanEditMultipleObjects]
    [CustomEditor(typeof(VideoPlayerOut))]
    public class VideoPlayerOutEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            DrawPropertiesExcluding(serializedObject, new string[] {"m_Script"});

            serializedObject.ApplyModifiedProperties();
        }
    }
}
