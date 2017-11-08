//
// KinoRamp - Color ramp overlay
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
    [CustomEditor(typeof(Ramp))]
    public class RampEditor : Editor
    {
        SerializedProperty _color1;
        SerializedProperty _color2;
        SerializedProperty _angle;
        SerializedProperty _opacity;
        SerializedProperty _blendMode;
        SerializedProperty _debug;

        static GUIContent _textDebug = new GUIContent("Debug (view ramp)");

        void OnEnable()
        {
            _color1 = serializedObject.FindProperty("_color1");
            _color2 = serializedObject.FindProperty("_color2");
            _angle = serializedObject.FindProperty("_angle");
            _opacity = serializedObject.FindProperty("_opacity");
            _blendMode = serializedObject.FindProperty("_blendMode");
            _debug = serializedObject.FindProperty("_debug");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.PropertyField(_color1);
            EditorGUILayout.PropertyField(_color2);
            EditorGUILayout.PropertyField(_angle);
            EditorGUILayout.PropertyField(_opacity);
            EditorGUILayout.PropertyField(_blendMode);
            EditorGUILayout.PropertyField(_debug, _textDebug);

            serializedObject.ApplyModifiedProperties();
        }
    }
}
