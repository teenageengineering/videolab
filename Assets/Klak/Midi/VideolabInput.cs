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

        // ivar only for debugging, no need to serialize
        float _activeTrack;
        public float activeTrack {
            get { return _activeTrack; }
            set {
                _activeTrack = value;
                _activeTrackEvent.Invoke(_activeTrack);
            }
        }

        float _activePattern;
        public float activePattern {
            get { return _activePattern; }
            set {
                _activePattern = value;
                _activePatternEvent.Invoke(_activePattern);
            }
        }

        float _activeProject;
        public float activeProject {
            get { return _activeProject; }
            set {
                _activeProject = value;
                _activeProjectEvent.Invoke(_activeProject);
            }
        }

        float _masterVolume;
        public float masterVolume {
            get { return _masterVolume; }
            set {
                _masterVolume = value;
                _masterVolumeEvent.Invoke(_masterVolume);
            }
        }

        float _batteryLevel;
        public float batteryLevel {
            get { return _batteryLevel; }
            set {
                _batteryLevel = value;
                _batteryLevelEvent.Invoke(_batteryLevel);
            }
        }

        float _tempo;
        public float tempo {
            get { return _tempo; }
            set {
                _tempo = value;
                _tempoEvent.Invoke(_tempo);
            }
        }

        #endregion

        #region Node I/O

        [SerializeField, Outlet]
        FloatEvent _activeTrackEvent = new FloatEvent();

        [SerializeField, Outlet]
        FloatEvent _activePatternEvent = new FloatEvent();

        [SerializeField, Outlet]
        FloatEvent _activeProjectEvent = new FloatEvent();

        [SerializeField, Outlet]
        FloatEvent _masterVolumeEvent = new FloatEvent();

        [SerializeField, Outlet]
        FloatEvent _batteryLevelEvent = new FloatEvent();

        [SerializeField, Outlet]
        FloatEvent _tempoEvent = new FloatEvent();

        #endregion

        #region Private members

        void OnSysex(MidiSysex id, int value)
        {
            if (id == MidiSysex.ActiveTrack)
            {
                activeTrack = value;
            }
            else if (id == MidiSysex.ActivePattern)
            {
                activePattern = value;
            }
            else if (id == MidiSysex.ActiveProject)
            {
                activeProject = value;
            }
            else if (id == MidiSysex.MasterVolume)
            {
                masterVolume = value / 127f;
            }
            else if (id == MidiSysex.BatteryLevel)
            {
                batteryLevel = value / 127f;
            }
            else if (id == MidiSysex.Tempo)
            {
                tempo = value;
            }
        }

        MidiSource _prevSource;

        bool _needsReset;

        void SwitchSource()
        {
            if (_prevSource)
                _prevSource.sysexDelegate -= OnSysex;

            if (!_source)
                _source = MidiMaster.GetSource();

            _source.sysexDelegate += OnSysex;

            _needsReset = true;

            _prevSource = _source;
        }

        #endregion

        #region MonoBehaviour functions

        void OnDisable()
        {
            if (_source)
                _source.sysexDelegate -= OnSysex;
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
                OnSysex(MidiSysex.ActiveTrack, _source.GetSysex(MidiSysex.ActiveTrack));
                OnSysex(MidiSysex.ActivePattern, _source.GetSysex(MidiSysex.ActivePattern));
                OnSysex(MidiSysex.ActiveProject, _source.GetSysex(MidiSysex.ActiveProject));
                OnSysex(MidiSysex.MasterVolume, _source.GetSysex(MidiSysex.MasterVolume));
                OnSysex(MidiSysex.BatteryLevel, _source.GetSysex(MidiSysex.BatteryLevel));
                OnSysex(MidiSysex.Tempo, _source.GetSysex(MidiSysex.Tempo));

                _needsReset = false;
            }
        }

        #endregion
    }
}
