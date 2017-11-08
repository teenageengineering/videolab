//
// Kino/Vision - Frame visualization utility
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
    [CustomEditor(typeof(Vision))]
    public class VisionEditor : Editor
    {
        // Common
        SerializedProperty _source;
        SerializedProperty _blendRatio;
        SerializedProperty _preferDepthNormals;

        static GUIContent _textUseDepthNormals = new GUIContent("Use Depth Normals");

        // Depth
        SerializedProperty _depthRepeat;

        static GUIContent _textRepeat = new GUIContent("Repeat");

        // Normals
        SerializedProperty _validateNormals;

        static GUIContent _textCheckValidity = new GUIContent("Check Validity");

        // Motion vectors
        SerializedProperty _motionOverlayAmplitude;
        SerializedProperty _motionVectorsAmplitude;
        SerializedProperty _motionVectorsResolution;

        static GUIContent _textOverlayAmplitude = new GUIContent("Overlay Amplitude");
        static GUIContent _textArrowsAmplitude = new GUIContent("Arrows Amplitude");
        static GUIContent _textArrowsResolution = new GUIContent("Arrows Resolution");

        void OnEnable()
        {
            // Common
            _source = serializedObject.FindProperty("_source");
            _blendRatio = serializedObject.FindProperty("_blendRatio");
            _preferDepthNormals = serializedObject.FindProperty("_preferDepthNormals");

            // Depth
            _depthRepeat = serializedObject.FindProperty("_depthRepeat");

            // Normals
            _validateNormals = serializedObject.FindProperty("_validateNormals");

            // Motion vectors
            _motionOverlayAmplitude = serializedObject.FindProperty("_motionOverlayAmplitude");
            _motionVectorsAmplitude = serializedObject.FindProperty("_motionVectorsAmplitude");
            _motionVectorsResolution = serializedObject.FindProperty("_motionVectorsResolution");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.PropertyField(_source);

            var source = (Vision.Source)_source.enumValueIndex;

            if (source == Vision.Source.Depth)
            {
                // Depth
                EditorGUILayout.PropertyField(_depthRepeat, _textRepeat);
                EditorGUILayout.PropertyField(_preferDepthNormals, _textUseDepthNormals);
            }

            if (source == Vision.Source.Normals)
            {
                // Normals
                EditorGUILayout.PropertyField(_preferDepthNormals, _textUseDepthNormals);
                EditorGUI.BeginDisabledGroup(_preferDepthNormals.boolValue);
                EditorGUILayout.PropertyField(_validateNormals, _textCheckValidity);
                EditorGUI.EndDisabledGroup();
            }

            if (source == Vision.Source.MotionVectors)
            {
                // Motion vectors
                EditorGUILayout.PropertyField(_motionOverlayAmplitude, _textOverlayAmplitude);
                EditorGUILayout.PropertyField(_motionVectorsAmplitude, _textArrowsAmplitude);
                EditorGUILayout.PropertyField(_motionVectorsResolution, _textArrowsResolution);
            }

            EditorGUILayout.PropertyField(_blendRatio);

            serializedObject.ApplyModifiedProperties();
        }
    }
}
