//
// MidiJack - MIDI Input Plugin for Unity
//
// Copyright (C) 2013-2016 Keijiro Takahashi
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
using System;
using System.Collections.Generic;

namespace MidiJack
{
    public class MidiSource : MidiEndpoint
    {
        class ChannelState
        {
            // Note state array
            // X<0    : Released on this frame
            // X=0    : Off
            // 0<X<=1 : On (X represents velocity)
            // 1<X<=2 : Triggered on this frame
            //          (X-1 represents velocity)
            public float[] _noteArray;

            // Knob number to knob value mapping
            public Dictionary<int, float> _knobMap;

            public ChannelState()
            {
                _noteArray = new float[128];
                _knobMap = new Dictionary<int, float>();
            }
        }

        // Channel state array
        ChannelState[] _channelArray;

        bool _isPlaying;

        int[] _sysexMem;

        protected override void AddEndpoint()
        {
            MidiDriver.AddSource(this);
        }

        protected override void RemoveEndpoint(uint endpointId)
        {
            MidiDriver.RemoveSource(endpointId);
        }

        #region Accessor Methods

        public override uint GetEndpointIdAtIndex(int index)
        {
            return MidiDriver.GetSourceIdAtIndex(index);
        }

        public override string GetEndpointName(uint endpointId)
        {
            return MidiDriver.GetSourceName(endpointId);
        }

        public override int CountEndpoints()
        {
            return MidiDriver.CountSources();
        }

        public float GetKey(MidiChannel channel, int noteNumber)
        {
            MidiDriver.Refresh();
            var v = _channelArray[(int)channel]._noteArray[noteNumber];
            if (v > 1) return v - 1;
            if (v > 0) return v;
            return 0.0f;
        }

        public bool GetKeyDown(MidiChannel channel, int noteNumber)
        {
            MidiDriver.Refresh();
            return _channelArray[(int)channel]._noteArray[noteNumber] > 1;
        }

        public bool GetKeyUp(MidiChannel channel, int noteNumber)
        {
            MidiDriver.Refresh();
            return _channelArray[(int)channel]._noteArray[noteNumber] < 0;
        }

        public int[] GetKnobNumbers(MidiChannel channel)
        {
            MidiDriver.Refresh();
            var cs = _channelArray[(int)channel];
            var numbers = new int[cs._knobMap.Count];
            cs._knobMap.Keys.CopyTo(numbers, 0);
            return numbers;
        }

        public float GetKnob(MidiChannel channel, int knobNumber, float defaultValue)
        {
            MidiDriver.Refresh();
            var cs = _channelArray[(int)channel];
            if (_midiMap) knobNumber = _midiMap.JackValue(knobNumber);
            if (cs._knobMap.ContainsKey(knobNumber)) return cs._knobMap[knobNumber];
            return defaultValue;
        }

        public bool IsPlaying()
        {
            MidiDriver.Refresh();
            return _isPlaying;
        }

        public int GetSysex(MidiSysex id)
        {
            MidiDriver.Refresh();

            int index = (int)id;

            if (index >= _sysexMem.Length)
                return 0;

            return _sysexMem[index];
        }

        #endregion

        #region Event Delegates

        public delegate void NoteOnDelegate(MidiChannel channel, int note, float velocity);
        public delegate void NoteOffDelegate(MidiChannel channel, int note);
        public delegate void KnobDelegate(MidiChannel channel, int knobNumber, float knobValue);
        public delegate void RealtimeDelegate(MidiRealtime realtimeMsg);
        public delegate void SysexDelegate(MidiSysex id, int value);

        public NoteOnDelegate noteOnDelegate { get; set; }
        public NoteOffDelegate noteOffDelegate { get; set; }
        public KnobDelegate knobDelegate { get; set; }
        public RealtimeDelegate realtimeDelegate { get; set; }
        public SysexDelegate sysexDelegate { get; set; }

        #endregion

        #region Public Methods

        public Queue<MidiMessage> msgQueue;

