using UnityEngine;
using UnityEditor;

namespace Klak.Wiring
{
    [CanEditMultipleObjects]
    [CustomEditor(typeof(ToEuler))]
    public class ToEulerEditor : ScriptlessEditor {}
}
