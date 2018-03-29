using UnityEngine;
using Klak.Wiring;
using MidiJack;

namespace Klak.Midi
{
    [AddComponentMenu("Klak/Wiring/Input/MIDI/Videolab Input")]
    public class VideolabInput : NodeBase
    {
        #region Editable properties

        [SerializeField]
        MidiSource _source;

        #endregion

        #region Node I/O

        [SerializeField, Outlet]
        FloatEvent _activeTrack = new FloatEvent();

        [SerializeField, Outlet]
        FloatEvent _activePattern = new FloatEvent();

        [SerializeField, Outlet]
        FloatEvent _activeProject = new FloatEvent();

        [SerializeField, Outlet]
        FloatEvent _masterVolume = new FloatEvent();

        [SerializeField, Outlet]
        FloatEvent _batteryLevel = new FloatEvent();

        #endregion

        #region Private members

        void OnSysex(MidiSysex id, int value)
        {
            if (id == MidiSysex.ActiveTrack)
            {
                _activeTrack.Invoke(value);
            }
            else if (id == MidiSysex.ActivePattern)
            {
                _activePattern.Invoke(value);
            }
            else if (id == MidiSysex.ActiveProject)
            {
                _activeProject.Invoke(value);
            }
            else if (id == MidiSysex.MasterVolume)
            {
                _masterVolume.Invoke(value);
            }
            else if (id == MidiSysex.BatteryLevel)
            {
                _batteryLevel.Invoke(value);
            }
        }

        MidiSource _prevSource;

        void SwitchSource()
        {
            if (_prevSource)
                _prevSource.sysexDelegate -= OnSysex;

            if (!_source)
                _source = MidiMaster.GetSource();

            _source.sysexDelegate += OnSysex;

            _prevSource = _source;
        }

        #endregion

        #region MonoBehaviour functions

        void OnDisable()
        {
            if (_source)
                _source.sysexDelegate -= OnSysex;
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
