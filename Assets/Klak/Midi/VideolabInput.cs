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
                masterVolume = value;
            }
            else if (id == MidiSysex.BatteryLevel)
            {
                batteryLevel = value;
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
