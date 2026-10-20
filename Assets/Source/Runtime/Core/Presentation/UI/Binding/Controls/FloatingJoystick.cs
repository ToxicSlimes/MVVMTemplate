using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.Layouts;
using UnityEngine.InputSystem.OnScreen;

namespace Core.UI.Controls
{
    public sealed class FloatingJoystick : OnScreenControl,
        IPointerDownHandler, IInitializePotentialDragHandler, IDragHandler, IPointerUpHandler
    {
        [InputControl(layout = "Vector2")]
        [SerializeField] private string _controlPath = "<Gamepad>/leftStick";
        [SerializeField] private RectTransform _ring;
        [SerializeField] private RectTransform _knob;
        [SerializeField] private float _movementRange = 100f;

        private Vector3 _ringRestPosition;
        private int? _pointerId;

        protected override string controlPathInternal
        {
            get => _controlPath;
            set => _controlPath = value;
        }

        private void Awake() => _ringRestPosition = _ring.localPosition;

        public void OnPointerDown(PointerEventData eventData)
        {
            if (_pointerId.HasValue)
            {
                return;
            }

            _pointerId = eventData.pointerId;
            _ring.localPosition = ToZonePoint(eventData);
            _knob.localPosition = Vector3.zero;
        }

        public void OnInitializePotentialDrag(PointerEventData eventData) => eventData.useDragThreshold = false;

        public void OnDrag(PointerEventData eventData)
        {
            if (eventData.pointerId != _pointerId)
            {
                return;
            }

            var offset = Vector2.ClampMagnitude(ToZonePoint(eventData) - (Vector2)_ring.localPosition, _movementRange);
            _knob.localPosition = offset;
            SendValueToControl(offset / _movementRange);
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (eventData.pointerId == _pointerId)
            {
                Release();
            }
        }

        private void OnApplicationFocus(bool hasFocus)
        {
            if (!hasFocus)
            {
                Release();
            }
        }

        private void Release()
        {
            _pointerId = null;
            _ring.localPosition = _ringRestPosition;
            _knob.localPosition = Vector3.zero;
            SendValueToControl(Vector2.zero);
        }

        private Vector2 ToZonePoint(PointerEventData eventData)
        {
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                (RectTransform)transform, eventData.position, eventData.pressEventCamera, out var point);
            return point;
        }
    }
}
