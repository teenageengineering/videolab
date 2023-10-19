using UnityEngine;
using System.Reflection;
using System.Collections.Generic;

namespace Klak.Wiring
{
    [AddComponentMenu("Klak/Wiring/Config/Cron")]
    public class Cron : NodeBase
    {
        [Tooltip("Path of the config file that will contain the events")]
        [SerializeField]
        public string _fileName = "cron.json";

        [Tooltip("When auto-save is enabled added events will be written immediately to the file.")]
        [SerializeField]
        public bool _autoSave = false;

        [Inlet]
        public float step
        {
            set
            {
                _steps++;
                if (_events == null)
                {
                    _events = CronMaster.GetEvents(_fileName);
                    _index = 0;
                    _nextTimestamp = null;
                }
                // Try to find an efficient way to trigger events on timestamp
                if (_events.Count > 0)
                {
                    if (_nextTimestamp == null)
                    {
                        _nextTimestamp = _events[0].timestamp;
                    }
                    if (_steps >= _nextTimestamp)
                    {
                        _valueEvent.Invoke(_events[_index].value);
                        if (_index < _events.Count-1)
                        {
                            _index++;
                            _nextTimestamp = _events[_index].timestamp;
                        } else {
                            _nextTimestamp = float.MaxValue;
                        }
                    }
                }
            }
        }

        [Inlet]
        public void reset()
        {
            _steps = 0;
            _index = 0;
            _nextTimestamp = null;
        }

        [Inlet]
        public float value
        {
            set
            {
                CronMaster.AddEvent(_fileName, _steps, value);
            }
        }

        [Inlet]
        public void saveEvents()
        {
            CronMaster.Save(_fileName, float.MaxValue);
        }

        [Inlet]
        public void deleteAllEvents()
        {
            _events = null;
            CronMaster.DeleteFile(_fileName);
        }

        [SerializeField, Outlet]
        FloatEvent _valueEvent = new FloatEvent();

        private float _steps = 0;
        private int _index = 0;

        private float? _nextTimestamp;
        private List<CronMaster.CronEvent> _events = null;

        void Update()
        {
            if (_autoSave)
            {
                CronMaster.Save(_fileName, Time.time);
            }
        }
    }
}