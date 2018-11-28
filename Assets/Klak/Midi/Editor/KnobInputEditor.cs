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
    [CustomEditor(typeof(KnobInput))]
    public class KnobInputEditor : Editor
    {
        SerializedProperty _source;
        SerializedProperty _channel;
        SerializedProperty _knobNumber;
        SerializedProperty _isRelative;
        SerializedProperty _responseCurve;
        SerializedProperty _interpolator;
        SerializedProperty _onEvent;
        SerializedProperty _offEvent;
        SerializedProperty _valueEvent;

        void OnEnable()
        {
            _source = serializedObject.FindProperty("_source");
            _channel = serializedObject.FindProperty("_channel");
            _knobNumber = serializedObject.FindProperty("_knobNumber");
            _isRelative = serializedObject.FindProperty("_isRelative");
            _responseCurve = serializedObject.FindProperty("_responseCurve");
            _interpolator = serializedObject.FindProperty("_interpolator");
            _onEvent = serializedObject.FindProperty("_onEvent");
            _offEvent = serializedObject.FindProperty("_offEvent");
            _valueEvent = serializedObject.FindProperty("_valueEvent");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.PropertyField(_source);

            EditorGUILayout.Space();

            EditorGUILayout.PropertyField(_channel);

            EditorGUILayout.PropertyField(_knobNumber);

            EditorGUILayout.Space();

            EditorGUILayout.PropertyField(_isRelative);

            if (!_isRelative.boolValue)
            {
                EditorGUILayout.PropertyField(_responseCurve);
                EditorGUILayout.PropertyField(_interpolator);
            }

            EditorGUILayout.Space();

            EditorGUILayout.PropertyField(_onEvent);
            EditorGUILayout.PropertyField(_offEvent);
            EditorGUILayout.PropertyField(_valueEvent);

            if (EditorApplication.isPlaying &&
                !serializedObject.isEditingMultipleObjects)
            {
                var instance = (KnobInput)target;
                instance.debugInput =
                    EditorGUILayout.Slider("Debug", instance.debugInput, 0, 1);
                EditorUtility.SetDirty(target); // request repaint
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
}
