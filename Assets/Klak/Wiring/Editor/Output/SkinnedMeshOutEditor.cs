using UnityEngine;
using UnityEditor;

namespace Klak.Wiring
{
    [CanEditMultipleObjects]
    [CustomEditor(typeof(SkinnedMeshOut))]
    public class SkinnedMeshOutEditor : Editor
    {
        SerializedProperty _skinnedMesh;
        SerializedProperty _blendShapeIndex;

        void OnEnable()
        {
            _skinnedMesh = serializedObject.FindProperty("_skinnedMesh");
            _blendShapeIndex = serializedObject.FindProperty("_blendShapeIndex");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.PropertyField(_skinnedMesh);

            SkinnedMeshRenderer skinnedMesh = _skinnedMesh.objectReferenceValue as SkinnedMeshRenderer;
            if (skinnedMesh)
            {
                string[] blendShapeNames = GetBlendShapeNames(skinnedMesh.gameObject);
                _blendShapeIndex.intValue = EditorGUILayout.Popup("Blend Shape Index", _blendShapeIndex.intValue, blendShapeNames);
            }

            serializedObject.ApplyModifiedProperties();
        }

        public string[] GetBlendShapeNames(GameObject obj)
        {
            SkinnedMeshRenderer head = obj.GetComponent<SkinnedMeshRenderer>();
            Mesh m = head.sharedMesh;
            string[] names = new string[m.blendShapeCount];
            for (int i = 0; i < m.blendShapeCount; i++)
                names[i] = m.GetBlendShapeName(i);

            return names;
        }
    }
}