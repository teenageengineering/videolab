using UnityEngine;
using Klak.Wiring;
using MidiJack;

namespace Klak.Midi
{
    [AddComponentMenu("Klak/Wiring/Output/MIDI/Knob Out")]
    public class KnobOut : NodeBase
    {
        #region Editable properties

        [SerializeField]
        MidiDestination _destination;

        MidiDestination destination {
            get { 
                if (!_destination) 
                    _destination = MidiMaster.GetDestination();

                return _destination;
            }
        }

        [SerializeField]
        MidiChannel _channel = MidiChannel.All;

        [SerializeField]
        int _knobNumber;
        public int knobNumber {
            get { return _knobNumber; }
            set { _knobNumber = value; }
        }

        #endregion

        #region Node I/O

        [Inlet]
        public float channel {
            set {
                if (!enabled)
                    return;

                _channel = (MidiChannel)Mathf.Clamp(value, (float)MidiChannel.Ch1, (float)MidiChannel.All);
            }
        }

        [Inlet]
        public float absoluteValue {
            set {
                if (!enabled)
                    return;

                float newValue = Mathf.Clamp(value, 0, 1);
                destination.SendKnob(_channel, _knobNumber, newValue);
            }
        }

        [Inlet]
        public float delta {
            set {
                if (!enabled)
                    return;

                float newValue = Mathf.Clamp(value, -1, 1);
                float relValue = (newValue >= 0) ? 1f / 127 : 1f;
                for (float acc = 0; acc < Mathf.Abs(newValue); acc += 1f / 127)
                    destination.SendKnob(_channel, _knobNumber, relValue);
            }
        }

        #endregion

        #region MonoBehaviour functions

        void OnEnable()
        {
            if (!_destination) 
                _destination = MidiMaster.GetDestination();
        }

        #endregion
    }
}
