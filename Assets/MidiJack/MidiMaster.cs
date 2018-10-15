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

namespace MidiJack
{
    public class MidiMaster
    {
        MidiSource _source;
        MidiDestination _destination;

        MidiMaster()
        {
            GameObject sourceGo = new GameObject("Midi Master Source");
            GameObject.DontDestroyOnLoad(sourceGo);
            sourceGo.hideFlags = HideFlags.HideInHierarchy;
            _source = sourceGo.AddComponent<MidiSource>();
            _source.endpointId = uint.MaxValue;

            GameObject destinationGo = new GameObject("Midi Master Destination");
            GameObject.DontDestroyOnLoad(destinationGo);
            destinationGo.hideFlags = HideFlags.HideInHierarchy;
            _destination = destinationGo.AddComponent<MidiDestination>();
            _destination.endpointId = uint.MaxValue;
        }

        #region Singleton

        static MidiMaster _instance = null;

        public static MidiMaster Instance {
            get {
                if (_instance == null)
                    _instance = new MidiMaster();

                return _instance;
            }
        }

        #endregion

        // MIDI event delegates
        public static MidiSource.NoteOnDelegate noteOnDelegate {
            get { return Instance._source.noteOnDelegate; }
            set { Instance._source.noteOnDelegate = value; }
        }

        public static MidiSource.NoteOffDelegate noteOffDelegate {
            get { return Instance._source.noteOffDelegate; }
            set { Instance._source.noteOffDelegate = value; }
        }

        public static MidiSource.KnobDelegate knobDelegate {
            get { return Instance._source.knobDelegate; }
            set { Instance._source.knobDelegate = value; }
        }

        // Returns the key state (on: velocity, off: zero).
        public static float GetKey(MidiChannel channel, int noteNumber)
        {
            return Instance._source.GetKey(channel, noteNumber);
        }

        public static float GetKey(int noteNumber)
        {
            return Instance._source.GetKey(MidiChannel.All, noteNumber);
        }

        // Returns true if the key was pressed down in the current frame.
        public static bool GetKeyDown(MidiChannel channel, int noteNumber)
        {
            return Instance._source.GetKeyDown(channel, noteNumber);
        }

        public static bool GetKeyDown(int noteNumber)
        {
            return Instance._source.GetKeyDown(MidiChannel.All, noteNumber);
        }

        // Returns true if the key was released in the current frame.
        public static bool GetKeyUp(MidiChannel channel, int noteNumber)
        {
            return Instance._source.GetKeyUp(channel, noteNumber);
        }

        public static bool GetKeyUp(int noteNumber)
        {
            return Instance._source.GetKeyUp(MidiChannel.All, noteNumber);
        }

        // Provides the CC (knob) list.
        public static int[] GetKnobNumbers(MidiChannel channel)
        {
            return Instance._source.GetKnobNumbers(channel);
        }

        public static int[] GetKnobNumbers()
        {
            return Instance._source.GetKnobNumbers(MidiChannel.All);
        }

        // Returns the CC (knob) value.
        public static float GetKnob(MidiChannel channel, int knobNumber, float defaultValue = 0)
        {
            return Instance._source.GetKnob(channel, knobNumber, defaultValue);
        }

        public static float GetKnob(int knobNumber, float defaultValue = 0)
        {
            return Instance._source.GetKnob(MidiChannel.All, knobNumber, defaultValue);
        }

        public static void SendKeyDown(MidiChannel channel, int noteNumber, float velocity)
        {
            Instance._destination.SendKeyDown(channel, noteNumber, velocity);
        }

        public static void SendKeyUp(MidiChannel channel, int noteNumber)
        {
            Instance._destination.SendKeyUp(channel, noteNumber);
        }

        public static void SendKnob(MidiChannel channel, int knobNumber, float value)
        {
            Instance._destination.SendKnob(channel, knobNumber, value);
        }

        public static void SendRealtime(MidiJack.MidiRealtime code)
        {
            Instance._destination.SendRealtime(code);
        }

        // Bad singleton style..
        public static MidiSource GetSource()
        {
            return Instance._source;
        }

        public static MidiDestination GetDestination()
        {
            return Instance._destination;
        }
    }
}
