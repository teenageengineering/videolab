using UnityEngine;
using System.Reflection;
using UnityEngine.SceneManagement;

namespace Klak.Wiring
{
    [AddComponentMenu("Klak/Wiring/Output/Switch scene")]
    public class SwitchScene : NodeBase
    {
        [Inlet]
        public string scene
        {
            set
            {
                if (!string.IsNullOrWhiteSpace(value))
                {
                    SceneManager.LoadScene(value);
                }
            }
        }
    }
}
