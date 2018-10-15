//
// MidiKlak - MIDI extension for Klak
//
// Copyright (C) 2016 Keijiro Takahashi
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
//
using UnityEngine;
using Klak.Math;
using Klak.Wiring;
using MidiJack;

namespace Klak.Midi
{
    [AddComponentMenu("Klak/Wiring/Input/MIDI/Knob Input")]
    public class KnobInput : NodeBase
    {
        #region Editable properties

        [SerializeField]
        MidiSource _source;

        [SerializeField]
        MidiChannel _channel = MidiChannel.All;

        [SerializeField]
        [Tooltip("ModWheel is 1, generic knobs are 12 - 31.")]
        int _knobNumber = 0;

        [SerializeField]
        bool _isRelative = false;

        [SerializeField]
        AnimationCurve _responseCurve = AnimationCurve.Linear(0, 0, 1, 1);

        [SerializeField]
        FloatInterpolator.Config _interpolator = new FloatInterpolator.Config(
            FloatInterpolator.Config.InterpolationType.DampedSpring, 30
        );

        #endregion

        #region Node I/O

        [Inlet]
        public float channel {
            set {
                if (!enabled)
                    return;

                MidiChannel newChannel = (MidiChannel)Mathf.Clamp(value, (float)MidiChannel.Ch1, (float)MidiChannel.All);
                if (newChannel == _channel)
                    return;

                _channel = newChannel;
                ResetValue();
            }
        }

        [SerializeField, Outlet]
        VoidEvent _onEvent = new VoidEvent();

        [SerializeField, Outlet]
        VoidEvent _offEvent = new VoidEvent();

        [SerializeField, Outlet]
        FloatEvent _valueEvent = new FloatEvent();

        #endregion

        #region Private members

        FloatInterpolator _floatValue;
        float _lastInputValue;

        void OnKnobUpdate(MidiChannel channel, int knobNumber, float knobValue)
        {
            // Do nothing if the setting doesn't match.
            if (_channel != MidiChannel.All && channel != _channel) return;
            if (_knobNumber != knobNumber) return;
            // Do the actual process.
            DoKnobUpdate(knobValue);
        }

        void DoKnobUpdate(float inputValue)
        {
            const float threshold = 0.5f;

            if (_isRelative)
            {
                float relValue = (inputValue < 0.5f) ? 1 : -1;
                _valueEvent.Invoke(relValue / 127);
            }
            else
            {
                // Update the target value for the interpolator.
                _floatValue.targetValue = _responseCurve.Evaluate(inputValue);

                // Invoke the event in direct mode.
                if (!_interpolator.enabled)
                    _valueEvent.Invoke(_floatValue.Step());
            }

            // Detect an on-event and invoke the event.
            if (_lastInputValue < threshold && inputValue >= threshold)
                _onEvent.Invoke();

            // Detect an off-event and invoke the event.
            if (inputValue < threshold && _lastInputValue >= threshold)
                _offEvent.Invoke();

            _lastInputValue = inputValue;
        }

        void ResetValue()
        {
            _lastInputValue = _source.GetKnob(_channel, _knobNumber, 0);
            _floatValue.targetValue = _responseCurve.Evaluate(_lastInputValue);
        }

        MidiSource _prevSource;

        void SwitchSource()
        {
            if (_prevSource)
                _prevSource.knobDelegate -= OnKnobUpdate;

            if (!_source)
                _source = MidiMaster.GetSource();

            _source.knobDelegate += OnKnobUpdate;

            ResetValue();

            _prevSource = _source;
        }

        #endregion

        #region MonoBehaviour functions

        void Awake()
        {
            _floatValue = new FloatInterpolator(0, _interpolator);
        }

        void OnDisable()
        {
            if (_source)
                _source.knobDelegate -= OnKnobUpdate;
        }

        void OnEnable()
        {
            SwitchSource();
        }

        void Update()
        {
            if (_source != _prevSource)
                SwitchSource();

            if (!_isRelative && _interpolator.enabled)
                _valueEvent.Invoke(_floatValue.Step());
        }

        #endregion

        #if UNITY_EDITOR

        #region Editor interface

        public float debugInput {
            get { return _lastInputValue; }
            set { DoKnobUpdate(value); }
        }

        #endregion

        #endif
    }
}
