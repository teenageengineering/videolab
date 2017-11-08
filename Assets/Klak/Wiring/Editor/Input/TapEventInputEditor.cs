using UnityEngine;
using UnityEditor;

namespace Klak.Wiring
{
    [CanEditMultipleObjects]
    [CustomEditor(typeof(TapEventInput))]
    public class TapEventInputEditor : ScriptlessEditor {}
}