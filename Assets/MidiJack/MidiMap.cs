using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MidiJack
{
    [CreateAssetMenu(fileName = "MidiMap", menuName = "MidiJack/MidiMap")]
    public class MidiMap : ScriptableObject
    {
        [Serializable]
        public class Entry
        {
            public int deviceValue;
            public int jackValue;
        }

        public List<Entry> entries = new List<Entry>();

        public int JackValue(int deviceValue)
        {
            Entry entry = entries.Find(e => e.deviceValue == deviceValue);
            if (entry != null)
                return entry.jackValue;

            return deviceValue;
        }

        public int DeviceValue(int jackValue)
        {
            Entry entry = entries.Find(e => e.jackValue == jackValue);
            if (entry != null)
                return entry.deviceValue;

            return jackValue;
        }
    }
}