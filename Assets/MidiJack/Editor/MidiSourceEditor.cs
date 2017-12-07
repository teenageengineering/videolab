using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace MidiJack
{
    [CustomEditor(typeof(MidiSource))]
    public class MidiSourceEditor : Editor {

        SerializedProperty _autoConnect;
        SerializedProperty _preferredName;
        SerializedProperty _midiMap;
        SerializedProperty _autoAssignMap;

        void OnEnable()
        {
            _autoConnect    = serializedObject.FindProperty("_autoConnect");
            _preferredName  = serializedObject.FindProperty("_preferredName");
            _midiMap        = serializedObject.FindProperty("_midiMap");
            _autoAssignMap  = serializedObject.FindProperty("_autoAssignMap");
        }

        public override void OnInspectorGUI()
        {
            MidiSource source = target as MidiSource;

            if (source.connectToAll)
            {
                EditorGUILayout.LabelField("Receives from all inputs.");
                return;
            }

            var sourceCount = MidiDriver.CountSources();

            List<uint> sourceIds = new List<uint>();
            List<string> sourceNames = new List<string>();

            sourceIds.Add(0);
            sourceNames.Add("No connection");

            for (var i = 0; i < sourceCount; i++)
            {
                var id = MidiDriver.GetSourceIdAtIndex(i);
                sourceIds.Add(id);
                sourceNames.Add(MidiDriver.GetSourceName(id));
            }

            int sourceIndex = sourceIds.FindIndex(x => x == source.endpointId);

            // Show missing endpoint.
            if (sourceIndex == -1)
            {
                sourceIds.Add(source.endpointId);
                sourceNames.Add(source.endpointName + " *");
                sourceIndex = sourceIds.Count - 1;
            }

            EditorGUI.BeginChangeCheck();
            sourceIndex = EditorGUILayout.Popup("Source", sourceIndex, sourceNames.ToArray());
            if (EditorGUI.EndChangeCheck())
            {
                if (sourceIds[sourceIndex] != source.endpointId)
                    source.endpointId = sourceIds[sourceIndex];
            }

            serializedObject.Update();

            EditorGUILayout.PropertyField(_autoConnect);

            if (_autoConnect.boolValue)
                EditorGUILayout.PropertyField(_preferredName);

            EditorGUILayout.Space();

            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(_midiMap);
            if (EditorGUI.EndChangeCheck())
            {
                _autoAssignMap.boolValue = (!_midiMap.objectReferenceValue);
            }

            EditorGUILayout.PropertyField(_autoAssignMap);

            serializedObject.ApplyModifiedProperties();
        }
    }
}
