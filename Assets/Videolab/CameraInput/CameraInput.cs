using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Linq;

namespace VideoLab
{
    [RequireComponent(typeof(RawImage))]
    public class CameraInput : MonoBehaviour
    {
        #region Public members

        RawImage _image;

        public int numCameras {
            get {
                return WebCamTexture.devices.Length;
            }
        }

        int _curCameraIndex = -1;
        public int curCameraIndex {
            get { return _curCameraIndex; }
            set { 

                if (WebCamTexture.devices.Length == 0) 
                {
                    Debug.LogError("No cameras found");
                    return;
                }

                _curCameraIndex = (int)Mathf.Repeat(value, numCameras);

                WebCamDevice curCamera = WebCamTexture.devices.ElementAt(_curCameraIndex);
                WebCamTexture curTexture = new WebCamTexture(curCamera.name);

                curCameraTexture = curTexture;
            }
        }

        #endregion

        #region Private

        Vector3 rotationVector = new Vector3(0f, 0f, 0f);
        Rect defaultRect = new Rect(0f, 0f, 1f, 1f);
        Rect fixedRect = new Rect(0f, 1f, 1f, -1f);

        const int kValidWidth = 10;

        WebCamTexture _curCameraTexture;
        WebCamTexture curCameraTexture {
            get { return _curCameraTexture; }
            set {
                if (_curCameraTexture != null) {
                    _curCameraTexture.Stop();
                }

                _curCameraTexture = value;

                _image.texture = _curCameraTexture;
                _image.material.mainTexture = _curCameraTexture;

                _curCameraTexture.Play();
            }
        }

        #endregion
            
        #region Monobehaviour

        void Start()
        {
            _image = GetComponent<RawImage>();

            curCameraIndex = 0;
        }

        void Update()
        {
            if (curCameraTexture.width < kValidWidth) 
                return;

            rotationVector.z = -curCameraTexture.videoRotationAngle;
            _image.rectTransform.localEulerAngles = rotationVector;

            _image.uvRect = curCameraTexture.videoVerticallyMirrored ? fixedRect : defaultRect;
        }

        #endregion
    }
}