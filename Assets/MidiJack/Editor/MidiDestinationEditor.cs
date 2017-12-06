using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace MidiJack
{
    [CustomEditor(typeof(MidiDestination))]
    public class MidiDestinationEditor : Editor {

        SerializedProperty _midiMap;
        SerializedProperty _autoAssignMap;

        void OnEnable()
        {
            _midiMap = serializedObject.FindProperty("_midiMap");
            _autoAssignMap = serializedObject.FindProperty("_autoAssignMap");
        }

        public override void OnInspectorGUI()
        {
            MidiDestination destination = target as MidiDestination;

            var destinationCount = MidiDriver.CountDestinations();

            List<uint> destinationIds = new List<uint>();
            List<string> destinationNames = new List<string>();

            destinationIds.Add(0);
            destinationNames.Add("All");

            for (var i = 0; i < destinationCount; i++)
            {
                var id = MidiDriver.GetDestinationIdAtIndex(i);
                destinationIds.Add(id);
                destinationNames.Add(MidiDriver.GetDestinationName(id));
            }

            int destinationIndex = destinationIds.FindIndex(x => x == destination.endpointId);

            // Show missing endpoint.
            if (destinationIndex == -1)
            {
                destinationIds.Add(destination.endpointId);
                destinationNames.Add(destination.endpointName + " *");
                destinationIndex = destinationIds.Count - 1;
            }

            EditorGUI.BeginChangeCheck();
            destinationIndex = EditorGUILayout.Popup("Destination", destinationIndex, destinationNames.ToArray());
            if (EditorGUI.EndChangeCheck())
            {
                if (destinationIds[destinationIndex] != destination.endpointId)
                    destination.endpointId = destinationIds[destinationIndex];
            }

            EditorGUILayout.Space();

            serializedObject.Update();

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
