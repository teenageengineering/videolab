using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Linq;

namespace Videolab
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

        [SerializeField]
        float _targetFrameRate = 60;

        public float targetFrameRate {
            get { return _targetFrameRate; }
            set { _targetFrameRate = value; }
        }

        [SerializeField]
        bool _autoPlay;

        public bool autoPlay {
            get { return _autoPlay; }
            set { _autoPlay = value; }
        }

        public bool playing {
            get { return _camTexture && _camTexture.isPlaying; }
            set {
                if (!_camTexture || value == _camTexture.isPlaying)
                    return;

                if (value)
                {
                    _camTexture.requestedFPS = _targetFrameRate;
                    _camTexture.Play();
                }
                else
                    _camTexture.Stop();
            }
        }

        WebCamTexture _camTexture;
        public WebCamTexture camTexture {
            get { return _camTexture; }
        }

        bool _frameNeedsFixing;
        public bool isReady {
            get { return _frameNeedsFixing; }
        }

        #endregion

        #region Private

        void ReloadCamTexture()
        {
            playing = false;

            _camTexture = null;

            if (_deviceIndex >= 0 && _deviceIndex < WebCamTexture.devices.Length)
            {
                WebCamDevice device = WebCamTexture.devices.ElementAt(_deviceIndex);
                _camTexture = new WebCamTexture(device.name);
                _camTexture.requestedWidth = 1280;
                _camTexture.requestedHeight = 720;
                _camTexture.requestedFPS = _targetFrameRate;

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

            if (_autoPlay)
                playing = true;
        }

        ScreenOrientation _screenOrientation = ScreenOrientation.Unknown;

        void Update()
        {
            if (!_camTexture || _camTexture.width < 100)
                return;

            if (Screen.orientation != _screenOrientation)
            {
                _frameNeedsFixing = true;
                _screenOrientation = Screen.orientation;
            }

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
