using UnityEngine;
using UnityEditor;

namespace Klak.Wiring
{
    [CanEditMultipleObjects]
    [CustomEditor(typeof(TransformInput))]
    public class TransformInputEditor : ScriptlessEditor {}
}