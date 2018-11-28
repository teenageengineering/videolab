using UnityEngine;
using UnityEditor;

namespace Klak.Wiring
{
    [CustomEditor(typeof(BoolOut))]
    public class BoolOutEditor : GenericOutEditor<bool>
    {
    }
}