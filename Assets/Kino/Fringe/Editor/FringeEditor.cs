//
// KinoFringe - Chromatic aberration effect
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
    [CanEditMultipleObjects, CustomEditor(typeof(Fringe))]
    public class FringeEditor : Editor
    {
        SerializedProperty _lateralShift;
        SerializedProperty _axialStrength;
        SerializedProperty _axialShift;
        SerializedProperty _axialQuality;

        static GUIContent _textShift = new GUIContent("Shift");
        static GUIContent _textStrength = new GUIContent("Strength");
        static GUIContent _textQuality = new GUIContent("Quality");

        void OnEnable()
        {
            _lateralShift = serializedObject.FindProperty("_lateralShift");
            _axialStrength = serializedObject.FindProperty("_axialStrength");
            _axialShift = serializedObject.FindProperty("_axialShift");
            _axialQuality = serializedObject.FindProperty("_axialQuality");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.LabelField("Lateral CA", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(_lateralShift, _textShift);

            EditorGUILayout.LabelField("Axial CA (purple fringing)", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(_axialStrength, _textStrength);
            EditorGUILayout.PropertyField(_axialShift, _textShift);
            EditorGUILayout.PropertyField(_axialQuality, _textQuality);

            serializedObject.ApplyModifiedProperties();
        }
    }
}
