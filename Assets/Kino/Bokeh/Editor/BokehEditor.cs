//
// Kino/Bokeh - Depth of field effect
//
// Copyright (C) 2016 Unity Technologies
// Copyright (C) 2015 Keijiro Takahashi
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
    [CanEditMultipleObjects, CustomEditor(typeof(Bokeh))]
    public class BokehEditor : Editor
    {
        SerializedProperty _pointOfFocus;
        SerializedProperty _focusDistance;
        SerializedProperty _fNumber;
        SerializedProperty _useCameraFov;
        SerializedProperty _focalLength;
        SerializedProperty _kernelSize;
        SerializedProperty _visualize;

        static GUIContent _labelPointOfFocus = new GUIContent(
            "Point Of Focus",
            "Transform that represents the point of focus."
        );

        static GUIContent _labelFocusDistance = new GUIContent(
            "Distance",
            "Distance to the point of focus (only used when none is specified in PointOfFocus)."
        );

        static GUIContent _labelFNumber = new GUIContent(
            "Aperture (f-stop)",
            "Ratio of aperture (known as f-stop or f-number). The smaller the value is, the shallower the depth of field is."
        );

        static GUIContent _labelUseCameraFov = new GUIContent(
            "Use Camera FOV",
            "Calculate the focal length from the field-of-view value."
        );

        static GUIContent _labelFocalLength = new GUIContent(
            "Focal Length (mm)",
            "Distance between the lens and the film. The larger the value is, the shallower the depth of field is."
        );

        static GUIContent _labelKernelSize = new GUIContent(
            "Kernel Size",
            "Convolution kernel size of the bokeh filter, which determines the maximum radius of bokeh. It also affects the performance (the larger the kernel is, the longer the GPU time is required)."
        );

        static GUIContent _labelVisualize = new GUIContent(
            "Visualize",
            "Visualize the depths as red (focused), green (far) or blue (near)."
        );

        void OnEnable()
        {
            _pointOfFocus = serializedObject.FindProperty("_pointOfFocus");
            _focusDistance = serializedObject.FindProperty("_focusDistance");
            _fNumber = serializedObject.FindProperty("_fNumber");
            _useCameraFov = serializedObject.FindProperty("_useCameraFov");
            _focalLength = serializedObject.FindProperty("_focalLength");
            _kernelSize = serializedObject.FindProperty("_kernelSize");
            _visualize = serializedObject.FindProperty("_visualize");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            // Point of focus
            EditorGUILayout.PropertyField(_pointOfFocus, _labelPointOfFocus);
            if (_pointOfFocus.hasMultipleDifferentValues || _pointOfFocus.objectReferenceValue == null)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(_focusDistance, _labelFocusDistance);
                EditorGUI.indentLevel--;
            }

            // Aperture
            EditorGUILayout.PropertyField(_fNumber, _labelFNumber);

            // Focal Length
            EditorGUILayout.PropertyField(_useCameraFov, _labelUseCameraFov);

            if (_useCameraFov.hasMultipleDifferentValues || !_useCameraFov.boolValue)
            {
                if (_focalLength.hasMultipleDifferentValues)
                {
                    EditorGUILayout.PropertyField(_focalLength);
                }
                else
                {
                    EditorGUI.BeginChangeCheck();

                    var f = _focalLength.floatValue * 1000;
                    f = EditorGUILayout.Slider(_labelFocalLength, f, 10.0f, 300.0f);

                    if (EditorGUI.EndChangeCheck())
                        _focalLength.floatValue = f / 1000;
                }
            }

            // Kernel Size
            EditorGUILayout.PropertyField(_kernelSize, _labelKernelSize);

            // Visualize
            EditorGUILayout.PropertyField(_visualize, _labelVisualize);

            serializedObject.ApplyModifiedProperties();
        }
    }
}
