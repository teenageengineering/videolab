using UnityEngine;
using UnityEditor;

namespace Reaktion {

[CanEditMultipleObjects]
[CustomEditor(typeof(Spawner))]
public class SpawnerEditor : Editor
{
    SerializedProperty _prefabs;
    SerializedProperty _spawnRate;
    SerializedProperty _spawnRateRandomness;
    SerializedProperty _distribution;
    SerializedProperty _sphereRadius;
    SerializedProperty _boxSize;
    SerializedProperty _spawnPoints;
    SerializedProperty _randomRotation;
    SerializedProperty _parent;

    void OnEnable()
    {
        _prefabs = serializedObject.FindProperty("prefabs");
        _spawnRate = serializedObject.FindProperty("_spawnRate");
        _spawnRateRandomness = serializedObject.FindProperty("_spawnRateRandomness");
        _distribution = serializedObject.FindProperty("distribution");
        _sphereRadius = serializedObject.FindProperty("sphereRadius");
        _boxSize = serializedObject.FindProperty("boxSize");
        _spawnPoints = serializedObject.FindProperty("spawnPoints");
        _randomRotation = serializedObject.FindProperty("randomRotation");
        _parent = serializedObject.FindProperty("parent");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        EditorGUILayout.PropertyField(_prefabs, new GUIContent("Prefabs"), true);

        EditorGUILayout.Space();

        EditorGUILayout.PropertyField(_spawnRate);
        EditorGUILayout.PropertyField(_spawnRateRandomness);

        EditorGUILayout.Space();

        EditorGUILayout.PropertyField(_distribution);

        if (_distribution.intValue == (int)Spawner.Distribution.InSphere)
            EditorGUILayout.PropertyField(_sphereRadius);
        else if (_distribution.intValue == (int)Spawner.Distribution.InBox)
            EditorGUILayout.PropertyField(_boxSize);
        else
            EditorGUILayout.PropertyField(_spawnPoints, new GUIContent("Spawn Points"), true);

        EditorGUILayout.Space();

        EditorGUILayout.PropertyField(_randomRotation);
        EditorGUILayout.PropertyField(_parent);

        serializedObject.ApplyModifiedProperties();
    }
}

} // namespace Reaktion
