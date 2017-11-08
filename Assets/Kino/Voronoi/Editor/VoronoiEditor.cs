//
// Kino/Voronoi - Voronoi diagram image effect
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

namespace Kino
{
    [CanEditMultipleObjects]
    [CustomEditor(typeof(Voronoi))]
    public class VoronoiEditor : Editor
    {
        SerializedProperty _lineColor;
        SerializedProperty _cellColor;
        SerializedProperty _backgroundColor;
        SerializedProperty _rangeMin;
        SerializedProperty _rangeMax;
        SerializedProperty _cellExponent;
        SerializedProperty _iteration;
        SerializedProperty _opacity;

        void OnEnable()
        {
            _lineColor = serializedObject.FindProperty("_lineColor");
            _cellColor = serializedObject.FindProperty("_cellColor");
            _backgroundColor = serializedObject.FindProperty("_backgroundColor");
            _rangeMin = serializedObject.FindProperty("_rangeMin");
            _rangeMax = serializedObject.FindProperty("_rangeMax");
            _cellExponent = serializedObject.FindProperty("_cellExponent");
            _iteration = serializedObject.FindProperty("_iteration");
            _opacity = serializedObject.FindProperty("_opacity");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.PropertyField(_lineColor);
            EditorGUILayout.PropertyField(_backgroundColor);
            EditorGUILayout.PropertyField(_iteration);
            EditorGUILayout.PropertyField(_opacity);

            EditorGUILayout.Space();

            EditorGUILayout.LabelField("Input Range", EditorStyles.boldLabel);
            EditorGUI.indentLevel++;
            EditorGUILayout.PropertyField(_rangeMin, new GUIContent("Minimum"));
            EditorGUILayout.PropertyField(_rangeMax, new GUIContent("Maximum"));
            EditorGUI.indentLevel--;

            EditorGUILayout.Space();

            EditorGUILayout.LabelField("Cell Style", EditorStyles.boldLabel);
            EditorGUI.indentLevel++;
            EditorGUILayout.PropertyField(_cellColor, new GUIContent("Fill Color"));
            EditorGUILayout.PropertyField(_cellExponent, new GUIContent("Exponent"));
            EditorGUI.indentLevel--;

            serializedObject.ApplyModifiedProperties();
        }
    }
}
