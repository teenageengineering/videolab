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

        [Tooltip("When auto-save is enabled all property changes will immediately be written to the preset.")]
        [SerializeField]
        public bool _autoSave = false;

        [Inlet]
        public float preset
        {
            set
            {
                if (value != _preset)
                {
                    if (!_autoSave)
                    {
                        ConfigMaster.Revert(_fileName, value);
                    }
                    _presetEvent.Invoke(value);
                    _preset = value;
                }
            }
        }

        [Inlet]
        public void saveNow()
        {
            ConfigMaster.Save(_fileName, float.MaxValue);
        }

        [Inlet]
        public void revert()
        {
            ConfigMaster.Revert(_fileName, _preset);
            _presetEvent.Invoke(_preset);
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
            if (_autoSave)
            {
                ConfigMaster.Save(_fileName, Time.time);
            }
        }
    }
}