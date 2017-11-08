using UnityEngine;
using Klak.Math;
using Klak.Wiring;
using MidiJack;

namespace Klak.Midi
{
    [AddComponentMenu("Klak/Wiring/Output/MIDI/Sequencer Out")]
    public class SequencerOut : NodeBase
    {
        #region Editable properties

        [SerializeField]
        MidiDestination _destination;

        #endregion

        #region Node I/O

        [Inlet]
        public void ClockTick()
        {
            _destination.SendRealtime(MidiRealtime.Clock);
        }

        [Inlet]
        public void StartPlayback()
        {
            _destination.SendRealtime(MidiRealtime.Start);
        }

        [Inlet]
        public void ResumePlayback()
        {
            _destination.SendRealtime(MidiRealtime.Continue);
        }

        [Inlet]
        public void StopPlayback()
        {
            _destination.SendRealtime(MidiRealtime.Stop);
        }

        #endregion

        #region MonoBehaviour functions

        void OnEnable()
        {
            if (_destination == null)
                _destination = MidiMaster.GetDestination();
        }

        #endregion
    }
}