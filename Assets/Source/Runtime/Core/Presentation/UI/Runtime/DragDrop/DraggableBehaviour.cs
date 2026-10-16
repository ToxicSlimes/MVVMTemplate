using System;
using LitMotion;
using LitMotion.Extensions;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Core.Presentation.UI.DragDrop
{
    [RequireComponent(typeof(CanvasGroup))]
    public class DraggableBehaviour : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IDraggable
    {
        [SerializeField] private float _ghostAlpha = 0.7f;
        [SerializeField] private float _sourceAlpha = 0.4f;
        [SerializeField] private bool _returnOnFail = true;
        [SerializeField] private Canvas _dragCanvas;

        private RectTransform _rectTransform;
        private CanvasGroup _canvasGroup;
        private GameObject _ghost;
        private RectTransform _ghostRect;
        private Canvas _rootCanvas;
        private DragDropService _service;
        private MotionHandle _activeMotion;
        private Func<object> _dragDataProvider;
        private Func<bool> _canDragProvider;

        public RectTransform RectTransform => _rectTransform;
        public object DragData => _dragDataProvider?.Invoke();
        public bool CanDrag => _canDragProvider?.Invoke() ?? true;

        public void Initialize(DragDropService service, Func<object> dragDataProvider, Func<bool> canDragProvider = null)
        {
            _service = service;
            _dragDataProvider = dragDataProvider;
            _canDragProvider = canDragProvider;
        }

        private void Awake()
        {
            _rectTransform = GetComponent<RectTransform>();
            _canvasGroup = GetComponent<CanvasGroup>();
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            if (eventData.button != PointerEventData.InputButton.Left) return;
            if (!CanDrag) return;

            _rootCanvas = _dragCanvas != null ? _dragCanvas : GetRootCanvas();
            if (_rootCanvas == null) return;

            _ghost = DragVisualFactory.CreateGhost(_rectTransform, _rootCanvas, _ghostAlpha);
            _ghostRect = _ghost.GetComponent<RectTransform>();

            UpdateGhostPosition(eventData);
            AnimateGhostAppear();

            _canvasGroup.alpha = _sourceAlpha;
            _canvasGroup.blocksRaycasts = false;

            _service?.BeginDrag(this);
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (_ghost == null) return;
            UpdateGhostPosition(eventData);
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            if (_ghost == null) return;

            if (_service != null)
            {
                var dropZone = _service.FindDropZoneAt(eventData.position);
                _service.EndDrag(dropZone);

                if (dropZone != null)
                {
                    DestroyGhost();
                    return;
                }
            }

            if (_returnOnFail)
                AnimateReturnToSource();
            else
                DestroyGhost();
        }

        void IDraggable.OnDragStarted()
        {
        }

        void IDraggable.OnDragEnded(bool success)
        {
            if (!success && !_returnOnFail)
                RestoreSource();
        }

        private void UpdateGhostPosition(PointerEventData eventData)
        {
            if (_rootCanvas.renderMode == RenderMode.ScreenSpaceOverlay)
            {
                _ghostRect.position = (Vector3)eventData.position;
            }
            else
            {
                RectTransformUtility.ScreenPointToWorldPointInRectangle(
                    _rootCanvas.GetComponent<RectTransform>(),
                    eventData.position,
                    eventData.pressEventCamera,
                    out var worldPoint);
                _ghostRect.position = worldPoint;
            }
        }

        private void AnimateGhostAppear()
        {
            _ghostRect.localScale = Vector3.one * 0.9f;
            CancelMotion();
            _activeMotion = LMotion.Create(0.9f, 1f, 0.15f)
                .WithScheduler(MotionScheduler.UpdateIgnoreTimeScale)
                .WithEase(Ease.OutBack)
                .BindToLocalScaleX(_ghostRect)
                .AddTo(_ghost);

            LMotion.Create(0.9f, 1f, 0.15f)
                .WithScheduler(MotionScheduler.UpdateIgnoreTimeScale)
                .WithEase(Ease.OutBack)
                .BindToLocalScaleY(_ghostRect)
                .AddTo(_ghost);
        }

        private void AnimateReturnToSource()
        {
            var startPos = _ghostRect.position;
            var endPos = _rectTransform.position;

            CancelMotion();
            _activeMotion = LMotion.Create(startPos, endPos, 0.25f)
                .WithScheduler(MotionScheduler.UpdateIgnoreTimeScale)
                .WithEase(Ease.OutCubic)
                .WithOnComplete(() =>
                {
                    DestroyGhost();
                    RestoreSource();
                })
                .BindToPosition(_ghostRect)
                .AddTo(_ghost);
        }

        private void RestoreSource()
        {
            if (_canvasGroup == null) return;
            _canvasGroup.alpha = 1f;
            _canvasGroup.blocksRaycasts = true;
        }

        private void DestroyGhost()
        {
            CancelMotion();
            if (_ghost != null)
                Destroy(_ghost);
            _ghost = null;
            _ghostRect = null;
            RestoreSource();
        }

        private void CancelMotion()
        {
            if (_activeMotion.IsActive())
                _activeMotion.Cancel();
        }

        private Canvas GetRootCanvas()
        {
            var canvas = GetComponentInParent<Canvas>();
            while (canvas != null && !canvas.isRootCanvas)
            {
                var parent = canvas.transform.parent;
                canvas = parent != null ? parent.GetComponentInParent<Canvas>() : null;
            }
            return canvas;
        }

        private void OnDisable()
        {
            if (_ghost != null)
            {
                _service?.CancelDrag();
                DestroyGhost();
            }
        }
    }
}
