using UnityEngine;

namespace Videolab
{
    [ExecuteInEditMode]
    [RequireComponent(typeof(Camera))]
    [AddComponentMenu("Image Effects/Degrade")]
    public class Degrade : MonoBehaviour
    {
        #region Public Properties

        [SerializeField, Range(1, 8)]
        int _resolutionBits = 8;

        public float resolutionBits {
            get { return _resolutionBits; }
            set { _resolutionBits = (int)value; }
        }

        [SerializeField, Range(1, 8)]
        int _colorBits = 8;

        public float colorBits {
            get { return _colorBits; }
            set { _colorBits = (int)value; }
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

            _material.SetFloat("_colorDepth", Mathf.Pow(2, _colorBits - 1));

            RenderTexture rt = source;
            int d = (int)Mathf.Pow(2, 8 - _resolutionBits);
            rt = RenderTexture.GetTemporary(source.width / d, source.height / d);
            rt.filterMode = FilterMode.Point;

            Graphics.Blit(source, rt, _material);

            Graphics.Blit(rt, destination);

            RenderTexture.ReleaseTemporary(rt);
        }

        #endregion
    }
}