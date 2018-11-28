using UnityEngine;

namespace Videolab
{
    [ExecuteInEditMode]
    [RequireComponent(typeof(Camera))]
    [AddComponentMenu("Image Effects/Halftone")]
    public class Halftone : MonoBehaviour
    {
        #region Public Properties

        [SerializeField, Range(0, 2)]
        float _pointSize = 1;

        public float pointSize {
            get { return _pointSize; }
            set { _pointSize = value; }
        }

        [SerializeField, Range(0, 50)]
        float _frequency = 30;

        public float frequency {
            get { return _frequency; }
            set { _frequency = value; }
        }

        // color filters

        [SerializeField, Range(0, 90)]
        float _filterAngleA = 75;

        public float filterAngleA {
            get { return _filterAngleA; }
            set { _filterAngleA = value; }
        }

        [SerializeField, Range(0, 90)]
        float _filterAngleB = 45;

        public float filterAngleB {
            get { return _filterAngleB; }
            set { _filterAngleB = value; }
        }

        [SerializeField, Range(0, 90)]
        float _filterAngleC = 15;

        public float filterAngleC {
            get { return _filterAngleC; }
            set { _filterAngleC = value; }
        }

        [SerializeField]
        bool _monochrome = false;

        public bool monochrome {
            get { return _monochrome; }
            set { _monochrome = value; }
        }

        [SerializeField]
        bool _invert = false;

        public bool invert {
            get { return _invert; }
            set { _invert = value; }
        }

        // settings

        [SerializeField]
        bool _downSample = true;

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

            _material.SetFloat("_pointSize", _pointSize);
            _material.SetFloat("_frequency", _frequency);

            _material.SetVector("_sines", new Vector3(
                Mathf.Sin(_filterAngleA * Mathf.Deg2Rad), 
                Mathf.Sin(filterAngleB * Mathf.Deg2Rad), 
                Mathf.Sin(_filterAngleC * Mathf.Deg2Rad)
            ));
            _material.SetVector("_cosines", new Vector3(
                Mathf.Cos(_filterAngleA * Mathf.Deg2Rad), 
                Mathf.Cos(filterAngleB * Mathf.Deg2Rad), 
                Mathf.Cos(_filterAngleC * Mathf.Deg2Rad)
            ));

            _material.SetFloat("_mono", (_monochrome) ? 1.0f : 0.0f);
            _material.SetFloat("_invert", (_invert) ? 1.0f : 0.0f);

            RenderTexture rt = source;
            if (_downSample)
            {
                rt = RenderTexture.GetTemporary(source.width / 8, source.height / 8);
                Graphics.Blit(source, rt);
            }

            Graphics.Blit(rt, destination, _material);

            if (_downSample)
                RenderTexture.ReleaseTemporary(rt);
        }

        #endregion
    }
}