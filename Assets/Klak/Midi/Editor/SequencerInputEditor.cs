using UnityEngine;
using UnityEditor;
using Klak.Wiring;

namespace Klak.Midi
{
    [CanEditMultipleObjects]
    [CustomEditor(typeof(SequencerInput))]
    public class SequencerInputEditor : Editor 
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            DrawPropertiesExcluding(serializedObject, new string[] {"m_Script"});

            serializedObject.ApplyModifiedProperties();
        }
    }
}
