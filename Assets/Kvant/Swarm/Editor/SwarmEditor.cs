//
// Custom editor class for Swarm
//
using UnityEngine;
using UnityEditor;

namespace Kvant
{
    [CanEditMultipleObjects, CustomEditor(typeof(Swarm))]
    public class SwarmEditor : Editor
    {
        SerializedProperty _lineCount;
        SerializedProperty _historyLength;
        SerializedProperty _throttle;
        SerializedProperty _flow;

        SerializedProperty _attractor;
        SerializedProperty _attractorPosition;
        SerializedProperty _attractorRadius;
        SerializedProperty _forcePerDistance;
        SerializedProperty _forceRandomness;
        SerializedProperty _drag;

        SerializedProperty _noiseAmplitude;
        SerializedProperty _noiseFrequency;
        SerializedProperty _noiseSpread;
        SerializedProperty _noiseMotion;

        SerializedProperty _swirlAmplitude;
        SerializedProperty _swirlFrequency;

        SerializedProperty _lineWidth;
        SerializedProperty _lineWidthRandomness;
        SerializedProperty _colorMode;
        SerializedProperty _color1;
        SerializedProperty _color2;
        SerializedProperty _metallic;
        SerializedProperty _smoothness;

        SerializedProperty _castShadows;
        SerializedProperty _receiveShadows;

        SerializedProperty _fixTimeStep;
        SerializedProperty _stepsPerSecond;
        SerializedProperty _randomSeed;

        static GUIContent _textAmplitude = new GUIContent("Amplitude");
        static GUIContent _textFlow      = new GUIContent("Flow Vector");
        static GUIContent _textFrequency = new GUIContent("Frequency");
        static GUIContent _textMotion    = new GUIContent("Motion");
        static GUIContent _textSpread    = new GUIContent("Spread");

        void OnEnable()
        {
            _lineCount     = serializedObject.FindProperty("_lineCount");
            _historyLength = serializedObject.FindProperty("_historyLength");
            _throttle      = serializedObject.FindProperty("_throttle");
            _flow          = serializedObject.FindProperty("_flow");

            _attractor         = serializedObject.FindProperty("_attractor");
            _attractorPosition = serializedObject.FindProperty("_attractorPosition");
            _attractorRadius   = serializedObject.FindProperty("_attractorRadius");
            _forcePerDistance  = serializedObject.FindProperty("_forcePerDistance");
            _forceRandomness   = serializedObject.FindProperty("_forceRandomness");
            _drag              = serializedObject.FindProperty("_drag");

            _noiseAmplitude = serializedObject.FindProperty("_noiseAmplitude");
            _noiseFrequency = serializedObject.FindProperty("_noiseFrequency");
            _noiseSpread    = serializedObject.FindProperty("_noiseSpread");
            _noiseMotion    = serializedObject.FindProperty("_noiseMotion");

            _swirlAmplitude = serializedObject.FindProperty("_swirlAmplitude");
            _swirlFrequency = serializedObject.FindProperty("_swirlFrequency");

            _lineWidth           = serializedObject.FindProperty("_lineWidth");
            _lineWidthRandomness = serializedObject.FindProperty("_lineWidthRandomness");

            _colorMode  = serializedObject.FindProperty("_colorMode");
            _color1     = serializedObject.FindProperty("_color1");
            _color2     = serializedObject.FindProperty("_color2");
            _metallic   = serializedObject.FindProperty("_metallic");
            _smoothness = serializedObject.FindProperty("_smoothness");

            _castShadows    = serializedObject.FindProperty("_castShadows");
            _receiveShadows = serializedObject.FindProperty("_receiveShadows");

            _fixTimeStep    = serializedObject.FindProperty("_fixTimeStep");
            _stepsPerSecond = serializedObject.FindProperty("_stepsPerSecond");
            _randomSeed     = serializedObject.FindProperty("_randomSeed");
        }

        public override void OnInspectorGUI()
        {
            var instance = target as Swarm;

            serializedObject.Update();

            EditorGUI.BeginChangeCheck();

            EditorGUILayout.PropertyField(_lineCount);
            EditorGUILayout.PropertyField(_historyLength);

            if (EditorGUI.EndChangeCheck()) instance.Restart();

            EditorGUILayout.PropertyField(_throttle);
            EditorGUILayout.PropertyField(_flow, _textFlow);

            EditorGUILayout.Space();

            EditorGUILayout.LabelField("Dynamics", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(_attractor);
            if (_attractor.hasMultipleDifferentValues ||
                _attractor.objectReferenceValue == null)
                EditorGUILayout.PropertyField(_attractorPosition);
            EditorGUILayout.PropertyField(_attractorRadius);
            EditorGUILayout.PropertyField(_forcePerDistance);
            EditorGUILayout.PropertyField(_forceRandomness);
            EditorGUILayout.PropertyField(_drag);

            EditorGUILayout.Space();

            EditorGUILayout.LabelField("Turbulent Noise", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(_noiseAmplitude, _textAmplitude);
            EditorGUILayout.PropertyField(_noiseFrequency, _textFrequency);
            EditorGUILayout.PropertyField(_noiseSpread, _textSpread);
            EditorGUILayout.PropertyField(_noiseMotion, _textMotion);

            EditorGUILayout.Space();

            EditorGUILayout.PropertyField(_swirlAmplitude);
            EditorGUILayout.PropertyField(_swirlFrequency);

            EditorGUILayout.Space();

            EditorGUILayout.LabelField("Render Settings", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(_lineWidth);
            EditorGUILayout.PropertyField(_lineWidthRandomness);

            EditorGUILayout.Space();

            EditorGUILayout.PropertyField(_colorMode);
            EditorGUILayout.PropertyField(_color1);
            EditorGUILayout.PropertyField(_color2);
            EditorGUILayout.Slider(_metallic, 0, 1);
            EditorGUILayout.Slider(_smoothness, 0, 1);

            EditorGUILayout.Space();

            EditorGUILayout.PropertyField(_castShadows);
            EditorGUILayout.PropertyField(_receiveShadows);

            EditorGUILayout.Space();

            EditorGUILayout.PropertyField(_fixTimeStep);
            if (_fixTimeStep.hasMultipleDifferentValues || _fixTimeStep.boolValue)
                EditorGUILayout.PropertyField(_stepsPerSecond);

            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(_randomSeed);
            if (EditorGUI.EndChangeCheck()) instance.Restart();

            serializedObject.ApplyModifiedProperties();
        }
    }
}
