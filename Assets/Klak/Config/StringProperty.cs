using UnityEngine;
using System.Reflection;

namespace Klak.Wiring
{
    [AddComponentMenu("Klak/Wiring/Config/String Property")]
    public class StringProperty : NodeBase
    {
        [Tooltip("The key is the name of the property that shows up in the config file when the files gets saved")]
        [SerializeField]
        public string _key = "myText";

        [Tooltip("Default value will be sent out if no property value can be found for the selected preset")]

        [SerializeField]
        public string _defaultValue;

        [Inlet]
        public string text
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
            _textEvent.Invoke(_value);
        }
        [SerializeField, Outlet]
        StringEvent _textEvent = new StringEvent();

        string _value = null;
        string _init = null;
        float? _preset = null;
        string _fileName = null;

        void LoadPreset(float preset)
        {
            if (_fileName != null)
            {
                this._preset = preset;
                _value = ConfigMaster.GetStringProperty(_fileName, preset, _key);
                if (_value == null) _value = _defaultValue;
                _init = _value;
                _textEvent.Invoke(_value);
            }
        }

        void SetValue(string value)
        {
            if (this._value != value)
            {
                this._value = value;
                if (_fileName != null && _preset != null)
                {
                    ConfigMaster.SetStringProperty(_fileName, (float)_preset, _key, _value);
                }
            }
        }

        void Update()
        {
            if (_value == null)
            {
                _value = _defaultValue;
                _textEvent.Invoke(_value);
            }
        }
    }
}