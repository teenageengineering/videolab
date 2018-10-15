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

namespace MidiJack
{
    public class MidiDestination : MidiEndpoint
    {
        public override uint GetEndpointIdAtIndex(int index)
        {
            return MidiDriver.GetDestinationIdAtIndex(index);
        }

        public override string GetEndpointName(uint endpointId)
        {
            return MidiDriver.GetDestinationName(endpointId);
        }

        public override int CountEndpoints()
        {
            return MidiDriver.CountDestinations();
        }

        public void SendMessage(MidiMessage msg)
        {
            if (_endpointId == uint.MaxValue)
            {
                // Send to all.
                for (var i = 0; i < MidiDriver.CountDestinations(); i++)
                {
                    msg.endpoint = MidiDriver.GetDestinationIdAtIndex(i);
                    MidiDriver.SendMessage(msg.Encode64Bit());
                }
            }
            else if (_endpointId != 0)
            {
                msg.endpoint = _endpointId;
                MidiDriver.SendMessage(msg.Encode64Bit());
            }
        }

        public void SendKeyDown(MidiChannel channel, int noteNumber, float velocity)
        {
            MidiMessage msg = new MidiMessage();
            msg.status = (byte)(0x90 | ((int)channel & 0x0f));
            msg.data1 = (byte)noteNumber;
            msg.data2 = (byte)System.Convert.ToByte(velocity * 127);

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
            if (_midiMap) knobNumber = _midiMap.DeviceValue(knobNumber);
            msg.data1 = (byte)knobNumber;
            msg.data2 = (byte)System.Convert.ToByte(value * 127);

            SendMessage(msg);
        }

        public void SendRealtime(MidiJack.MidiRealtime code)
        {
            MidiMessage msg = new MidiMessage();
            msg.status = (byte)code;

            SendMessage(msg);
        }

        #region Monobehaviour

        void Update()
        {
            Refresh();   
        }

        #endregion
    }
}
