using UnityEngine;
using UnityEditor;

namespace Klak.Wiring
{
    [CanEditMultipleObjects]
    [CustomEditor(typeof(DragEventInput))]
    public class DragEventInputEditor : ScriptlessEditor {}
}