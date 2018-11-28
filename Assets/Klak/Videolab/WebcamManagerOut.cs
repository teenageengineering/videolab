using UnityEngine;
using Videolab;

namespace Klak.Wiring
{
    [AddComponentMenu("Klak/Wiring/Output/Videolab/Webcam Manager Out")]
    public class WebcamManagerOut : NodeBase
    {
        #region Editable properties

        [SerializeField]
        WebcamManager _webcamManager;

        #endregion

        #region Node I/O

        [Inlet]
        public float deviceIndex {
            set {
                if (!enabled || _webcamManager == null) return;
                _webcamManager.deviceIndex = (int)Mathf.Repeat(value, WebCamTexture.devices.Length);
            }
        }

        [Inlet]
        public void Play() 
        {
            if (!enabled || _webcamManager == null) return;
            _webcamManager.playing = true;
        }

        [Inlet]
        public void Stop() 
        {
            if (!enabled || _webcamManager == null) return;
            _webcamManager.playing = false;
        }

        #endregion
    }
}
