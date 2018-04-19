using UnityEngine;
using UnityEditor;
using Klak.Wiring;

namespace Klak.Midi
{
    [CanEditMultipleObjects]
    [CustomEditor(typeof(SequencerOut))]
    public class SequencerOutEditor : Editor 
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            DrawPropertiesExcluding(serializedObject, new string[] {"m_Script"});

            serializedObject.ApplyModifiedProperties();
        }
    }
}