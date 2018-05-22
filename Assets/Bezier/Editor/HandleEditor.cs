using UnityEditor;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Bezier
{
    [CanEditMultipleObjects, CustomEditor(typeof(Handle))]
    public class HandleEditor : Editor
    {
        #region Menu

        public static GameObject GetShape(UnityEngine.Object menuContext)
        {
            GameObject parentGo = menuContext as GameObject;
            if (parentGo && parentGo.GetComponent<Shape>())
                return parentGo;
            
            GameObject shapeGo = ShapeEditor.CreateShape();
            GameObjectUtility.SetParentAndAlign(shapeGo, ShapeEditor.GetCanvas(menuContext));

            return shapeGo;
        }

        [MenuItem("GameObject/UI/Bezier/Handle")]
        static void CreateBezierHandle(MenuCommand menuCommand)
        {
            GameObject go = new GameObject("Handle");
            go.AddComponent<Handle>();

            GameObjectUtility.SetParentAndAlign(go, GetShape(menuCommand.context));

            Undo.RegisterCreatedObjectUndo(go, "Create " + go.name);
            Selection.activeObject = go;
        }

        #endregion

        int selectedHandle = 0;

        static Color[] modeColors = {
            Color.yellow,
            Color.cyan,
            Color.magenta
        };

        SerializedProperty mode;
        SerializedProperty cornerRadius;
        SerializedProperty control1;
        SerializedProperty control2;

        void OnEnable()
        {
            mode            = serializedObject.FindProperty("mode");
            cornerRadius    = serializedObject.FindProperty("_cornerRadius");
            control1        = serializedObject.FindProperty("_control1");
            control2        = serializedObject.FindProperty("_control2");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.PropertyField(mode);

            if (mode.enumValueIndex == (int)Handle.Mode.Rounded)
            {
                EditorGUILayout.PropertyField(cornerRadius);
                selectedHandle = 0;
                SceneView.RepaintAll();
            }
            else
            {
                EditorGUI.BeginChangeCheck();
                EditorGUILayout.PropertyField(control1);
                if (EditorGUI.EndChangeCheck())
                {
                    selectedHandle = 1;
                    SceneView.RepaintAll();
                }

                EditorGUI.BeginChangeCheck();
                EditorGUILayout.PropertyField(control2);
                if (EditorGUI.EndChangeCheck())
                {
                    selectedHandle = 2;
                    SceneView.RepaintAll();
                }
            }

            serializedObject.ApplyModifiedProperties();
        }

        public void OnSceneGUI() {

            Handle handle = target as Handle;
            if (!handle.enabled)
                return;

            float size = HandleUtility.GetHandleSize(handle.pos);
            Transform handleTransform = handle.transform;
            Quaternion handleRotation = Tools.pivotRotation == PivotRotation.Local ? handleTransform.rotation : Quaternion.identity;

            Event e = Event.current;
            bool deletePressed = (e.type == EventType.KeyDown && (Event.current.keyCode == KeyCode.Backspace));

            Vector3 p = handleTransform.TransformPoint(Vector3.zero);
            Vector3 c1 = handleTransform.TransformPoint(handle.control1);
            Vector3 c2 = handleTransform.TransformPoint(handle.control2);

            if (handle.mode != Handle.Mode.Rounded)
            {
                Handles.color = modeColors[(int)handle.mode];

                Handles.DrawLine(p, c1);
                if (Handles.Button(c1, handleRotation, size * 0.05f, size * 0.05f, Handles.DotHandleCap))
                    selectedHandle = 1;

                Handles.DrawLine(p, c2);
                if (Handles.Button(c2, handleRotation, size * 0.05f, size * 0.05f, Handles.DotHandleCap))
                    selectedHandle = 2;
            }

            if (selectedHandle == 0)
            {
                // use standard editor tools
                Tools.hidden = false;
            }
            else
            {
                Tools.hidden = true;

                Handles.color = Color.HSVToRGB(0, 0, 0.8f);
                if (Handles.Button(p, handleRotation, size * 0.05f, size * 0.05f, Handles.DotHandleCap))
                    selectedHandle = 0;

                if (selectedHandle == 1)
                {
                    if (deletePressed)
                    {
                        Undo.RecordObject(handle, "Delete Control 1");

                        handle.mode = Handle.Mode.Free;
                        handle.control1 = Vector3.zero;
                        EditorUtility.SetDirty(handle);

                        selectedHandle = 0;
                    }

                    EditorGUI.BeginChangeCheck();
                    c1 = Handles.DoPositionHandle(c1, handleRotation);
                    if (EditorGUI.EndChangeCheck())
                    {
                        Undo.RecordObject(handle, "Move Control 1");

                        handle.control1 = handleTransform.InverseTransformPoint(c1);
                        EditorUtility.SetDirty(handle);
                    }
                }
                else
                {
                    if (deletePressed)
                    {
                        Undo.RecordObject(handle, "Delete Control 2");

                        handle.mode = Handle.Mode.Free;
                        handle.control2 = Vector3.zero;
                        EditorUtility.SetDirty(handle);

                        selectedHandle = 0;
                    }

                    EditorGUI.BeginChangeCheck();
                    c2 = Handles.DoPositionHandle(c2, handleRotation);
                    if (EditorGUI.EndChangeCheck())
                    {
                        Undo.RecordObject(handle, "Move Control 2");

                        handle.control2 = handleTransform.InverseTransformPoint(c2);
                        EditorUtility.SetDirty(handle);
                    }
                }
            }
        }
    }
}