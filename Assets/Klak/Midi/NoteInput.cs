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
    [AddComponentMenu("Klak/Wiring/Input/MIDI/Note Input")]
    public class NoteInput : NodeBase
    {
        #region Editable properties

        public enum NoteFilter {
            Off, NoteName, NoteNumber
        }

        public enum NoteName {
            C, CSharp, D, DSharp, E, F, FSharp, G, GSharp, A, ASharp, B
        }

        [SerializeField]
        MidiSource _source;

        MidiSource source {
            get { 
                if (_source == null)
                    return MidiMaster.GetSource();

                return _source;
            }
        }

        [SerializeField]
        MidiChannel _channel = MidiChannel.All;

        [SerializeField]
        NoteFilter _noteFilter = NoteFilter.Off;

        [SerializeField]
        NoteName _noteName;

        [SerializeField]
        int _lowestNote = 60; // C4

        [SerializeField]
        int _highestNote = 60; // C4

        [SerializeField]
        AnimationCurve _velocityCurve = AnimationCurve.Linear(0, 0, 1, 1);

        [SerializeField]
        float _offValue = 0.0f;

        [SerializeField]
        float _onValue = 1.0f;

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

                _channel = (MidiChannel)Mathf.Clamp(value, (float)MidiChannel.Ch1, (float)MidiChannel.All);
            }
        }

        [SerializeField, Outlet]
        VoidEvent _noteOnEvent = new VoidEvent();

        [SerializeField, Outlet]
        FloatEvent _noteOnVelocityEvent = new FloatEvent();

        [SerializeField, Outlet]
        VoidEvent _noteOffEvent = new VoidEvent();

        [SerializeField, Outlet]
        FloatEvent _valueEvent = new FloatEvent();

        #endregion

        #region Private members

        FloatInterpolator _floatValue;

        bool CompareNoteToName(int number, NoteName name)
        {
            return (number % 12) == (int)name;
        }

        bool FilterNote(MidiChannel channel, int note)
        {
            if (_channel != MidiChannel.All && channel != _channel) return false;
            if (_noteFilter == NoteFilter.Off) return true;
            if (_noteFilter == NoteFilter.NoteName)
                return CompareNoteToName(note, _noteName);
            else // NoteFilter.Number
                return _lowestNote <= note && note <= _highestNote;
        }

        void NoteOn(MidiChannel channel, int note, float velocity)
        {
            if (!FilterNote(channel, note)) return;

            velocity = _velocityCurve.Evaluate(velocity);

            _noteOnEvent.Invoke();
            _noteOnVelocityEvent.Invoke(velocity);

            _floatValue.targetValue = _onValue * velocity;
        }

        void NoteOff(MidiChannel channel, int note)
        {
            if (!FilterNote(channel, note)) return;

            _noteOffEvent.Invoke();

            _floatValue.targetValue = _offValue;
        }

        MidiSource _prevSource;

        void SwitchSource()
        {
            if (_prevSource)
            {
                _prevSource.noteOnDelegate -= NoteOn;
                _prevSource.noteOffDelegate -= NoteOff;
            }

            if (!_source)
                _source = MidiMaster.GetSource();

            _source.noteOnDelegate += NoteOn;
            _source.noteOffDelegate += NoteOff;

            _prevSource = _source;
        }

        #endregion

        #region MonoBehaviour functions

        void Awake()
        {
            _floatValue = new FloatInterpolator(_offValue, _interpolator);
        }

        void OnDisable()
        {
            if (_source)
            {
                _source.noteOnDelegate -= NoteOn;
                _source.noteOffDelegate -= NoteOff;
            }
        }

        void OnEnable()
        {
            SwitchSource();
        }

        void Update()
        {
            if (_source != _prevSource)
                SwitchSource();
            
            _valueEvent.Invoke(_floatValue.Step());
        }

        #endregion

        #if UNITY_EDITOR

        #region Editor Interface

        bool _debugInput;

        int debugNote
        {
            get {
                if (_noteFilter == NoteFilter.NoteName)
                    return (int)_noteName + 60; // C4
                else
                    return _lowestNote;
            }
        }

        public bool debugInput {
            get { return _debugInput; }
            set {
                if (!_debugInput)
                    if (value) NoteOn(_channel, debugNote, 1);
                else
                    if (!value) NoteOff(_channel, debugNote);
                _debugInput = value;
            }
        }

        #endregion

        #endif
    }
}
