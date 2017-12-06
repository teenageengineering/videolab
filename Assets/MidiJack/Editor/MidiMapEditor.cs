using System;
using UnityEngine;
using UnityEditor;
using System.IO;

namespace MidiJack
{
    [CustomEditor(typeof(MidiMap))]
    public class MidiMapEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            EditorGUILayout.Space();

            MidiMap map = (MidiMap)target;
            if(GUILayout.Button("Export JSON"))
            {
                string path = EditorUtility.SaveFilePanel("Export JSON", Application.persistentDataPath, map.name, "json");
                if (path != "")
                {
                    string jsonString = JsonUtility.ToJson(map, true);
                    File.WriteAllText(path, jsonString);

                    AssetDatabase.Refresh();
                }
            }
        }
    }
}