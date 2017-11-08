//
// Custom editor class for Stream
//
using UnityEngine;
using UnityEditor;

namespace Kvant
{
    [CustomEditor(typeof(Stream)), CanEditMultipleObjects]
    public class StreamEditor : Editor
    {
        SerializedProperty _maxParticles;
        SerializedProperty _emitterPosition;
        SerializedProperty _emitterSize;
        SerializedProperty _throttle;

        SerializedProperty _direction;
        SerializedProperty _minSpeed;
        SerializedProperty _maxSpeed;
        SerializedProperty _spread;

        SerializedProperty _noiseAmplitude;
        SerializedProperty _noiseFrequency;
        SerializedProperty _noiseSpeed;

        SerializedProperty _color;
        SerializedProperty _tail;
        SerializedProperty _randomSeed;
        SerializedProperty _debug;

        static GUIContent _textCenter    = new GUIContent("Center");
        static GUIContent _textSize      = new GUIContent("Size");
        static GUIContent _textSpeed     = new GUIContent("Speed");
        static GUIContent _textAmplitude = new GUIContent("Amplitude");
        static GUIContent _textFrequency = new GUIContent("Frequency");

        void OnEnable()
        {
            _maxParticles    = serializedObject.FindProperty("_maxParticles");
            _emitterPosition = serializedObject.FindProperty("_emitterPosition");
            _emitterSize     = serializedObject.FindProperty("_emitterSize");
            _throttle        = serializedObject.FindProperty("_throttle");

            _direction = serializedObject.FindProperty("_direction");
            _minSpeed  = serializedObject.FindProperty("_minSpeed");
            _maxSpeed  = serializedObject.FindProperty("_maxSpeed");
            _spread    = serializedObject.FindProperty("_spread");

            _noiseAmplitude = serializedObject.FindProperty("_noiseAmplitude");
            _noiseFrequency = serializedObject.FindProperty("_noiseFrequency");
            _noiseSpeed     = serializedObject.FindProperty("_noiseSpeed");

            _color      = serializedObject.FindProperty("_color");
            _tail       = serializedObject.FindProperty("_tail");
            _randomSeed = serializedObject.FindProperty("_randomSeed");
            _debug      = serializedObject.FindProperty("_debug");
        }

        public override void OnInspectorGUI()
        {
            var targetStream = target as Stream;

            serializedObject.Update();

            EditorGUI.BeginChangeCheck();

            EditorGUILayout.PropertyField(_maxParticles);
            if (!_maxParticles.hasMultipleDifferentValues)
                EditorGUILayout.LabelField(" ", "Allocated: " + targetStream.maxParticles, EditorStyles.miniLabel);

            if (EditorGUI.EndChangeCheck())
                targetStream.NotifyConfigChange();

            EditorGUILayout.LabelField("Emitter", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(_emitterPosition, _textCenter);
            EditorGUILayout.PropertyField(_emitterSize, _textSize);
            EditorGUILayout.PropertyField(_throttle);

            EditorGUILayout.Space();

            EditorGUILayout.LabelField("Velocity", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(_direction);
            MinMaxSlider(_textSpeed, _minSpeed, _maxSpeed, 0.0f, 50.0f);
            EditorGUILayout.PropertyField(_spread);

            EditorGUILayout.Space();

            EditorGUILayout.LabelField("Turbulent Noise", EditorStyles.boldLabel);
            EditorGUILayout.Slider(_noiseAmplitude, 0.0f, 50.0f, _textAmplitude);
            EditorGUILayout.Slider(_noiseFrequency, 0.01f, 1.0f, _textFrequency);
            EditorGUILayout.Slider(_noiseSpeed, 0.0f, 10.0f, _textSpeed);

            EditorGUILayout.Space();

            EditorGUILayout.PropertyField(_color);
            EditorGUILayout.Slider(_tail, 0.0f, 20.0f);
            EditorGUILayout.PropertyField(_randomSeed);
            EditorGUILayout.PropertyField(_debug);

            serializedObject.ApplyModifiedProperties();
        }

        void MinMaxSlider(GUIContent label, SerializedProperty propMin, SerializedProperty propMax, float minLimit, float maxLimit)
        {
            var min = propMin.floatValue;
            var max = propMax.floatValue;

            EditorGUI.BeginChangeCheck();

            // Min-max slider.
            EditorGUILayout.MinMaxSlider(label, ref min, ref max, minLimit, maxLimit);

            var prevIndent = EditorGUI.indentLevel;
            EditorGUI.indentLevel = 0;

            // Float value boxes.
            var rect = EditorGUILayout.GetControlRect();
            rect.x += EditorGUIUtility.labelWidth;
            rect.width = (rect.width - EditorGUIUtility.labelWidth) / 2 - 2;

            if (EditorGUIUtility.wideMode)
            {
                EditorGUIUtility.labelWidth = 28;
                min = Mathf.Clamp(EditorGUI.FloatField(rect, "min", min), minLimit, max);
                rect.x += rect.width + 4;
                max = Mathf.Clamp(EditorGUI.FloatField(rect, "max", max), min, maxLimit);
                EditorGUIUtility.labelWidth = 0;
            }
            else
            {
                min = Mathf.Clamp(EditorGUI.FloatField(rect, min), minLimit, max);
                rect.x += rect.width + 4;
                max = Mathf.Clamp(EditorGUI.FloatField(rect, max), min, maxLimit);
            }

            EditorGUI.indentLevel = prevIndent;

            if (EditorGUI.EndChangeCheck()) {
                propMin.floatValue = min;
                propMax.floatValue = max;
            }
        }
    }
}
