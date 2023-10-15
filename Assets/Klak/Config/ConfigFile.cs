using UnityEngine;
using System.Reflection;

namespace Klak.Wiring
{
    [AddComponentMenu("Klak/Wiring/Config/Config File")]
    public class ConfigFile : NodeBase
    {
        [Tooltip("Path of the config file that will contain the connected properties.")]
        [SerializeField]
        public string _fileName = "config.json";

        [Tooltip("Default preset will be loaded on startup before any preset has been selected")]
        [SerializeField]
        public float _defaultPreset = 0;

        [Inlet]
        public float preset
        {
            set
            {
                if (value != _preset)
                {
                    _presetEvent.Invoke(value);
                    _preset = value;
                }
            }
        }

        [SerializeField, Outlet]
        FloatEvent _presetEvent = new FloatEvent();

        [SerializeField, Outlet]
        StringEvent _filenameEvent = new StringEvent();

        float _preset;

        void Awake()
        {
            _filenameEvent.Invoke(_fileName);
            _presetEvent.Invoke(_defaultPreset);
        }

        void Update()
        {
            ConfigMaster.Save(_fileName, Time.time);
        }
    }
}