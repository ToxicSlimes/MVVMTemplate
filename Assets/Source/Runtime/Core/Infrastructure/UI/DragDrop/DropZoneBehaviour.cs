using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Core.Infrastructure.UI.DragDrop
{
    public class DropZoneBehaviour : MonoBehaviour, IDropHandler, IPointerEnterHandler, IPointerExitHandler, IDropZone
    {
        [SerializeField] private Image _highlightBorder;
        [SerializeField] private Color _acceptColor = new(0.3f, 0.9f, 0.3f, 0.5f);
        [SerializeField] private Color _rejectColor = new(0.9f, 0.3f, 0.3f, 0.5f);
        [SerializeField] private float _highlightAlpha = 0.5f;

        [SerializeField] private UnityEvent<object> _onItemDropped;

        private RectTransform _rectTransform;
        private DragDropService _service;
        private Func<object, bool> _acceptFilter;
        private Color _originalColor;
        private bool _highlighted;

        public RectTransform RectTransform => _rectTransform;
        public UnityEvent<object> OnItemDropped => _onItemDropped;

        public void Initialize(DragDropService service, Func<object, bool> acceptFilter = null)
        {
            _service = service;
            _acceptFilter = acceptFilter;
        }

        public void SetAcceptFilter(Func<object, bool> filter)
        {
            _acceptFilter = filter;
        }

        private void Awake()
        {
            _rectTransform = GetComponent<RectTransform>();
            if (_highlightBorder != null)
                _originalColor = _highlightBorder.color;
        }

        public bool CanAccept(object dragData)
        {
            return _acceptFilter?.Invoke(dragData) ?? true;
        }

        public void OnDragEnter(object dragData)
        {
            if (_highlightBorder == null) return;

            _highlighted = true;
            var color = CanAccept(dragData) ? _acceptColor : _rejectColor;
            color.a = _highlightAlpha;
            _highlightBorder.color = color;
            _highlightBorder.enabled = true;
        }

        public void OnDragExit()
        {
            if (!_highlighted) return;
            _highlighted = false;

            if (_highlightBorder != null)
            {
                _highlightBorder.color = _originalColor;
                _highlightBorder.enabled = _originalColor.a > 0.01f;
            }
        }

        public void OnDrop(object dragData)
        {
            OnDragExit();
            _onItemDropped?.Invoke(dragData);
        }

        public void OnDrop(PointerEventData eventData)
        {
            var draggable = eventData.pointerDrag?.GetComponent<IDraggable>();
            if (draggable == null) return;

            var data = draggable.DragData;
            if (!CanAccept(data)) return;

            _service?.EndDrag(this);
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (!eventData.dragging) return;

            var draggable = eventData.pointerDrag?.GetComponent<IDraggable>();
            if (draggable == null) return;

            OnDragEnter(draggable.DragData);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            OnDragExit();
        }

        private void OnEnable()
        {
            _service?.RegisterDropZone(this);
        }

        private void OnDisable()
        {
            OnDragExit();
            _service?.UnregisterDropZone(this);
        }
    }
}
