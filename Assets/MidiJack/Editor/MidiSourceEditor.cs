using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace MidiJack
{
    [CustomEditor(typeof(MidiSource))]
    public class MidiSourceEditor : Editor {

        public override void OnInspectorGUI()
        {
            MidiSource source = target as MidiSource;

            var sourceCount = MidiDriver.CountSources();

            List<uint> sourceIds = new List<uint>();
            List<string> sourceNames = new List<string>();

            sourceIds.Add(0);
            sourceNames.Add("All");

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
        }
    }
}
