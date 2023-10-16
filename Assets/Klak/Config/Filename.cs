using UnityEngine;

namespace Klak.Wiring
{
    [AddComponentMenu("Klak/Wiring/Config/Filename")]
    public class Filename : NodeBase
    {
        #region Node I/O

        [Inlet]
        public string text
        {
            set
            {
                _textEvent.Invoke(ConfigMaster.GetFolder() + value);
            }
        }

        [SerializeField, Outlet]
        StringEvent _textEvent = new StringEvent();

        #endregion
    }
}