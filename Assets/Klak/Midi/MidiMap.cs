using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MidiJack;

namespace Klak.Midi
{
    [Serializable]
    public struct MidiMapEntry
    {
        public string name;
        public int value;
    }

    [CreateAssetMenu(fileName = "MidiMap", menuName = "Klak/MIDI/MidiMap")]
    public class MidiMap : ScriptableObject
    {
        public List<MidiMapEntry> entries;

        public string[] GetNames()
        {
            List<string> names = new List<string>();
            for (int i = 0; i < entries.Count; i++)
                names.Add(entries[i].name);

            return names.ToArray();
        }
    }
}