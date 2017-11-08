using UnityEngine;
using UnityEditor;

namespace Klak.Wiring
{
    [CanEditMultipleObjects]
    [CustomEditor(typeof(VectorComponents))]
    public class VectorComponentsEditor : ScriptlessEditor {}
}