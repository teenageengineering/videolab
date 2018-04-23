using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Linq;

namespace VideoLab
{
    public class WebcamManager : MonoBehaviour
    {
        #region Editable properties

        [SerializeField]
        int _deviceIndex;

        public int deviceIndex {
            get { return _deviceIndex; }
            set { 
                if (value == _deviceIndex)
                    return;
                
                _deviceIndex = value;

                ReloadCamTexture();
            }
        }

        [SerializeField]
        Material _material;

        public Material material {
            get { return _material; }
            set {
                if (value == _material)
                    return;
                
                if (_material)
                    _material.mainTexture = null;

                _material = value;

                if (_material)
                    _material.mainTexture = _camTexture;
            }
        }

        [SerializeField]
        RawImage _rawImage;

        public RawImage rawImage {
            get { return _rawImage; }
            set {
                if (value == _rawImage)
                    return;

                if (_rawImage)
                    _rawImage.texture = null;

                _rawImage = value;

                if (_rawImage)
                    _rawImage.texture = _camTexture;

                _frameNeedsFixing = true;
            }
        }

        [SerializeField]
        AspectRatioFitter _aspectRatioFitter;

        public AspectRatioFitter aspectRatioFitter {
            get { return _aspectRatioFitter; }
            set {
                if (value == _aspectRatioFitter)
                    return;
                
                _aspectRatioFitter = value;

                _frameNeedsFixing = true;
            }
        }

        public bool playing {
            get { return _camTexture && _camTexture.isPlaying; }
            set {
                if (!_camTexture || value == _camTexture.isPlaying)
                    return;

                if (value)
                    _camTexture.Play();
                else 
                    _camTexture.Stop();
            }
        }

        #endregion

        #region Private

        WebCamTexture _camTexture;

        bool _frameNeedsFixing;

        void ReloadCamTexture()
        {
            playing = false;

            _camTexture = null;

            if (_deviceIndex >= 0 && _deviceIndex < WebCamTexture.devices.Length)
            {
                WebCamDevice device = WebCamTexture.devices.ElementAt(_deviceIndex);
                WebCamTexture texture = new WebCamTexture(device.name);
                _camTexture = texture;

                if (_material)
                    _material.mainTexture = _camTexture;

                if (_rawImage)
                    _rawImage.texture = _camTexture;

                _frameNeedsFixing = true;
            }
        }

        #endregion

        #region Monobehaviour

        void Start()
        {
            ReloadCamTexture();
        }

        void Update()
        {
            if (!_camTexture || _camTexture.width < 100)
                return;

            if (_frameNeedsFixing)
            {
                if (_rawImage)
                {
                    _rawImage.rectTransform.localEulerAngles = new Vector3(0, 0, -_camTexture.videoRotationAngle);
                    _rawImage.uvRect = _camTexture.videoVerticallyMirrored ? new Rect(0, 1, 1, -1) : new Rect(0, 0, 1, 1);
                }

                if (_aspectRatioFitter)
                    _aspectRatioFitter.aspectRatio = (float)_camTexture.width / _camTexture.height;

                _frameNeedsFixing = false;
            }
        }

        #endregion
    }
}