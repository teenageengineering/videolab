using UnityEngine;
using System.Reflection;

namespace Klak.Wiring
{
    [AddComponentMenu("Klak/Wiring/Config/Preset Manager")]
    public class PresetManager : NodeBase
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
        public float loadPreset
        {
            set
            {
                if (value != _preset)
                {
                    if (!_autoSave)
                    {
                        PresetMaster.DiscardChanges(_fileName, value);
                    }
                    _presetEvent.Invoke(value);
                    _preset = value;
                }
            }
        }

        [Inlet]
        public void savePreset()
        {
            PresetMaster.Save(_fileName, float.MaxValue);
        }
        

        [Inlet]
        public void deletePreset()
        {
            PresetMaster.DeletePreset(_fileName, _preset);
            _presetEvent.Invoke(_preset);
        }

        [Inlet]
        public void deleteAllPresets()
        {
            PresetMaster.DeleteFile(_fileName);
            _presetEvent.Invoke(_preset);
        }

        [Inlet]
        public void discardChanges()
        {
            PresetMaster.DiscardChanges(_fileName, _preset);
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
            _preset = _defaultPreset;
            _presetEvent.Invoke(_defaultPreset);
        }

        void Update()
        {
            if (_autoSave)
            {
                PresetMaster.Save(_fileName, Time.time);
            }
        }
    }
}