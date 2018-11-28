using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using System.Text.RegularExpressions;

namespace Videolab
{
    [CanEditMultipleObjects]
    [CustomEditor(typeof(Toy))]
    public class ToyEditor : Editor 
    {
        SerializedProperty _shadertoyText;
        SerializedProperty _shader;

        string _templatePath;

        void OnEnable()
        {
            MonoScript thisScript = MonoScript.FromScriptableObject(this);
            string thisScriptPath = AssetDatabase.GetAssetPath(thisScript);
            _templatePath = Path.GetDirectoryName(thisScriptPath) + "/../Toy.shader";

            _shadertoyText  = serializedObject.FindProperty("_shadertoyText");
            _shader         = serializedObject.FindProperty("_shader");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(_shadertoyText);
            if (EditorGUI.EndChangeCheck())
            {
                ConvertShader((TextAsset)_shadertoyText.objectReferenceValue);
            }

            EditorGUILayout.Space();

            DrawPropertiesExcluding(serializedObject, new string[] {"m_Script", "_shadertoyText"});

            serializedObject.ApplyModifiedProperties();
        }

        void ConvertShader(TextAsset shadertoyText)
        {
            if (shadertoyText == null)
            {
                _shader.objectReferenceValue = (Shader)AssetDatabase.LoadAssetAtPath(_templatePath, typeof(Shader));
                return;
            }

            string stPath = AssetDatabase.GetAssetPath(shadertoyText);
            string template = File.ReadAllText(_templatePath);

            // Rename shader.
            template = template.Replace("Hidden/Toy", "Hidden/" + Path.GetFileNameWithoutExtension(stPath));

            // Insert Shadertoy code.
            template = template.Replace("fixed4 mainImage( float2 fragCoord ) { return fixed4(0, 0, 0, 0); }", shadertoyText.text);

            // Rewrite entry point signature.
            template = template.Replace("void mainImage( out vec4 fragColor, in vec2 fragCoord )", "fixed4 mainImage( float2 fragCoord )");
            template = template.Replace("fragColor =", "return");

            template = template.Replace("gl_FragCoord", "fragCoord");
            template = template.Replace("iResolution", "_ScreenParams");
            template = template.Replace("iTime", "_Time.y");
            template = template.Replace("iTimeDelta", "unity_DeltaTime.x");

            template = template.Replace("const", "static const");

            template = template.Replace("mix(", "lerp(");
            template = template.Replace("fract(", "frac(");
            template = template.Replace("mod(", "fmod(");
            template = template.Replace("dFdx(", "ddx(");
            template = template.Replace("dFdy(", "ddy(");
            template = template.Replace("fma(", "mad(");
            template = template.Replace("texture(", "tex2D(");
            template = template.Replace("texture2D(", "tex2D(");

            template = Regex.Replace(template, @"vec([2-4])", "float$1");
            template = Regex.Replace(template, @"ivec([2-4])", "int$1");
            template = Regex.Replace(template, @"bvec([2-4])", "bool$1");
            template = Regex.Replace(template, @"float4\(([^(,]+?)\)", "float4($1, $1, $1, $1)");
            template = Regex.Replace(template, @"float3\(([^(,]+?)\)", "float3($1, $1, $1)");
            template = Regex.Replace(template, @"float2\(([^(,]+?)\)", "float2($1, $1)");

            template = Regex.Replace(template, @"mat([2-4])", "float$1x$1");
            template = Regex.Replace(template, @"([\w.]+)\s*\*=\s*([^;]+)", "$1 = mul($2, $1)");

            foreach (Match match in Regex.Matches(template, @"textureLod\((.*)\)"))
            {
                var args = Regex.Matches(match.Groups[1].Value, @"(?:[^,()]+((?:\((?>[^()]+|\((?<open>)|\)(?<-open>))*\)))*)+");
                string rep = String.Format("tex2Dlod({0}, float4(({1}).x, ({1}).y, 0., {2}))", args[0], args[1], args[2]);
                template = template.Replace(match.Value, rep);
            }

            foreach (Match match in Regex.Matches(template, @"atan\((.*)\)"))
            {
                var args = Regex.Matches(match.Groups[1].Value, @"(?:[^,()]+((?:\((?>[^()]+|\((?<open>)|\)(?<-open>))*\)))*)+");
                string rep = String.Format("atan2({1}, {0})", args[0], args[1]);
                template = template.Replace(match.Value, rep);
            }

            string shaderPath = Path.GetDirectoryName(stPath) + "/" + Path.GetFileNameWithoutExtension(stPath) + ".shader";
            File.WriteAllText(shaderPath, template);
            AssetDatabase.Refresh();

            _shader.objectReferenceValue = (Shader)AssetDatabase.LoadAssetAtPath(shaderPath, typeof(Shader));
        }
    }
}
