using UnityEngine;
using UnityEngine.Video;

namespace Klak.Wiring
{
    [AddComponentMenu("Klak/Wiring/Output/Component/Video Player Out")]
    public class VideoPlayerOut : NodeBase
    {
        #region Editable properties

        [SerializeField]
        VideoPlayer _videoPlayer;

        #endregion

        #region Node I/O

        [Inlet]
        public float speed {
            set {
                if (!enabled || _videoPlayer == null) return;
                _videoPlayer.playbackSpeed = value;
            }
        }

        [Inlet]
        public float time {
            set {
                if (!enabled || _videoPlayer == null) return;
                _videoPlayer.time = value;
            }
        }

        [Inlet]
        public float normalizedTime {
            set {
                if (!enabled || _videoPlayer == null) return;
                _videoPlayer.time = _videoPlayer.clip.length * value;
            }
        }

        [Inlet]
        public void Play()
        {
            if (!enabled || _videoPlayer == null) return;
            if (_videoPlayer.isPlaying)
                _videoPlayer.time = 0;
            else
                _videoPlayer.Play();
        }

        [Inlet]
        public void Stop()
        {
            if (!enabled || _videoPlayer == null) return;
            _videoPlayer.Stop();
        }

        #endregion
    }
}
