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
        FloatEvent _playPositionEvent = new FloatEvent();

        #endregion

        #region Private members

        bool _playing;
        int _ppqCounter;

        void OnRealtime(MidiRealtime realtimeMsg)
        {
            if (realtimeMsg == MidiRealtime.Clock)
            {
                _clockEvent.Invoke();

                if (_playing)
                    _playPositionEvent.Invoke((float)++_ppqCounter / 96);
            }
            else if (realtimeMsg == MidiRealtime.Start)
            {
                _ppqCounter = 0;
                _playing = true;
                _startEvent.Invoke();
            }
            else if (realtimeMsg == MidiRealtime.Continue)
            {
                _playing = true;
                _continueEvent.Invoke();
            }
            else if (realtimeMsg == MidiRealtime.Stop)
            {
                _playing = false;
                _stopEvent.Invoke();
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

            _playing = false;

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
