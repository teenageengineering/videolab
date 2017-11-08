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
        FloatEvent _playPositionEvent = new FloatEvent();

        #endregion

        #region Private members

        bool _playing;
        int _ppgCounter;

        void OnRealtime(MidiRealtime realtimeMsg)
        {
            if (realtimeMsg == MidiRealtime.Clock)
            {
                ++_ppgCounter;
                _clockEvent.Invoke();
            }
            else if (realtimeMsg == MidiRealtime.Start)
            {
                _ppgCounter = 0;
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

        #endregion

        #region MonoBehaviour functions

        void OnEnable()
        {
            if (_source == null)
                _source = MidiMaster.GetSource();

            _source.realtimeDelegate += OnRealtime;
        }

        void OnDisable()
        {
            _source.realtimeDelegate -= OnRealtime;
        }

        void Update()
        {
            _playingEvent.Invoke((_playing) ? 1 : 0);   
            _playPositionEvent.Invoke(_ppgCounter);  
        }

        #endregion
    }
}
