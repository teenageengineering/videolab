using UnityEngine;
using UnityEditor;

namespace Klak.Wiring
{
    [CustomEditor(typeof(IntOut))]
    public class IntOutEditor : GenericOutEditor<int>
    {
    }
}