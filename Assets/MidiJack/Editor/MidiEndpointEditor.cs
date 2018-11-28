using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace MidiJack
{
    [CustomEditor(typeof(MidiEndpoint), true)]
    public class MidiEndpointEditor : Editor 
    {
        #region Menu

        [MenuItem("GameObject/MIDI Jack/MIDI Source", false, 10)]
        static void CreateMIDISource()
        {
            GameObject go = new GameObject("MIDI Source");
            go.AddComponent<MidiSource>();
            Selection.activeGameObject = go;
        }

        [MenuItem("GameObject/MIDI Jack/MIDI Destination", false, 10)]
        static void CreateMIDIDestination()
        {
            GameObject go = new GameObject("MIDI Destination");
            go.AddComponent<MidiDestination>();
            Selection.activeGameObject = go;
        }

        #endregion

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
            MidiEndpoint endpoint = target as MidiEndpoint;

            if (endpoint.endpointId == uint.MaxValue)
            {
                EditorGUILayout.LabelField("Connect to all.");
                return;
            }

            int endpointCount = endpoint.CountEndpoints();

            List<uint> endpointIds = new List<uint>();
            List<string> endpointNames = new List<string>();

            endpointIds.Add(0);
            endpointNames.Add("No connection");

            for (var i = 0; i < endpointCount; i++)
            {
                uint id = endpoint.GetEndpointIdAtIndex(i);
                endpointIds.Add(id);
                endpointNames.Add(endpoint.GetEndpointName(id));
            }

            int endpointIndex = endpointIds.FindIndex(x => x == endpoint.endpointId);

            // Show missing endpoint.
            if (endpointIndex == -1)
            {
                endpointIds.Add(endpoint.endpointId);
                endpointNames.Add(endpoint.endpointName + " (offline)");
                endpointIndex = endpointIds.Count - 1;
            }

            EditorGUI.BeginChangeCheck();
            endpointIndex = EditorGUILayout.Popup("Endpoint", endpointIndex, endpointNames.ToArray());
            if (EditorGUI.EndChangeCheck())
                endpoint.endpointId = endpointIds[endpointIndex];

            serializedObject.Update();

            EditorGUILayout.PropertyField(_autoConnect);

            if (_autoConnect.boolValue)
                EditorGUILayout.PropertyField(_preferredName);

            EditorGUILayout.Space();

            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(_midiMap);
            if (EditorGUI.EndChangeCheck())
                _autoAssignMap.boolValue = (!_midiMap.objectReferenceValue);

            EditorGUILayout.PropertyField(_autoAssignMap);

            serializedObject.ApplyModifiedProperties();
        }
    }
}
