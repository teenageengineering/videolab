using UnityEngine;
using UnityEngine.Playables;

namespace Klak.Wiring
{
    [AddComponentMenu("Klak/Wiring/Output/Component/Playable Director Out")]
    public class PlayableDirectorOut : NodeBase
    {
        #region Editable properties

        [SerializeField]
        PlayableDirector _playableDirector;

        #endregion

        #region Node I/O

        [Inlet]
        public float speed {
            set {
                if (!enabled || _playableDirector == null) return;
                var graph = _playableDirector.playableGraph;
                if (!graph.IsValid()) return;
                for (var i = 0; i < graph.GetRootPlayableCount(); i++)
                    graph.GetRootPlayable(i).SetSpeed(value);
            }
        }

        [Inlet]
        public float time {
            set {
                if (!enabled || _playableDirector == null) return;
                _playableDirector.time = value;
            }
        }

        [Inlet]
        public float normalizedTime {
            set {
                if (!enabled || _playableDirector == null) return;
                _playableDirector.time = _playableDirector.duration * value;
            }
        }

        [Inlet]
        public void Play()
        {
            if (!enabled || _playableDirector == null) return;
            _playableDirector.Play();
            _playableDirector.time = 0;
        }

        [Inlet]
        public void Stop()
        {
            if (!enabled || _playableDirector == null) return;
            _playableDirector.Stop();
        }

        #endregion
    }
}
