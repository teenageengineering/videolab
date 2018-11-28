using UnityEngine;
using UnityEditor;

namespace Klak.Wiring
{
    [CustomEditor(typeof(BoolInput))]
    public class BoolInputEditor : GenericInputEditor<bool>
    {
    }
}