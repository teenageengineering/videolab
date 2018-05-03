using UnityEngine;

namespace VideoLab
{
    [ExecuteInEditMode]
    [RequireComponent(typeof(Camera))]
    [AddComponentMenu("Image Effects/Photomatic")]
    public class PhotomaticEffect : MonoBehaviour
    {
        #region Public Properties

        [SerializeField, Range(0, 1)]
        float _hue = 0;

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

            _material.SetVector("_hsbc", new Vector4(_hue, _saturation, _brightness, _contrast));

            Graphics.Blit(source, destination, _material);
        }

        #endregion
    }
}