using Scellecs.Morpeh;
using Scellecs.Morpeh.Providers;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;

namespace Core.Gameplay.Input
{
    public sealed class TouchLookZone : EntityProvider,
        IPointerDownHandler, IInitializePotentialDragHandler, IDragHandler, IPointerUpHandler
    {
        private TouchLookAspect _touchLook;
        private int? _pointerId;

        protected override void Initialize()
        {
            _touchLook = new TouchLookAspect(Entity.GetWorld()).For(Entity);
            _touchLook.AddTo(Entity);
        }

        protected override void Deinitialize()
        {
            _pointerId = null;
            _touchLook.RemoveFrom(Entity);
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (_pointerId.HasValue || !IsTouch(eventData))
            {
                return;
            }

            _pointerId = eventData.pointerId;
            _touchLook.Held = true;
        }

        public void OnInitializePotentialDrag(PointerEventData eventData) => eventData.useDragThreshold = false;

        public void OnDrag(PointerEventData eventData)
        {
            if (eventData.pointerId == _pointerId)
            {
                _touchLook.Delta += eventData.delta;
            }
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
            if (!hasFocus && _pointerId.HasValue)
            {
                Release();
            }
        }

        private void Release()
        {
            _pointerId = null;
            _touchLook.Held = false;
        }

        private static bool IsTouch(PointerEventData eventData) =>
            eventData is ExtendedPointerEventData { device: Touchscreen or Pen };
    }
}
