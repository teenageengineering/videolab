using UnityEngine;
using UnityEditor;
using Klak.Wiring;

namespace Klak.Midi
{
    [CanEditMultipleObjects]
    [CustomEditor(typeof(SequencerOut))]
    public class SequencerOutEditor : ScriptlessEditor {}
}