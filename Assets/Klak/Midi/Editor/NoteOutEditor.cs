using UnityEngine;
using UnityEditor;
using Klak.Wiring;

namespace Klak.Midi
{
    [CanEditMultipleObjects]
    [CustomEditor(typeof(NoteOut))]
    public class NoteOutEditor : ScriptlessEditor {}
}