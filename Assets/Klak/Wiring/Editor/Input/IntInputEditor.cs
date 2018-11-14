using UnityEngine;
using UnityEditor;

namespace Klak.Wiring
{
    [CustomEditor(typeof(IntInput))]
    public class IntInputEditor : GenericInputEditor<int>
    {
    }
}