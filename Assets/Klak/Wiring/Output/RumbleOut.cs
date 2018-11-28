using UnityEngine;
using UnityEngine.Events;

namespace Klak.Wiring
{
    [AddComponentMenu("Klak/Wiring/Output/Rumble Out")]
    public class RumbleOut : NodeBase
    {
        #region Node I/O

        [Inlet]
        public void Vibrate()
        {
            #if UNITY_IOS
            Handheld.Vibrate();
            #endif
        }

        #endregion
	}
}