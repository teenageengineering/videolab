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

                if (_source.IsPlaying())
                    _stepEvent.Invoke(1f / 24);
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

        bool _needsReset;

        void SwitchSource()
        {
            if (_prevSource)
                _prevSource.realtimeDelegate -= OnRealtime;

            if (!_source)
                _source = MidiMaster.GetSource();

            _source.realtimeDelegate += OnRealtime;

            _needsReset = true;

            _prevSource = _source;
        }

        #endregion

        #region MonoBehaviour functions

        void OnDisable()
        {
            if (_source)
                _source.realtimeDelegate -= OnRealtime;
        }

        void OnEnable()
        {
            SwitchSource();
        }

        void Update()
        {
            if (_source != _prevSource)
                SwitchSource();

            if (_needsReset)
            {
                if (_source.IsPlaying())
                    OnRealtime(MidiRealtime.Start);
                else
                    OnRealtime(MidiRealtime.Stop);

                _needsReset = false;
            }
        }

        #endregion
    }
}
