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
using System.IO;
using System.Linq;

namespace MidiJack
{
    public abstract class MidiEndpoint : MonoBehaviour
    {
        [SerializeField]
        protected uint _endpointId = 0;

        public uint endpointId {
            get { return _endpointId; }
            set { 
                _endpointId = value; 
                _endpointName = (_endpointId != 0) ? GetEndpointName(_endpointId) : "";
            }
        }

        [SerializeField]
        protected string _endpointName = "";

        public string endpointName {
            get { return _endpointName; }
            set { _endpointName = value; }
        }

        [SerializeField]
        protected bool _autoConnect;

        [SerializeField]
        protected string _preferredName;

        [SerializeField]
        protected MidiMap _midiMap;

        [SerializeField]
        protected bool _autoAssignMap = true;

        void AutoAssignMidiMap()
        {
            _midiMap = FindMapAtPath(Application.persistentDataPath);
            if (_midiMap == null)
                _midiMap = FindMapAtPath(Application.streamingAssetsPath);
        }

        MidiMap FindMapAtPath(string path)
        {
            if (!Directory.Exists(path))
                return null;
            
            var mapFiles = Directory.GetFiles(path).Where(p => Path.GetExtension(p) == ".json");

            foreach (string mapFile in mapFiles)
            {
                string name = Path.GetFileNameWithoutExtension(mapFile);
                int i = _endpointName.IndexOf(name);
                if (i > -1)
                {
                    MidiMap midiMap = ScriptableObject.CreateInstance<MidiMap>();
                    midiMap.name = name;
                    string mapJson = File.ReadAllText(mapFile);
                    JsonUtility.FromJsonOverwrite(mapJson, midiMap);

                    return midiMap;
                }
            }

            return null;
        }

        int _numEndpoints = 0;

        void CheckConnection()
        {
            _numEndpoints = CountEndpoints();

            if (_endpointId == uint.MaxValue)
                return;

            bool validId = false;
            int indexOfName = -1;
            int indexOfPreferredName = -1;

            for (var i = 0; i < _numEndpoints; i++)
            {
                var id = GetEndpointIdAtIndex(i);

                // Device still available?
                if (_endpointId == id)
                {
                    validId = true;
                    break;
                }

                string endpointName = GetEndpointName(id);

                // Device reconnected?
                if (_endpointName == endpointName)
                    indexOfName = i;

                // Device with preferred name available?
                if (_autoConnect && endpointName.IndexOf(_preferredName) != -1)
                    indexOfPreferredName = i;
            }

            if (validId)
            {
                // We're ok!
            }
            else if (indexOfName != -1)
            {
                _endpointId = GetEndpointIdAtIndex(indexOfName);
            }
            else if (indexOfPreferredName != -1)
            {
                _endpointId = GetEndpointIdAtIndex(indexOfPreferredName);
            }
            else
            {
                _endpointId = 0;
            }
        }

        protected virtual void AddEndpoint() {}

        protected virtual void RemoveEndpoint(uint endpointId) {}

        uint _prevEndpointId;

        void SwitchEndpoint()
        {
            RemoveEndpoint(_prevEndpointId);

            if (_endpointId != 0)
                AddEndpoint();

            if (_autoAssignMap)
                AutoAssignMidiMap();

            _prevEndpointId = _endpointId;
        }

        protected void Refresh()
        {
            if (_numEndpoints != CountEndpoints())
                CheckConnection();

            if (_endpointId != _prevEndpointId)
                SwitchEndpoint();
        }

        public abstract uint GetEndpointIdAtIndex(int index);

        public abstract string GetEndpointName(uint endpointID);

        public abstract int CountEndpoints();

        #region Monobehaviour

        void Start()
        {
            CheckConnection();
        }

        #endregion
    }
}
