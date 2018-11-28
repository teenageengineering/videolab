using UnityEngine;
using System;

namespace Videolab
{
    [RequireComponent(typeof(Camera))]
    [AddComponentMenu("Image Effects/Toy")]
    public class Toy : MonoBehaviour
    {
        #region Public Properties

        [SerializeField]
        TextAsset _shadertoyText;

        [SerializeField]
        Vector2 _resolution = new Vector2(1280, 720);

        [SerializeField]
        Texture[] _channels = new Texture[4];

        #endregion

        #region Private Properties

        [SerializeField, HideInInspector]
        Shader _shader;

        Material _material;

        int _frameCounter;

        Vector2 _mouseDown = Vector2.zero;

        #endregion

        #region MonoBehaviour Functions

        void Start()
        {
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
            channelResolution[0] = (_channels[0] != null) ? new Vector4(_channels[0].width, _channels[0].height): new Vector4();
            channelResolution[1] = (_channels[1] != null) ? new Vector4(_channels[1].width, _channels[1].height): new Vector4();
            channelResolution[2] = (_channels[2] != null) ? new Vector4(_channels[2].width, _channels[2].height): new Vector4();
            channelResolution[3] = (_channels[3] != null) ? new Vector4(_channels[3].width, _channels[3].height): new Vector4();
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

            _material.SetTexture("iChannel0", _channels[0]);
            _material.SetTexture("iChannel1", _channels[1]);
            _material.SetTexture("iChannel2", _channels[2]);
            _material.SetTexture("iChannel3", _channels[3]);

            RenderTexture rt = source;
            rt = RenderTexture.GetTemporary((int)_resolution.x, (int)_resolution.y);

            Graphics.Blit(source, rt, _material);

            Graphics.Blit(rt, destination);

            RenderTexture.ReleaseTemporary(rt);

            ++_frameCounter;
        }

        #endregion
    }
}
