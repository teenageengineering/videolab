using UnityEngine;

namespace Videolab
{
    [ExecuteInEditMode]
    [RequireComponent(typeof(Camera))]
    [AddComponentMenu("Image Effects/Photomatic")]
    public class PhotomaticEffect : MonoBehaviour
    {
        #region Public Properties

        [SerializeField, Range(0, 1)]
        float _hue = 0.5f;

        public float hue {
            get { return _hue; }
            set { _hue = value; }
        }

        [SerializeField, Range(0, 1)]
        float _saturation = 0.5f;

        public float saturation {
            get { return _saturation; }
            set { _saturation = value; }
        }

        [SerializeField, Range(0, 1)]
        float _brightness = 0.5f;

        public float brightness {
            get { return _brightness; }
            set { _brightness = value; }
        }

        [SerializeField, Range(0, 1)]
        float _contrast = 0.5f;

        public float contrast {
            get { return _contrast; }
            set { _contrast = value; }
        }

        [SerializeField]
        bool _invert;

        public bool invert {
            get { return _invert; }
            set { _invert = value; }
        }

        [SerializeField]
        bool _mirrorX;

        public bool mirrorX {
            get { return _mirrorX; }
            set { _mirrorX = value; }
        }

        [SerializeField]
        bool _mirrorY;

        public bool mirrorY {
            get { return _mirrorY; }
            set { _mirrorY = value; }
        }

        [SerializeField, Range(1, 4)]
        float _zoom = 1;

        public float zoom {
            get { return _zoom; }
            set { _zoom = value; }
        }

        [SerializeField]
        Color _colorMask = Color.white;

        public Color colorMask {
            get { return _colorMask; }
            set { _colorMask = value; }
        }

        #endregion

        #region Private Properties

        [SerializeField, HideInInspector]
        Shader _shader;

        Material _material;

        #endregion

        #region MonoBehaviour Functions

        void OnRenderImage(RenderTexture source, RenderTexture destination)
        {
            if (_material == null)
            {
                _material = new Material(_shader);
                _material.hideFlags = HideFlags.DontSave;
            }

            float hue = Mathf.Repeat(_hue - 0.5f, 1);
            _material.SetVector("_hsbc", new Vector4(hue, _saturation, _brightness, _contrast));
            _material.SetVector("_fx", new Vector4(_mirrorX ? 1 : 0, _mirrorY ? 1 : 0, _invert ? 1 : 0, _zoom));
            _material.SetColor("_colorMask", _colorMask);

            Graphics.Blit(source, destination, _material);
        }

        #endregion
    }
}