        #endregion

        #region Monobehaviour

        void Awake()
        {
            _channelArray = new ChannelState[17];
            for (var i = 0; i < 17; i++)
                _channelArray[i] = new ChannelState();

            _sysexMem = new int[Enum.GetNames(typeof(MidiSysex)).Length];
            _sysexMem[(int)MidiSysex.Tempo] = 120;

            msgQueue = new Queue<MidiMessage>();
        }

        void Update()
        {
            Refresh();

            // Update the note state array.
            foreach (var cs in _channelArray)
            {
                for (var i = 0; i < 128; i++)
                {
                    var x = cs._noteArray[i];
                    if (x > 1)
                        cs._noteArray[i] = x - 1; // Key down -> Hold.
                    else if (x < 0)
                        cs._noteArray[i] = 0; // Key up -> Off.
                }
            }

            MidiDriver.Refresh();

            // Process the message queue.
            while (true)
            {
                if (msgQueue.Count == 0)
                    break;

                // Pop from the queue.
                MidiMessage message = msgQueue.Dequeue();

                // Split the first byte.
                var statusCode = message.status >> 4;
                var channelNumber = message.status & 0xf;

                // Note on message?
                if (statusCode == 9)
                {
                    var velocity = 1.0f / 127 * message.data2 + 1;
                    _channelArray[channelNumber]._noteArray[message.data1] = velocity;
                    _channelArray[(int)MidiChannel.All]._noteArray[message.data1] = velocity;
                    if (noteOnDelegate != null)
                        noteOnDelegate((MidiChannel)channelNumber, message.data1, velocity - 1);
                }

                // Note off message?
                if (statusCode == 8 || (statusCode == 9 && message.data2 == 0))
                {
                    _channelArray[channelNumber]._noteArray[message.data1] = -1;
                    _channelArray[(int)MidiChannel.All]._noteArray[message.data1] = -1;
                    if (noteOffDelegate != null)
                        noteOffDelegate((MidiChannel)channelNumber, message.data1);
                }

                // CC message?
                if (statusCode == 0xb)
                {
                    // Normalize the value.
                    var level = 1.0f / 127 * message.data2;
                    // Update the channel if it already exists, or add a new channel.
                    int knobNumber = message.data1;
                    if (_midiMap) knobNumber = _midiMap.JackValue(knobNumber);
                    _channelArray[channelNumber]._knobMap[knobNumber] = level;
                    // Do again for All-ch.
                    _channelArray[(int)MidiChannel.All]._knobMap[knobNumber] = level;
                    if (knobDelegate != null)
                        knobDelegate((MidiChannel)channelNumber, knobNumber, level);
                }

                // System message?
                if (statusCode == 0xf)
                {
                    if (channelNumber == 0)
                    {
                        if (message.data1 >= _sysexMem.Length)
                            return;

                        _sysexMem[message.data1] = message.data2;

                        if (sysexDelegate != null)
                            sysexDelegate((MidiSysex)message.data1, message.data2);
                    }
                    else
                    {
                        if (message.status == (byte)MidiRealtime.Clock)
                            if (realtimeDelegate != null)
                                realtimeDelegate(MidiRealtime.Clock);

                        if (message.status == (byte)MidiRealtime.Start)
                        {
                            _isPlaying = true;

                            if (realtimeDelegate != null)
                                realtimeDelegate(MidiRealtime.Start);
                        }

                        if (message.status == (byte)MidiRealtime.Continue)
                        {
                            _isPlaying = true;

                            if (realtimeDelegate != null)
                                realtimeDelegate(MidiRealtime.Continue);
                        }

                        if (message.status == (byte)MidiRealtime.Stop)
                        {
                            _isPlaying = false;

                            if (realtimeDelegate != null)
                                realtimeDelegate(MidiRealtime.Stop);
                        }
                    }
                }
            }
        }

        void OnDestroy()
        {
            MidiDriver.RemoveSource(_endpointId);
        }

        #endregion
    }
}
