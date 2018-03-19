using UnityEngine;
using UnityEditor;

namespace Reaktion {

[CanEditMultipleObjects]
[CustomEditor(typeof(SelfDestruction))]
public class SelfDestructionEditor : Editor
{
    SerializedProperty _conditionType;
    SerializedProperty _referenceType;
    SerializedProperty _maxDistance;
    SerializedProperty _bounds;
    SerializedProperty _lifetime;
    SerializedProperty _referencePoint;
    SerializedProperty _referenceObject;
    SerializedProperty _referenceName;

    void OnEnable()
    {
        _conditionType = serializedObject.FindProperty("conditionType");
        _referenceType = serializedObject.FindProperty("referenceType");
        _maxDistance = serializedObject.FindProperty("maxDistance");
        _bounds = serializedObject.FindProperty("bounds");
        _lifetime = serializedObject.FindProperty("lifetime");
        _referencePoint = serializedObject.FindProperty("referencePoint");
        _referenceObject = serializedObject.FindProperty("referenceObject");
        _referenceName = serializedObject.FindProperty("referenceName");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        EditorGUILayout.PropertyField(_conditionType);

        if (_conditionType.intValue != (int)SelfDestruction.ConditionType.Time) 
        {
            if (_conditionType.intValue == (int)SelfDestruction.ConditionType.Distance)
                EditorGUILayout.PropertyField(_maxDistance);
            else
                EditorGUILayout.PropertyField(_bounds);
            
            EditorGUILayout.PropertyField(_referenceType);

            if (_referenceType.intValue == (int)SelfDestruction.ReferenceType.Point)
                EditorGUILayout.PropertyField(_referencePoint);
            else if (_referenceType.intValue == (int)SelfDestruction.ReferenceType.GameObject)
                EditorGUILayout.PropertyField(_referenceObject);
            else if (_referenceType.intValue == (int)SelfDestruction.ReferenceType.GameObjectName)
                EditorGUILayout.PropertyField(_referenceName);
        }
        else
            EditorGUILayout.PropertyField(_lifetime);

        serializedObject.ApplyModifiedProperties();
    }
}

} // namespace Reaktion