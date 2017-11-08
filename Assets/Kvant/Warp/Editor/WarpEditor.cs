//
// Kvant/Warp - Warp (hyperspace) light streaks effect
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

namespace Kvant
{
    [CanEditMultipleObjects]
    [CustomEditor(typeof(Warp))]
    public class WarpEditor : Editor
    {
        SerializedProperty _template;
        SerializedProperty _randomSeed;

        SerializedProperty _throttle;
        SerializedProperty _extent;

        SerializedProperty _speed;
        SerializedProperty _speedRandomness;

        SerializedProperty _lineRadius;
        SerializedProperty _lineWidth;
        SerializedProperty _lineWidthRandomness;

        static GUIContent _textRandomness = new GUIContent("Randomness");

        void OnEnable()
        {
            _template = serializedObject.FindProperty("_template");
            _randomSeed = serializedObject.FindProperty("_randomSeed");

            _throttle = serializedObject.FindProperty("_throttle");
            _extent = serializedObject.FindProperty("_extent");

            _speed = serializedObject.FindProperty("_speed");
            _speedRandomness = serializedObject.FindProperty("_speedRandomness");

            _lineRadius = serializedObject.FindProperty("_lineRadius");
            _lineWidth = serializedObject.FindProperty("_lineWidth");
            _lineWidthRandomness = serializedObject.FindProperty("_lineWidthRandomness");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.PropertyField(_template);
            EditorGUILayout.PropertyField(_randomSeed);

            EditorGUILayout.Space();

            EditorGUILayout.PropertyField(_throttle);
            EditorGUILayout.PropertyField(_extent);

            EditorGUILayout.Space();

            EditorGUILayout.PropertyField(_speed);
            EditorGUI.indentLevel++;
            EditorGUILayout.PropertyField(_speedRandomness, _textRandomness);
            EditorGUI.indentLevel--;

            EditorGUILayout.Space();

            EditorGUILayout.LabelField("Line Size", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(_lineRadius);
            EditorGUILayout.PropertyField(_lineWidth);
            EditorGUI.indentLevel++;
            EditorGUILayout.PropertyField(_lineWidthRandomness, _textRandomness);
            EditorGUI.indentLevel--;

            serializedObject.ApplyModifiedProperties();
        }
    }
}
