using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace Bezier
{
    [CanEditMultipleObjects, CustomEditor(typeof(Shape))]
    public class ShapeEditor : Editor
    {
        #region Menu

        public static GameObject GetCanvas(UnityEngine.Object menuContext)
        {
            GameObject parentGo = menuContext as GameObject;
            if (parentGo && parentGo.GetComponent<RectTransform>())
                return parentGo;

            GameObject canvasGo = new GameObject("Canvas");
            canvasGo.AddComponent<Canvas>();
            canvasGo.AddComponent<CanvasScaler>();
            canvasGo.AddComponent<GraphicRaycaster>();

            GameObject eventSysGo = new GameObject("EventSystem");
            eventSysGo.AddComponent<EventSystem>();
            eventSysGo.AddComponent<StandaloneInputModule>();

            return canvasGo;
        }

        public static GameObject CreateShape()
        {
            GameObject go = new GameObject("Bezier Shape");
            go.AddComponent<Image>();
            go.AddComponent<Shape>();

            return go;
        }

        [MenuItem("GameObject/UI/Bezier/Shape")]
        static void CreateBezierShape(MenuCommand menuCommand)
        {
            GameObject go = CreateShape();

            GameObjectUtility.SetParentAndAlign(go, GetCanvas(menuCommand.context));

            Undo.RegisterCreatedObjectUndo(go, "Create " + go.name);
            Selection.activeObject = go;
        }


        [MenuItem("GameObject/UI/Bezier/Rect")]
        static void CreateBezierRect(MenuCommand menuCommand)
        {
            GameObject go = CreateShape();

            Shape shape = go.GetComponent<Shape>();
            Vector3[] localCorners = new Vector3[4];
            shape.rectTransform.GetLocalCorners(localCorners);
            foreach (Vector3 corner in localCorners)
            {
                GameObject handleObj = new GameObject("Handle");
                Handle handle = handleObj.AddComponent<Handle>();
                handle.pos = corner;
                handleObj.transform.SetParent(go.transform, false);
            }

            GameObjectUtility.SetParentAndAlign(go, GetCanvas(menuCommand.context));

            Undo.RegisterCreatedObjectUndo(go, "Create " + go.name);
            Selection.activeObject = go;
        }

        #endregion

        SerializedProperty subdivisions;
        SerializedProperty outline;
        SerializedProperty lineWidth;
        SerializedProperty closedPath;
        SerializedProperty snapToSize;

        void OnEnable()
        {
            subdivisions    = serializedObject.FindProperty("subdivisions");
            outline         = serializedObject.FindProperty("outline");
            lineWidth       = serializedObject.FindProperty("_lineWidth");
            closedPath      = serializedObject.FindProperty("closedPath");
            snapToSize      = serializedObject.FindProperty("snapToSize");
        }

        public override void OnInspectorGUI()
        {
            Shape shape = target as Shape;
            Handle[] handles = shape.GetHandles();
            EditorGUILayout.LabelField(string.Format("{0} Points ({1} Triangles)", handles.Length, shape.numTris));

            serializedObject.Update();

            EditorGUILayout.Space();

            EditorGUILayout.PropertyField(subdivisions);

            EditorGUILayout.Space();

            EditorGUILayout.PropertyField(outline);
            if (outline.boolValue)
            {
                ++EditorGUI.indentLevel;
                EditorGUILayout.PropertyField(lineWidth);
                EditorGUILayout.PropertyField(closedPath);
                --EditorGUI.indentLevel;
            }

            EditorGUILayout.Space();

            EditorGUILayout.PropertyField(snapToSize);

            serializedObject.ApplyModifiedProperties();
        }

        public void OnSceneGUI() {

            Shape shape = target as Shape;
            if (!shape.enabled)
                return;

            float size = HandleUtility.GetHandleSize(shape.transform.localPosition);
            Quaternion shapeRotation = Tools.pivotRotation == PivotRotation.Local ? shape.transform.rotation : Quaternion.identity;

            Handles.color = Color.HSVToRGB(0, 0, 0.8f);

            Handle[] handles = shape.GetHandles();
            foreach (Handle handle in handles)
            {
                Vector3 p = shape.transform.TransformPoint(handle.transform.localPosition);
                if (Handles.Button(p, shapeRotation, size * 0.05f, size * 0.05f, Handles.DotHandleCap))
                {
                    Selection.activeTransform = handle.transform;
                    return;
                }
            }
        }
    }
}
