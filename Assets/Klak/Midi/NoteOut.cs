using UnityEngine;
using Klak.Math;
using Klak.Wiring;
using MidiJack;

namespace Klak.Midi
{
    [AddComponentMenu("Klak/Wiring/Output/MIDI/Note Out")]
    public class NoteOut : NodeBase
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

        #endregion

        #region Private members

        int _noteNumber;
        float _velocity;

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
        public float noteNumber {
            set {
                _noteNumber = (int)value;
            }
        }

        [Inlet]
        public float velocity {
            set {
                _velocity = value;
            }
        }

        [Inlet]
        public void NoteOn()
        {
            destination.SendKeyDown(_channel, _noteNumber, _velocity);
        }

        [Inlet]
        public void NoteOff()
        {
            destination.SendKeyUp(_channel, _noteNumber);
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