using UnityEngine;
using UnityEngine.EventSystems;

namespace Klak.Wiring
{
    public enum DragAxis
    {
        Horizontal,
        Vertical,
        Radial,
        Angular
    }

    [AddComponentMenu("Klak/Wiring/Input/UI Event/Drag Event Input")]
    public class DragEventInput : NodeBase
    {
        #region Editable properties

        [SerializeField]
        RectTransform _triggerRect;

        [SerializeField]
        DragAxis _axis = DragAxis.Vertical;

        [SerializeField]
        bool _relative = true;

        [SerializeField]
        float _range = 100;

        #endregion

        #region Node I/O

        [SerializeField, Outlet]
        FloatEvent _valueEvent = new FloatEvent();

        #endregion

        #region Private methods

        float GetAngle(Vector2 v1, Vector2 v2)
        {
            var sign = Mathf.Sign(v1.x * v2.y - v1.y * v2.x);
            return Vector2.Angle(v1, v2) * sign;
        }

        void DragRelative(PointerEventData pointerData)
        {
            Vector2 position = Vector2.zero;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(_triggerRect, pointerData.position, pointerData.pressEventCamera, out position);

            Canvas canvas = _triggerRect.GetComponentInParent<Canvas>();
            Vector2 deltaPosition = pointerData.delta / canvas.scaleFactor;

            Vector2 prevPos = position - deltaPosition;
            float delta = 0;

            switch (_axis) {

            case DragAxis.Horizontal:
                delta = deltaPosition.x / _range;
                break;

            case DragAxis.Vertical:
                delta = deltaPosition.y / _range;
                break;

            case DragAxis.Radial:
                delta = (position.magnitude - prevPos.magnitude) / _range;
                break;

            case DragAxis.Angular:
                delta = GetAngle(position, prevPos) / _range;
                break;
            }

            _valueEvent.Invoke(delta);
        }

        void DragAbsolute(PointerEventData pointerData)
        {
            Vector2 position = Vector2.zero;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(_triggerRect, pointerData.position, pointerData.pressEventCamera, out position);

            float value = 0;

            switch (_axis) {

            case DragAxis.Horizontal:
                value = position.x / _range + 0.5f;
                break;

            case DragAxis.Vertical:
                value = position.y / _range + 0.5f;
                break;

            case DragAxis.Radial:
                value = position.magnitude / _range;
                break;

            case DragAxis.Angular:
                value = GetAngle(position, Vector2.up) / _range + 0.5f;
                break;
            }

            _valueEvent.Invoke(value);
        }

        void PointerDown(BaseEventData eventData)
        {
            PointerEventData pointerData = eventData as PointerEventData;

            if (!_relative)
                DragAbsolute(pointerData);
        }

        void Drag(BaseEventData eventData)
        {
            PointerEventData pointerData = eventData as PointerEventData;

            if (_relative)
                DragRelative(pointerData);
            else
                DragAbsolute(pointerData);
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

            EventTrigger.Entry dragEntry = new EventTrigger.Entry();
            dragEntry.eventID = EventTriggerType.Drag;
            dragEntry.callback.AddListener((data) => {
                Drag((PointerEventData)data);
            });
            trigger.triggers.Add(dragEntry);
        }

        #endregion
    }
}