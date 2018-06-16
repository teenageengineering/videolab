using UnityEngine;
using UnityEditor;

namespace Klak.Midi
{
    [CanEditMultipleObjects]
    [CustomEditor(typeof(VideolabInput))]
    public class VideolabInputEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            DrawPropertiesExcluding(serializedObject, new string[] {"m_Script"});

            if (EditorApplication.isPlaying && !serializedObject.isEditingMultipleObjects)
            {
                VideolabInput instance = (VideolabInput)target;

                instance.activeTrack = EditorGUILayout.FloatField("Debug Active Track", instance.activeTrack);
                instance.activePattern = EditorGUILayout.FloatField("Debug Active Pattern", instance.activePattern);
                instance.activeProject = EditorGUILayout.FloatField("Debug Active Bank", instance.activeProject);
                instance.masterVolume = EditorGUILayout.Slider("Debug Master Volume", instance.masterVolume, 0, 1);
                instance.batteryLevel = EditorGUILayout.Slider("Debug Battery Level", instance.batteryLevel, 0, 1);
                instance.tempo = EditorGUILayout.IntSlider("Debug Tempo", (int)instance.tempo, 40, 200);

                EditorUtility.SetDirty(target); // request repaint
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
}
