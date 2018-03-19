using UnityEngine;
using UnityEngine.EventSystems;

namespace Klak.Wiring
{
    [AddComponentMenu("Klak/Wiring/Input/UI Event/Tap Event Input")]
    public class TapEventInput : NodeBase
    {
        #region Editable properties

        [SerializeField]
        RectTransform _triggerRect;

        #endregion

        #region Node I/O

        [SerializeField, Outlet]
        VoidEvent _down = new VoidEvent();

        [SerializeField, Outlet]
        VoidEvent _up = new VoidEvent();

        #endregion

        #region Private members

        void PointerDown(BaseEventData eventData)
        {
            _down.Invoke();
        }

        void PointerUp(BaseEventData eventData)
        {
            _up.Invoke();
        }

        #endregion

        #region MonoBehaviour functions

        void Start()
        {
            EventTrigger trigger = _triggerRect.gameObject.AddComponent<EventTrigger>();

            EventTrigger.Entry pointerDownEntry = new EventTrigger.Entry();
            pointerDownEntry.eventID = EventTriggerType.PointerDown;
            pointerDownEntry.callback.AddListener((data) => {
                PointerDown((PointerEventData)data);
            });
            trigger.triggers.Add(pointerDownEntry);

            EventTrigger.Entry pointerUpEntry = new EventTrigger.Entry();
            pointerUpEntry.eventID = EventTriggerType.PointerUp;
            pointerUpEntry.callback.AddListener((data) => {
                PointerUp((PointerEventData)data);
            });
            trigger.triggers.Add(pointerUpEntry);
        }

        #endregion
    }
}