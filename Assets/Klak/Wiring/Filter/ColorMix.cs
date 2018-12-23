using UnityEngine;

namespace Klak.Wiring
{
    [AddComponentMenu("Klak/Wiring/Mixing/Color Mix")]
    public class ColorMix : NodeBase
    {
        #region Editable properties

        #endregion

        #region Node I/O

        [Inlet]
        public Color colorA {
            set {
                if (!enabled) return;
                _colorA = value;
                _outputEvent.Invoke(MixValues());
            }
        }

        [Inlet]
        public Color colorB {
            set {
                if (!enabled) return;
                _colorB = value;
                _outputEvent.Invoke(MixValues());
            }
        }

        [Inlet]
        public float mix {
            set {
                if (!enabled) return;
                _mix = value;
                _outputEvent.Invoke(MixValues());
            }
        }

        [SerializeField, Outlet]
        ColorEvent _outputEvent = new ColorEvent();

        #endregion

        #region Private members

        Color _colorA = Color.black;
        Color _colorB = Color.white;
        float _mix;

        Color MixValues()
        {
            return (1 - _mix) * _colorA + _mix * _colorB;
        }

        #endregion
    }
}
