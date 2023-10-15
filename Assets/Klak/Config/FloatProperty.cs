using UnityEngine;
using System.Reflection;

namespace Klak.Wiring
{
    [AddComponentMenu("Klak/Wiring/Config/Float Property")]
    public class FloatProperty : NodeBase
    {
        [Tooltip("The key is the name of the property that shows up in the config file when the files gets saved")]
        [SerializeField]
        public string _key = "myNumber";

        [Tooltip("Default value will be sent out if no property value can be found for the selected preset")]

        [SerializeField]
        public float _defaultValue;

        [Inlet]
        public float value
        {
            set
            {
                SetValue(value);
            }
        }

        [Inlet]
        public float preset
        {
            set
            {
                if (value != _preset)
                {
                    LoadPreset(value);
                }
            }
        }

        [Inlet]
        public string fileName
        {
            set
            {
                this._fileName = value;
            }
        }

        [Inlet]
        public void revert()
        {
            SetValue(_init);
            _valueEvent.Invoke((float)_value);
        }

        [SerializeField, Outlet]
        FloatEvent _valueEvent = new FloatEvent();

        float? _value = null;
        float? _init = null;
        float? _preset = null;
        string _fileName = null;

        void LoadPreset(float preset)
        {
            if (_fileName != null)
            {
                this._preset = preset;
                _value = ConfigMaster.GetFloatProperty(_fileName, preset, _key);
                if (_value == null) _value = _defaultValue;
                _init = _value;
                _valueEvent.Invoke((float)_value);
            }
        }

        void SetValue(float? value)
        {
            if (this._value != value)
            {
                this._value = value;
                if (_fileName != null && _preset != null)
                {
                    ConfigMaster.SetFloatProperty(_fileName, (float)_preset, _key, (float)_value);
                }
            }
        }

        void Update()
        {
            if (_value == null)
            {
                _value = _defaultValue;
                _valueEvent.Invoke((float)_value);
            }
        }
    }
}