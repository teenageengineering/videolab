using UnityEngine;
using UnityEditor;
using Klak.Wiring;

namespace Klak.Midi
{
    [CanEditMultipleObjects]
    [CustomEditor(typeof(SequencerInput))]
    public class SequencerInputEditor : ScriptlessEditor {}
}
