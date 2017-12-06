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
            public int from;
            public int to;
        }

        public List<Entry> entries = new List<Entry>();

        public int Map(int from)
        {
            Entry entry = entries.Find(e => e.from == from);
            if (entry != null)
                return entry.to;

            return from;
        }
    }
}