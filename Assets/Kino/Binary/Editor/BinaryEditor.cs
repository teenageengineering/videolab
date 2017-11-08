//
// KinoBinary - 1-bit monochrome image effect
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
    [CustomEditor(typeof(Binary))]
    public class BinaryEditor : Editor
    {
        SerializedProperty _ditherMatrix;
        SerializedProperty _ditherScale;
        SerializedProperty _color0;
        SerializedProperty _color1;
        SerializedProperty _blendMode;
        SerializedProperty _blendFactor;

        static GUIContent _textColor0 = new GUIContent("Color (dark)");
        static GUIContent _textColor1 = new GUIContent("Color (light)");

        void OnEnable()
        {
            _ditherMatrix = serializedObject.FindProperty("_ditherMatrix");
            _ditherScale  = serializedObject.FindProperty("_ditherScale");
            _color0       = serializedObject.FindProperty("_color0");
            _color1       = serializedObject.FindProperty("_color1");
            _blendMode    = serializedObject.FindProperty("_blendMode");
            _blendFactor  = serializedObject.FindProperty("_blendFactor");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.PropertyField(_ditherMatrix);
            EditorGUILayout.PropertyField(_ditherScale);
            EditorGUILayout.PropertyField(_color0, _textColor0);
            EditorGUILayout.PropertyField(_color1, _textColor1);
            EditorGUILayout.PropertyField(_blendMode);
            EditorGUILayout.PropertyField(_blendFactor);

            serializedObject.ApplyModifiedProperties();
        }
    }
}
