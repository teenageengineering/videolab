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
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace MidiJack
{
    public class MidiDestination : MonoBehaviour
    {
        [SerializeField]
        private uint _endpointId = 0;
        public uint endpointId {
            get { return _endpointId; }
            set {
                _endpointId = value;
                this.endpointName = (_endpointId != 0) ? MidiDriver.GetDestinationName(value) : "";
            }
        }

        [SerializeField]
        private string _endpointName = "";
        public string endpointName {
            get { return _endpointName; }
            set { 
                _endpointName = value;

                if (_autoAssignMap)
                    AutoAssignMidiMap();
            }
        }

        [SerializeField]
        MidiMap _midiMap;

        [SerializeField]
        bool _autoAssignMap = true;

        void AutoAssignMidiMap()
        {
            _midiMap = MidiDriver.FindMapAtPath(_endpointName, Application.persistentDataPath);
            if (_midiMap == null)
                _midiMap = MidiDriver.FindMapAtPath(_endpointName, Application.streamingAssetsPath);
        }

        int _numDestinations = 0;

        void Update()
        {
            if (_numDestinations != MidiDriver.CountDestinations())
                CheckConnection();
        }

        void CheckConnection()
        {
            _numDestinations = MidiDriver.CountDestinations();

            // All destinations?
            if (endpointId == 0)
                return;

            // Restore MidiDriver connection
            int indexOfName = -1;
            for (var i = 0; i < _numDestinations; i++)
            {
                var id = MidiDriver.GetDestinationIdAtIndex(i);
                if (endpointId == id)
                    break;

                if (_endpointName == MidiDriver.GetDestinationName(id))
                    indexOfName = i;
            }

            if (indexOfName != -1)
                endpointId = MidiDriver.GetDestinationIdAtIndex(indexOfName);
        }

        public void SendMessage(MidiMessage msg)
        {
            if (endpointId == 0)
            {
                // Send to all.
                for (var i = 0; i < MidiDriver.CountDestinations(); i++)
                {
                    msg.endpoint = MidiDriver.GetDestinationIdAtIndex(i);
                    MidiDriver.SendMessage(msg.Encode64Bit());
                }
            }
            else
            {
                msg.endpoint = endpointId;
                MidiDriver.SendMessage(msg.Encode64Bit());
            }
        }

        public void SendKeyDown(MidiChannel channel, int noteNumber, int velocity)
        {
            MidiMessage msg = new MidiMessage();
            msg.status = (byte)(0x90 | ((int)channel & 0x0f));
            msg.data1 = (byte)noteNumber;
            msg.data2 = (byte)velocity;

            SendMessage(msg);
        }

        public void SendKeyUp(MidiChannel channel, int noteNumber)
        {
            MidiMessage msg = new MidiMessage();
            msg.status = (byte)(0x80 | ((int)channel & 0x0f));
            msg.data1 = (byte)noteNumber;

            SendMessage(msg);
        }

        public void SendKnob(MidiChannel channel, int knobNumber, float value)
        {
            MidiMessage msg = new MidiMessage();
            msg.status = (byte)(0xb0 | ((int)channel & 0x0f));
            if (_midiMap) knobNumber = _midiMap.Map(knobNumber);
            msg.data1 = (byte)knobNumber;
            msg.data2 = (byte)(value * 127);

            SendMessage(msg);
        }

        public void SendRealtime(MidiJack.MidiRealtime code)
        {
            MidiMessage msg = new MidiMessage();
            msg.status = (byte)code;

            SendMessage(msg);
        }
    }
}
