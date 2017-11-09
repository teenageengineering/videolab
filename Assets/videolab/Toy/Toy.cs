using UnityEngine;
using UnityEditor;
using System.IO;
using System.Text.RegularExpressions;
using System;

namespace VideoLab
{
    [ExecuteInEditMode]
    [RequireComponent(typeof(Camera))]
    [AddComponentMenu("Image Effects/Toy")]
    public class Toy : MonoBehaviour
    {
        const string kTemplatePath = "Assets/videolab/Toy/Shader/Toy.shader";

        #region Public Properties

        TextAsset _prevShadertoyText;
        public TextAsset shadertoyText;

        public Vector2 _resolution = new Vector2(1280, 720);

        public Texture[] channels = new Texture[4];

        #endregion

        #region Private Properties

        [SerializeField, HideInInspector]
        Shader _shader;

        Material _material;

        int _frameCounter;

        Vector2 _mouseDown = Vector2.zero;

        void ConvertShader()
        {
            string stCode = shadertoyText.text;
            string stPath = AssetDatabase.GetAssetPath(shadertoyText);

            string template = File.ReadAllText(kTemplatePath);

            // Rename shader.
            template = template.Replace("Hidden/Toy", "Hidden/" + Path.GetFileNameWithoutExtension(stPath));

            // Insert Shadertoy code.
            template = template.Replace("fixed4 mainImage( float2 fragCoord ) { return fixed4(0, 0, 0, 0); }", stCode);

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

            _shader = (Shader)AssetDatabase.LoadAssetAtPath(shaderPath, typeof(Shader));
        }

        #endregion

        #region MonoBehaviour Functions

        void Start()
        {
            if (shadertoyText == null)
                _shader = (Shader)AssetDatabase.LoadAssetAtPath(kTemplatePath, typeof(Shader));

            _frameCounter = 0;
        }

        void Update()
        {
            if (Input.GetMouseButtonDown(0))
                _mouseDown = Input.mousePosition;
        }

        void OnRenderImage(RenderTexture source, RenderTexture destination)
        {
            if (_material == null)
            {
                _material = new Material(_shader);
                _material.hideFlags = HideFlags.DontSave;
            }

            _material.SetFloat("iFrame", _frameCounter);

            // TODO
            float[] channelTime = new float[4] {0, 0, 0, 0};
            _material.SetFloatArray("iChannelTime", channelTime);

            Vector4[] channelResolution = new Vector4[4];
            channelResolution[0] = (channels[0] != null) ? new Vector4(channels[0].width, channels[0].height): new Vector4();
            channelResolution[1] = (channels[1] != null) ? new Vector4(channels[1].width, channels[1].height): new Vector4();
            channelResolution[2] = (channels[2] != null) ? new Vector4(channels[2].width, channels[2].height): new Vector4();
            channelResolution[3] = (channels[3] != null) ? new Vector4(channels[3].width, channels[3].height): new Vector4();
            _material.SetVectorArray("iChannelResolution", channelResolution);

            Vector2 mousePos = Vector2.zero;
            if (Input.GetMouseButton(0))
                mousePos = Input.mousePosition;
            Vector4 mouse = new Vector4(mousePos.x, mousePos.y, _mouseDown.x, _mouseDown.y);

            DateTime now = DateTime.Now;
            Vector4 date = new Vector4(now.Year, now.Month, now.Day, now.Hour);
            _material.SetVector("iDate", date);
            _material.SetVector("iMouse", mouse);

            _material.SetFloat("iSampleRate", 44.100f);

            _material.SetTexture("iChannel0", channels[0]);
            _material.SetTexture("iChannel1", channels[1]);
            _material.SetTexture("iChannel2", channels[2]);
            _material.SetTexture("iChannel3", channels[3]);

            RenderTexture rt = source;
            rt = RenderTexture.GetTemporary((int)_resolution.x, (int)_resolution.y);

            Graphics.Blit(source, rt, _material);

            Graphics.Blit(rt, destination);

            RenderTexture.ReleaseTemporary(rt);

            ++_frameCounter;
        }

        void OnValidate()
        {
            if (shadertoyText != _prevShadertoyText)
            {
                _prevShadertoyText = shadertoyText;

                if (shadertoyText != null)
                    ConvertShader();
                else
                    _shader = (Shader)AssetDatabase.LoadAssetAtPath(kTemplatePath, typeof(Shader));
            }
        }

        #endregion
    }
}
