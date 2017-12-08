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

        MidiDestination destination {
            get { return (!_destination) ? MidiMaster.GetDestination() : _destination; }
        }

        #endregion

        #region Node I/O

        [Inlet]
        public void ClockTick()
        {
            destination.SendRealtime(MidiRealtime.Clock);
        }

        [Inlet]
        public void StartPlayback()
        {
            destination.SendRealtime(MidiRealtime.Start);
        }

        [Inlet]
        public void ResumePlayback()
        {
            destination.SendRealtime(MidiRealtime.Continue);
        }

        [Inlet]
        public void StopPlayback()
        {
            destination.SendRealtime(MidiRealtime.Stop);
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