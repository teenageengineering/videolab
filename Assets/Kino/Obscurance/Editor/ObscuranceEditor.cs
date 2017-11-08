//
// Kino/Obscurance - Screen space ambient obscurance image effect
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
    [CustomEditor(typeof(Obscurance))]
    public class ObscuranceEditor : Editor
    {
        SerializedProperty _intensity;
        SerializedProperty _radius;
        SerializedProperty _sampleCount;
        SerializedProperty _sampleCountValue;
        SerializedProperty _downsampling;
        SerializedProperty _occlusionSource;
        SerializedProperty _ambientOnly;

        static GUIContent _textValue = new GUIContent("Value");

        static string _textNoGBuffer =
            "G-buffer is currently unavailable. " +
            "Change Renderring Path in camera settings to Deferred.";

        static string _textNoAmbientOnly =
            "Ambient-only mode is currently disabled; " +
            "it requires G-buffer source and HDR rendering.";

        #if UNITY_5_4_OR_NEWER
        static string _textSinglePassStereo =
            "Ambient-only mode isn't supported in single-pass stereo rendering.";
        #endif

        void OnEnable()
        {
            _intensity = serializedObject.FindProperty("_intensity");
            _radius = serializedObject.FindProperty("_radius");
            _sampleCount = serializedObject.FindProperty("_sampleCount");
            _sampleCountValue = serializedObject.FindProperty("_sampleCountValue");
            _downsampling = serializedObject.FindProperty("_downsampling");
            _occlusionSource = serializedObject.FindProperty("_occlusionSource");
            _ambientOnly = serializedObject.FindProperty("_ambientOnly");
        }

        public override void OnInspectorGUI()
        {
            var obscurance = (Obscurance)target;

            serializedObject.Update();

            EditorGUILayout.PropertyField(_intensity);
            EditorGUILayout.PropertyField(_radius);
            EditorGUILayout.PropertyField(_sampleCount);

            if (_sampleCount.hasMultipleDifferentValues ||
                _sampleCount.enumValueIndex == (int)Obscurance.SampleCount.Custom)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(_sampleCountValue, _textValue);
                EditorGUI.indentLevel--;
            }

            EditorGUILayout.PropertyField(_downsampling);
            EditorGUILayout.PropertyField(_occlusionSource);

            if (!_occlusionSource.hasMultipleDifferentValues)
                if (_occlusionSource.enumValueIndex != (int)obscurance.occlusionSource)
                    EditorGUILayout.HelpBox(_textNoGBuffer, MessageType.Warning);

            EditorGUILayout.PropertyField(_ambientOnly);

            if (!_ambientOnly.hasMultipleDifferentValues)
                if (_ambientOnly.boolValue != obscurance.ambientOnly)
                    EditorGUILayout.HelpBox(_textNoAmbientOnly, MessageType.Warning);

            #if UNITY_5_5_OR_NEWER
            if (_ambientOnly.boolValue && PlayerSettings.stereoRenderingPath == StereoRenderingPath.SinglePass)
                EditorGUILayout.HelpBox(_textSinglePassStereo, MessageType.Warning);
            #elif UNITY_5_4_OR_NEWER
            if (_ambientOnly.boolValue && PlayerSettings.singlePassStereoRendering)
                EditorGUILayout.HelpBox(_textSinglePassStereo, MessageType.Warning);
            #endif

            serializedObject.ApplyModifiedProperties();
        }
    }
}
