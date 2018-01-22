using UnityEngine;
using Klak.Wiring;
using MidiJack;

namespace Klak.Midi
{
    [AddComponentMenu("Klak/Wiring/Input/MIDI/Sequencer Input")]
    public class SequencerInput : NodeBase
    {
        #region Editable properties

        [SerializeField]
        MidiSource _source;

        #endregion

        #region Node I/O

        [SerializeField, Outlet]
        VoidEvent _clockEvent = new VoidEvent();

        [SerializeField, Outlet]
        VoidEvent _startEvent = new VoidEvent();

        [SerializeField, Outlet]
        VoidEvent _continueEvent = new VoidEvent();

        [SerializeField, Outlet]
        VoidEvent _stopEvent = new VoidEvent();

        [SerializeField, Outlet]
        FloatEvent _playingEvent = new FloatEvent();

        [SerializeField, Outlet]
        FloatEvent _stepEvent = new FloatEvent();

        #endregion

        #region Private members

        void OnRealtime(MidiRealtime realtimeMsg)
        {
            if (realtimeMsg == MidiRealtime.Clock)
            {
                _clockEvent.Invoke();
                _stepEvent.Invoke(1f / 96);
            }
            else if (realtimeMsg == MidiRealtime.Start)
            {
                _startEvent.Invoke();
                _playingEvent.Invoke(1);
            }
            else if (realtimeMsg == MidiRealtime.Continue)
            {
                _continueEvent.Invoke();
                _playingEvent.Invoke(1);
            }
            else if (realtimeMsg == MidiRealtime.Stop)
            {
                _stopEvent.Invoke();
                _playingEvent.Invoke(0);
            }
        }

        MidiSource _prevSource;

        void SwitchSource()
        {
            if (_prevSource)
                _prevSource.realtimeDelegate -= OnRealtime;

            if (!_source)
                _source = MidiMaster.GetSource();

            _source.realtimeDelegate += OnRealtime;

            _playingEvent.Invoke(0);

            _prevSource = _source;
        }

        #endregion

        #region MonoBehaviour functions

        void OnDisable()
        {
            _source.realtimeDelegate -= OnRealtime;
        }

        void Start()
        {
            SwitchSource();
        }

        void Update()
        {
            if (_source != _prevSource)
                SwitchSource();
        }

        #endregion
    }
}
