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
    public class MidiDriver
    {
        #region Internal Data

        // Last update frame number
        int _lastFrame;

        Dictionary<uint, MidiSource> _sourceMap;

        #endregion

        #region Editor Support

        #if UNITY_EDITOR

        // Update timer
        const float _updateInterval = 1.0f / 30;
        float _lastUpdateTime;

        bool CheckUpdateInterval()
        {
            var current = Time.realtimeSinceStartup;
            if (current - _lastUpdateTime > _updateInterval || current < _lastUpdateTime) {
                _lastUpdateTime = current;
                return true;
            }
            return false;
        }

        // Total message count
        int _totalMessageCount;

        public int TotalMessageCount {
            get {
                UpdateIfNeeded();
                return _totalMessageCount;
            }
        }

        // Message history
        Queue<MidiMessage> _messageHistory;

        public Queue<MidiMessage> History {
            get { return _messageHistory; }
        }

        #endif

        #endregion

        #region Public Methods

        MidiDriver()
        {
            _sourceMap = new Dictionary<uint, MidiSource>();

            #if UNITY_EDITOR
            _messageHistory = new Queue<MidiMessage>();
            #endif
        }

        #endregion

        #region Private Methods

        void UpdateIfNeeded()
        {
            if (Application.isPlaying)
            {
                var frame = Time.frameCount;
                if (frame != _lastFrame) {
                    Update();
                    _lastFrame = frame;
                }
            }
            else
            {
                #if UNITY_EDITOR
                if (CheckUpdateInterval()) Update();
                #endif
            }
        }

        void Update()
        {
            while (true)
            {
                // Pop from the queue.
                var data = DequeueIncomingData();
                if (data == 0) break;

                // Relay the message.
                var message = new MidiMessage(data);

                if (_sourceMap.ContainsKey(message.endpoint))
                {
                    MidiSource source = _sourceMap[message.endpoint];
                    source.msgQueue.Enqueue(message);
                }

                // Send to all?
                if (_sourceMap.ContainsKey(uint.MaxValue))
                {
                    MidiSource source = _sourceMap[uint.MaxValue];
                    source.msgQueue.Enqueue(message);
                }

                #if UNITY_EDITOR
                // Record the message.
                _totalMessageCount++;
                _messageHistory.Enqueue(message);
                #endif
            }

            #if UNITY_EDITOR
            // Truncate the history.
            while (_messageHistory.Count > 8)
                _messageHistory.Dequeue();
            #endif
        }

        #endregion

        #region Native Plugin Interface

        #if (!UNITY_IOS && !UNITY_TVOS)
        const string _libName = "MidiJackPlugin";
        #else
        const string _libName = "__Internal";
        #endif

        [DllImport(_libName, EntryPoint="MidiJackCountSources")]
        public static extern int CountSources();

        [DllImport(_libName, EntryPoint="MidiJackCountDestinations")]
        public static extern int CountDestinations();

        [DllImport(_libName, EntryPoint="MidiJackGetSourceIDAtIndex")]
        public static extern uint GetSourceIdAtIndex(int index);

        [DllImport(_libName, EntryPoint="MidiJackGetDestinationIDAtIndex")]
        public static extern uint GetDestinationIdAtIndex(int index);

        [DllImport(_libName)]
        public static extern System.IntPtr MidiJackGetSourceName(uint id);

        public static string GetSourceName(uint id) {
        return Marshal.PtrToStringAnsi(MidiJackGetSourceName(id));
        }

        [DllImport(_libName)]
        public static extern System.IntPtr MidiJackGetDestinationName(uint id);

        public static string GetDestinationName(uint id) {
        return Marshal.PtrToStringAnsi(MidiJackGetDestinationName(id));
        }

        [DllImport(_libName, EntryPoint="MidiJackDequeueIncomingData")]
        public static extern ulong DequeueIncomingData();

        [DllImport(_libName, EntryPoint="MidiJackSendMessage")]
        public static extern void SendMessage(ulong msg);

        #endregion

        #region Singleton Class Instance

        static MidiDriver _instance;

        public static MidiDriver Instance {
            get {
                if (_instance == null)
                    _instance = new MidiDriver();
                
                return _instance;
            }
        }

        public static void Refresh()
        {
            Instance.UpdateIfNeeded();
        }

        public static void AddSource(MidiSource source)
        {
            Instance._sourceMap[source.endpointId] = source;
        }

        public static void RemoveSource(uint endpointId)
        {
            Instance._sourceMap.Remove(endpointId);
        }

        #endregion
    }
}
