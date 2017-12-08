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
        int _knobNumber = 0;

        [SerializeField]
        AnimationCurve _responseCurve = AnimationCurve.Linear(0, 0, 1, 1);

        [SerializeField]
        FloatInterpolator.Config _interpolator = new FloatInterpolator.Config(
            FloatInterpolator.Config.InterpolationType.DampedSpring, 30
        );

        #endregion

        #region Node I/O

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

            // Update the target value for the interpolator.
            _floatValue.targetValue = _responseCurve.Evaluate(inputValue);

            // Invoke the event in direct mode.
            if (!_interpolator.enabled)
                _valueEvent.Invoke(_floatValue.Step());

            // Detect an on-event and invoke the event.
            if (_lastInputValue < threshold && inputValue >= threshold)
                _onEvent.Invoke();

            // Detect an ooff-event and invoke the event.
            if (inputValue < threshold && _lastInputValue >= threshold)
                _offEvent.Invoke();

            _lastInputValue = inputValue;
        }

        MidiSource _prevSource;

        void SwitchSource()
        {
            if (_prevSource)
                _prevSource.knobDelegate -= OnKnobUpdate;

            if (!_source)
                _source = MidiMaster.GetSource();
                
            _source.knobDelegate += OnKnobUpdate;

            _prevSource = _source;
        }

        #endregion

        #region MonoBehaviour functions

        void OnDisable()
        {
            _source.knobDelegate -= OnKnobUpdate;
        }

        void Start()
        {
            SwitchSource();

            _lastInputValue = _source.GetKnob(_channel, _knobNumber, 0);

            _floatValue = new FloatInterpolator(
                _responseCurve.Evaluate(_lastInputValue), _interpolator
            );
        }

        void Update()
        {
            if (_source != _prevSource)
                SwitchSource();
            
            if (_interpolator.enabled)
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
