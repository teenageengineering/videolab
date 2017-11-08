using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace VideoLab
{
    [CustomEditor(typeof(Shadertoy))]
    public class ShadertoyEditor : Editor {

        public override void OnInspectorGUI()
        {
            Shadertoy toy = target as Shadertoy;

            EditorGUI.BeginChangeCheck();
            TextAsset t = (TextAsset)EditorGUILayout.ObjectField("Shadertoy Source", toy.shadertoyText, typeof(TextAsset), false);
            if (EditorGUI.EndChangeCheck()) 
                toy.shadertoyText = t;

            EditorGUI.BeginChangeCheck();
            bool b = EditorGUILayout.Toggle("Aggressive Matching", toy.aggressiveMatching);
            if (EditorGUI.EndChangeCheck()) 
                toy.aggressiveMatching = b;

            serializedObject.Update();

            DrawPropertiesExcluding(serializedObject, new string[] {"m_Script", "_shadertoyText", "_aggressiveMatching"});

            serializedObject.ApplyModifiedProperties();
        }
    }
}
