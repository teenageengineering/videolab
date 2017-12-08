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
            get { return (!_destination) ? MidiMaster.GetDestination() : _destination; }
        }

        [SerializeField]
        MidiChannel _channel = MidiChannel.All;

        [SerializeField]
        int _knobNumber;

        #endregion

        #region Private members

        float _currentAbsoluteValue = 0;

        #endregion

        #region Node I/O

        [Inlet]
        public float absoluteValue {
            set {
                if (!enabled)
                    return;
                
                float newValue = Mathf.Clamp(value, 0, 1);
                destination.SendKnob(_channel, _knobNumber, newValue);
                _currentAbsoluteValue = newValue;
            }
        }

        [Inlet]
        public float delta {
            set {
                if (!enabled)
                    return;

                float newValue = Mathf.Clamp(_currentAbsoluteValue + value, 0, 1);
                destination.SendKnob(_channel, _knobNumber, newValue);
                _currentAbsoluteValue = newValue;
            }
        }

        #endregion
    }
}
