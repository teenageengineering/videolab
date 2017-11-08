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
        SerializedProperty _lowThreshold;
        SerializedProperty _highThreshold;
        SerializedProperty _depthSensitivity;
        SerializedProperty _normalSensitivity;
        SerializedProperty _fallOffDepth;

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
            _lowThreshold = serializedObject.FindProperty("_lowThreshold");
            _highThreshold = serializedObject.FindProperty("_highThreshold");
            _depthSensitivity = serializedObject.FindProperty("_depthSensitivity");
            _normalSensitivity = serializedObject.FindProperty("_normalSensitivity");
            _fallOffDepth = serializedObject.FindProperty("_fallOffDepth");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.PropertyField(_lineColor);
            EditorGUILayout.PropertyField(_backgroundColor);

            EditorGUILayout.Space();

            EditorGUILayout.PropertyField(_lowThreshold);
            EditorGUILayout.PropertyField(_highThreshold);

            EditorGUILayout.Space();

            EditorGUILayout.PropertyField(_depthSensitivity);
            EditorGUILayout.PropertyField(_normalSensitivity);

            if (_normalSensitivity.floatValue > 0 && !CheckDeferred())
                EditorGUILayout.HelpBox(useDeferredWarning, MessageType.Warning);

            EditorGUILayout.Space();

            if (_depthSensitivity.hasMultipleDifferentValues ||
                _depthSensitivity.floatValue > 0)
                EditorGUILayout.PropertyField(_fallOffDepth);

            serializedObject.ApplyModifiedProperties();
        }
    }
}
