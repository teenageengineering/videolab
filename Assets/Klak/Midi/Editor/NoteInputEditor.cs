//
// MidiKlak - MIDI extension for Klak
//
// Copyright (C) 2016 Keijiro Takahashi
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
//
using UnityEngine;
using UnityEditor;

namespace Klak.Midi
{
    [CanEditMultipleObjects]
    [CustomEditor(typeof(NoteInput))]
    public class NoteInputEditor : Editor
    {
        SerializedProperty _source;

        SerializedProperty _channel;
        SerializedProperty _noteFilter;
        SerializedProperty _noteName;
        SerializedProperty _lowestNote;
        SerializedProperty _highestNote;

        SerializedProperty _velocityCurve;
        SerializedProperty _offValue;
        SerializedProperty _onValue;
        SerializedProperty _interpolator;

        SerializedProperty _noteOnEvent;
        SerializedProperty _noteOnVelocityEvent;
        SerializedProperty _noteOffEvent;
        SerializedProperty _valueEvent;

        void OnEnable()
        {
            _source = serializedObject.FindProperty("_source");

            _channel = serializedObject.FindProperty("_channel");
            _noteFilter = serializedObject.FindProperty("_noteFilter");
            _noteName = serializedObject.FindProperty("_noteName");
            _lowestNote = serializedObject.FindProperty("_lowestNote");
            _highestNote = serializedObject.FindProperty("_highestNote");

            _velocityCurve = serializedObject.FindProperty("_velocityCurve");
            _offValue = serializedObject.FindProperty("_offValue");
            _onValue = serializedObject.FindProperty("_onValue");
            _interpolator = serializedObject.FindProperty("_interpolator");

            _noteOnEvent = serializedObject.FindProperty("_noteOnEvent");
            _noteOnVelocityEvent = serializedObject.FindProperty("_noteOnVelocityEvent");
            _noteOffEvent = serializedObject.FindProperty("_noteOffEvent");
            _valueEvent = serializedObject.FindProperty("_valueEvent");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.PropertyField(_source);

            EditorGUILayout.Space();

            EditorGUILayout.PropertyField(_channel);
            EditorGUILayout.PropertyField(_noteFilter);

            var noteFilter = (NoteInput.NoteFilter)_noteFilter.enumValueIndex;

            if (noteFilter == NoteInput.NoteFilter.NoteName)
                EditorGUILayout.PropertyField(_noteName);

            if (noteFilter == NoteInput.NoteFilter.NoteNumber) {
                EditorGUILayout.PropertyField(_lowestNote);
                EditorGUILayout.PropertyField(_highestNote);
            }

            EditorGUILayout.Space();

            EditorGUILayout.PropertyField(_velocityCurve);
            EditorGUILayout.PropertyField(_offValue);
            EditorGUILayout.PropertyField(_onValue);
            EditorGUILayout.PropertyField(_interpolator);

            EditorGUILayout.Space();

            EditorGUILayout.PropertyField(_noteOnEvent);
            EditorGUILayout.PropertyField(_noteOnVelocityEvent);
            EditorGUILayout.PropertyField(_noteOffEvent);
            EditorGUILayout.PropertyField(_valueEvent);

            if (EditorApplication.isPlaying &&
                !serializedObject.isEditingMultipleObjects)
            {
                var instance = (NoteInput)target;
                instance.debugInput =
                    EditorGUILayout.Toggle("Debug", instance.debugInput);
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
}
