using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using System;

namespace Bezier
{
    [CanEditMultipleObjects, CustomEditor(typeof(Group))]
    public class GroupEditor : Editor
    {
        #region Menu item

        [MenuItem("GameObject/UI/Bezier/Group")]
        static void CreateBezierGroup(MenuCommand menuCommand)
        {
            // prevent executing repeatedly
            if (Selection.gameObjects.Length > 1)
            if (menuCommand.context != Selection.gameObjects[0])
                return;

            if (Selection.gameObjects.Length > 0)
            {
                GameObject groupObj = new GameObject("Bezier Group");
                groupObj.AddComponent<RectTransform>();

                GameObject first = Selection.gameObjects[0];
                int index = first.transform.GetSiblingIndex();

                GameObjectUtility.SetParentAndAlign(groupObj, first.transform.parent.gameObject);
                groupObj.transform.SetSiblingIndex(index);

                foreach (GameObject go in Selection.gameObjects)
                    go.transform.SetParent(groupObj.transform);

                Group group = groupObj.AddComponent<Group>();
                group.EnvelopeChildren();

                Undo.RegisterCreatedObjectUndo(groupObj, "Create " + groupObj.name);
            }
        }

        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            Group group = target as Group;

            EditorGUILayout.BeginHorizontal();

            if(GUILayout.Button("Snap To Size"))
            {
                group.StretchChildren();
            }

            if(GUILayout.Button("Free"))
            {
                group.FreeChildren();
            }

            EditorGUILayout.EndHorizontal();
        }

        #endregion
    }
}
