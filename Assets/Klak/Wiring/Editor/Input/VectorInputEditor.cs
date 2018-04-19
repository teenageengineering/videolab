using UnityEngine;
using UnityEditor;

namespace Klak.Wiring
{
    [CustomEditor(typeof(VectorInput))]
    public class VectorInputEditor : GenericInputEditor<Vector3>
    {
    }
}
