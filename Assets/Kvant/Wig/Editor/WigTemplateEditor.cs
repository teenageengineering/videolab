using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;

namespace Kvant
{
    [CustomEditor(typeof(WigTemplate))]
    public class WigTemplateEditor : Editor
    {
        #region Editor functions

        SerializedProperty _segmentCount;

        void OnEnable()
        {
            _segmentCount = serializedObject.FindProperty("_segmentCount");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            // Editable properties.
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(_segmentCount);
            var rebuild = EditorGUI.EndChangeCheck();

            serializedObject.ApplyModifiedProperties();

            // Rebuild the template mesh when the properties are changed.
            if (rebuild)
                foreach (var t in targets) ((WigTemplate)t).RebuildMesh();
        }

        #endregion

        #region Create menu item functions

        static Object[] SelectedMeshes {
            get { return Selection.GetFiltered(typeof(Mesh), SelectionMode.Deep); }
        }

        [MenuItem("Assets/Kvant/Wig/Convert To Template", true)]
        static bool ValidateConvertToTemplate()
        {
            return SelectedMeshes.Length > 0;
        }

        [MenuItem("Assets/Kvant/Wig/Convert To Template")]
        static void ConvertToTemplate()
        {
            var templates = new List<Object>();

            foreach (Mesh mesh in SelectedMeshes)
            {
                // Destination file path.
                var dirPath = Path.GetDirectoryName(AssetDatabase.GetAssetPath(mesh));
                var filename = (string.IsNullOrEmpty(mesh.name) ? "Wig" : " Wig") + ".asset";
                var assetPath = AssetDatabase.GenerateUniqueAssetPath(dirPath + "/" + filename);

                // Create a template asset.
                var asset = ScriptableObject.CreateInstance<WigTemplate>();
                asset.Initialize(mesh);
                AssetDatabase.CreateAsset(asset, assetPath);
                AssetDatabase.AddObjectToAsset(asset.foundation, asset);
                AssetDatabase.AddObjectToAsset(asset.mesh, asset);

                templates.Add(asset);
            }

            // Save the generated assets.
            AssetDatabase.SaveAssets();

            // Select the generated assets.
            EditorUtility.FocusProjectWindow();
            Selection.objects = templates.ToArray();
        }

        #endregion
    }
}
