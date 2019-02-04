//
// KinoContour - Contour line effect
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
    [CustomEditor(typeof(Contour))]
    public class ContourEditor : Editor
    {
        SerializedProperty _lineColor;
        SerializedProperty _backgroundColor;
        SerializedProperty _lowerThreshold;
        SerializedProperty _upperThreshold;
        SerializedProperty _colorSensitivity;
        SerializedProperty _depthSensitivity;
        SerializedProperty _normalSensitivity;
        SerializedProperty _fallOffDepth;

        static class Styles
        {
            static public readonly GUIContent lowerBound = new GUIContent("Lower Bound");
            static public readonly GUIContent upperBound = new GUIContent("Upper Bound");
            static public readonly GUIContent color = new GUIContent("Color");
            static public readonly GUIContent depth = new GUIContent("Depth");
            static public readonly GUIContent normal = new GUIContent("Normal");
        }

        static string useDeferredWarning =
            "G-buffer is required for normal edge detection. " +
            "Use the deferred rendering path.";

        bool CheckDeferred()
        {
            var cam = ((Contour)target).GetComponent<Camera>();
            return cam.actualRenderingPath == RenderingPath.DeferredShading;
        }

        void OnEnable()
        {
            _lineColor = serializedObject.FindProperty("_lineColor");
            _backgroundColor = serializedObject.FindProperty("_backgroundColor");
            _lowerThreshold = serializedObject.FindProperty("_lowerThreshold");
            _upperThreshold = serializedObject.FindProperty("_upperThreshold");
            _colorSensitivity = serializedObject.FindProperty("_colorSensitivity");
            _depthSensitivity = serializedObject.FindProperty("_depthSensitivity");
            _normalSensitivity = serializedObject.FindProperty("_normalSensitivity");
            _fallOffDepth = serializedObject.FindProperty("_fallOffDepth");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.PropertyField(_lineColor);
            EditorGUILayout.PropertyField(_backgroundColor);

            EditorGUILayout.LabelField("Threshold");
            EditorGUI.indentLevel++;
            EditorGUILayout.PropertyField(_lowerThreshold, Styles.lowerBound);
            EditorGUILayout.PropertyField(_upperThreshold, Styles.upperBound);
            EditorGUI.indentLevel--;

            EditorGUILayout.LabelField("Sensitivity");
            EditorGUI.indentLevel++;
            EditorGUILayout.PropertyField(_colorSensitivity, Styles.color);
            EditorGUILayout.PropertyField(_depthSensitivity, Styles.depth);
            EditorGUILayout.PropertyField(_normalSensitivity, Styles.normal);
            EditorGUI.indentLevel--;

            if (_depthSensitivity.hasMultipleDifferentValues ||
                _depthSensitivity.floatValue > 0)
                EditorGUILayout.PropertyField(_fallOffDepth);

            if (_normalSensitivity.floatValue > 0 && !CheckDeferred())
                EditorGUILayout.HelpBox(useDeferredWarning, MessageType.Warning);

            serializedObject.ApplyModifiedProperties();
        }
    }
}
