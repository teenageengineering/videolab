using UnityEngine;
using UnityEditor;
using Klak.Wiring;

namespace Klak.Midi
{
    [CanEditMultipleObjects]
    [CustomEditor(typeof(KnobOut))]
    public class KnobOutEditor : ScriptlessEditor {}
}
