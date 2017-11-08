//
// KinoIsoline - Isoline effect
//
// Copyright (C) 2015 Keijiro Takahashi
//
// Permission is hereby granted, free of charge, to any person obtaining a copy of
// this software and associated documentation files (the "Software"), to deal in
// the Software without restriction, including without limitation the rights to
// use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of
// the Software, and to permit persons to whom the Software is furnished to do so,
// subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS
// FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR
// COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER
// IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN
// CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//
using UnityEngine;
using UnityEditor;

namespace Kino
{
    [CanEditMultipleObjects]
    [CustomEditor(typeof(Isoline))]
    public class IsolineEditor : Editor
    {
        SerializedProperty _lineColor;
        SerializedProperty _luminanceBlending;
        SerializedProperty _fallOffDepth;
        SerializedProperty _backgroundColor;

        SerializedProperty _axis;
        SerializedProperty _interval;
        SerializedProperty _offset;

        SerializedProperty _distortionFrequency;
        SerializedProperty _distortionAmount;

        SerializedProperty _modulationMode;
        SerializedProperty _modulationAxis;
        SerializedProperty _modulationFrequency;
        SerializedProperty _modulationSpeed;
        SerializedProperty _modulationExponent;

        static GUIContent _textAmount    = new GUIContent("Amount");
        static GUIContent _textAxis      = new GUIContent("Axis");
        static GUIContent _textExponent  = new GUIContent("Exponent");
        static GUIContent _textFrequency = new GUIContent("Frequency");
        static GUIContent _textMode      = new GUIContent("Mode");
        static GUIContent _textSpeed     = new GUIContent("Speed");

        void OnEnable()
        {
            _lineColor         = serializedObject.FindProperty("_lineColor");
            _luminanceBlending = serializedObject.FindProperty("_luminanceBlending");
            _fallOffDepth      = serializedObject.FindProperty("_fallOffDepth");
            _backgroundColor   = serializedObject.FindProperty("_backgroundColor");

            _axis     = serializedObject.FindProperty("_axis");
            _interval = serializedObject.FindProperty("_interval");
            _offset   = serializedObject.FindProperty("_offset");

            _distortionFrequency = serializedObject.FindProperty("_distortionFrequency");
            _distortionAmount    = serializedObject.FindProperty("_distortionAmount");

            _modulationMode      = serializedObject.FindProperty("_modulationMode");
            _modulationAxis      = serializedObject.FindProperty("_modulationAxis");
            _modulationFrequency = serializedObject.FindProperty("_modulationFrequency");
            _modulationSpeed     = serializedObject.FindProperty("_modulationSpeed");
            _modulationExponent  = serializedObject.FindProperty("_modulationExponent");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.LabelField("Color", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(_lineColor);
            EditorGUILayout.PropertyField(_luminanceBlending);
            EditorGUILayout.PropertyField(_fallOffDepth);
            EditorGUILayout.PropertyField(_backgroundColor);

            EditorGUILayout.Space();

            EditorGUILayout.LabelField("Potential Function", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(_axis);
            EditorGUILayout.PropertyField(_interval);
            EditorGUILayout.PropertyField(_offset);

            EditorGUILayout.Space();

            EditorGUILayout.LabelField("Distortion", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(_distortionFrequency, _textFrequency);
            EditorGUILayout.PropertyField(_distortionAmount, _textAmount);

            EditorGUILayout.Space();

            EditorGUILayout.LabelField("Modulation", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(_modulationMode, _textMode);

            if (_modulationMode.hasMultipleDifferentValues || _modulationMode.intValue > 0)
            {
                EditorGUILayout.PropertyField(_modulationAxis, _textAxis);
                EditorGUILayout.PropertyField(_modulationFrequency, _textFrequency);
                EditorGUILayout.PropertyField(_modulationSpeed, _textSpeed);
                EditorGUILayout.PropertyField(_modulationExponent, _textExponent);
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
}
