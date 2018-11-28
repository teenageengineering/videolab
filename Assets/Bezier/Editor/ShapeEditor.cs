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

        [MenuItem("GameObject/UI/Bezier/Shape")]
        static void CreateBezierShape(MenuCommand menuCommand)
        {
            Shape shape = Shape.CreateShape("Shape");

            GameObjectUtility.SetParentAndAlign(shape.gameObject, GetCanvas(menuCommand.context));

            Undo.RegisterCreatedObjectUndo(shape.gameObject, "Create " + shape.name);
            Selection.activeObject = shape.gameObject;
        }

        [MenuItem("GameObject/UI/Bezier/Rect")]
        static void CreateBezierRect(MenuCommand menuCommand)
        {
            Shape rect = Shape.CreateRect("Rect", new Vector2(100, 100), 0);

            GameObjectUtility.SetParentAndAlign(rect.gameObject, GetCanvas(menuCommand.context));

            Undo.RegisterCreatedObjectUndo(rect.gameObject, "Create " + rect.name);
            Selection.activeObject = rect.gameObject;
        }

        #endregion

        SerializedProperty subdivisions;
        SerializedProperty outline;
        SerializedProperty lineWidth;
        SerializedProperty closedPath;
        SerializedProperty snapToSize;

        void OnEnable()
        {
            subdivisions    = serializedObject.FindProperty("_subdivisions");
            outline         = serializedObject.FindProperty("_outline");
            lineWidth       = serializedObject.FindProperty("_lineWidth");
            closedPath      = serializedObject.FindProperty("_closedPath");
            snapToSize      = serializedObject.FindProperty("_snapToSize");
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
