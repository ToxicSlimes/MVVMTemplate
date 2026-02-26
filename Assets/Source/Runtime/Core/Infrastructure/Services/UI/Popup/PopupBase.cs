using System;
using System.Threading;
using Core.Abstractions.Popup;
using Core.Data.UI;
using Cysharp.Threading.Tasks;
using LitMotion;
using UnityEngine;

namespace Core.Infrastructure.Popup
{
    [RequireComponent(typeof(CanvasGroup))]
    public abstract class PopupBase : MonoBehaviour, IPopup
    {
        [SerializeField] protected CanvasGroup _canvasGroup;
        [SerializeField] protected RectTransform _contentRoot;

        private RectTransform _rectTransform;
        private RectTransform _canvasRect;
        private PopupShowParams _currentParams;
        private CancellationTokenSource _animationCts;
        private PopupAnimator _currentAnimator;
        private Func<Vector2> _mousePositionProvider;
        private bool _needsLayoutRebuild;

        public RectTransform RectTransform => _rectTransform ??= GetComponent<RectTransform>();
        public RectTransform Target { get; private set; }
        public bool IsVisible { get; private set; }

        public void SetMousePositionProvider(Func<Vector2> provider)
        {
            _mousePositionProvider = provider;
        }

        protected virtual void Awake()
        {
            if (_canvasGroup == null)
                _canvasGroup = GetComponent<CanvasGroup>();
            
            _rectTransform = GetComponent<RectTransform>();
        }

        public void SetTarget(RectTransform target)
        {
            Target = target;
        }

        public void SetCanvasRect(RectTransform canvasRect)
        {
            _canvasRect = canvasRect;
        }

        public void UpdatePosition(PopupShowParams showParams)
        {
            _currentParams = showParams ?? PopupShowParams.Default();

            if (_currentParams.Position == PopupPosition.Mouse)
            {
                UpdateMousePosition();
                return;
            }

            if (Target == null) return;

            UpdateTargetPosition();
        }

        private void UpdateMousePosition()
        {
            ForceLayoutRebuildIfNeeded();

            var screenPos = _mousePositionProvider?.Invoke() ?? Vector2.zero;
            var mousePos = ScreenClampUtility.GetMousePosition(_canvasRect, screenPos);
            RectTransform.anchoredPosition = mousePos + _currentParams.Offset;

            if (_currentParams.ClampToScreen && _canvasRect != null)
                RectTransform.anchoredPosition = ScreenClampUtility.ClampToCanvas(
                    RectTransform.anchoredPosition,
                    RectTransform.rect.size,
                    _canvasRect.rect.size,
                    _currentParams.ScreenPadding);
        }

        private void UpdateTargetPosition()
        {
            ForceLayoutRebuildIfNeeded();

            var basePosition = GetTargetPositionInParentSpace();
            var popupSize = RectTransform.rect.size;
            var targetSize = Target.rect.size;
            var halfTarget = targetSize * 0.5f;
            var halfPopup = popupSize * 0.5f;
            var padding = _currentParams.ScreenPadding;

            var position = _currentParams.Position;
            if (position == PopupPosition.Auto && _canvasRect != null)
                position = ScreenClampUtility.GetBestPosition(basePosition, popupSize, _canvasRect.rect.size, padding);
            else if (position == PopupPosition.Auto)
                position = PopupPosition.Bottom;

            var offset = ScreenClampUtility.CalculateOffset(position, halfTarget, halfPopup, _currentParams.Offset);
            var finalPos = basePosition + offset;

            if (_currentParams.ClampToScreen && _canvasRect != null)
            {
                var canvasSize = _canvasRect.rect.size;
                finalPos = ScreenClampUtility.ClampToCanvas(finalPos, popupSize, canvasSize, padding);

                if (_currentParams.AllowFlip && ScreenClampUtility.IsOutOfBounds(finalPos, popupSize, canvasSize, padding))
                {
                    var flipped = ScreenClampUtility.FlipPosition(position);
                    var flippedOffset = ScreenClampUtility.CalculateOffset(flipped, halfTarget, halfPopup, _currentParams.Offset);
                    var flippedPos = ScreenClampUtility.ClampToCanvas(basePosition + flippedOffset, popupSize, canvasSize, padding);

                    if (!ScreenClampUtility.IsOutOfBounds(flippedPos, popupSize, canvasSize, padding))
                        finalPos = flippedPos;
                }
            }

            RectTransform.anchoredPosition = finalPos;
        }

        private void ForceLayoutRebuildIfNeeded()
        {
            if (!_needsLayoutRebuild) return;

            Canvas.ForceUpdateCanvases();
            UnityEngine.UI.LayoutRebuilder.ForceRebuildLayoutImmediate(RectTransform);
            _needsLayoutRebuild = false;
        }

        private Vector2 GetTargetPositionInParentSpace()
        {
            var popupParent = RectTransform.parent as RectTransform;
            if (popupParent == null || popupParent == Target)
                return Vector2.zero;

            var targetWorldCenter = Target.TransformPoint(Target.rect.center);
            var localPoint = popupParent.InverseTransformPoint(targetWorldCenter);
            return new Vector2(localPoint.x, localPoint.y);
        }

        public virtual async UniTask ShowAsync()
        {
            CancelAnimation();

            gameObject.SetActive(true);
            IsVisible = true;

            _animationCts = CancellationTokenSource.CreateLinkedTokenSource(destroyCancellationToken);
            _currentAnimator = GetShowAnimation();
            
            try
            {
                await _currentAnimator.Play(_animationCts.Token);
            }
            catch (OperationCanceledException)
            {
            }
        }

        public virtual async UniTask HideAsync()
        {
            CancelAnimation();

            _animationCts = CancellationTokenSource.CreateLinkedTokenSource(destroyCancellationToken);
            _currentAnimator = GetHideAnimation();
            
            try
            {
                await _currentAnimator.Play(_animationCts.Token);
            }
            catch (OperationCanceledException)
            {
            }
            finally
            {
                IsVisible = false;
                if (this != null)
                    gameObject.SetActive(false);
            }
        }

        protected virtual PopupAnimator GetShowAnimation()
        {
            return Animate()
                .FadeIn(0.15f)
                .Scale(0.9f, 1f, 0.15f, Ease.OutBack);
        }

        protected virtual PopupAnimator GetHideAnimation()
        {
            return Animate()
                .FadeOut(0.1f)
                .Scale(1f, 0.95f, 0.1f, Ease.InQuad);
        }

        public PopupAnimator Animate()
        {
            return new PopupAnimator(_canvasGroup, RectTransform);
        }

        private void CancelAnimation()
        {
            _currentAnimator?.Cancel();
            _currentAnimator = null;
            
            _animationCts?.Cancel();
            _animationCts?.Dispose();
            _animationCts = null;
        }

        public virtual void OnGetFromPool()
        {
            _needsLayoutRebuild = true;

            if (_canvasGroup == null)
                _canvasGroup = GetComponent<CanvasGroup>();
            
            if (_canvasGroup != null)
            {
                _canvasGroup.alpha = 0f;
                _canvasGroup.interactable = true;
                _canvasGroup.blocksRaycasts = true;
            }

            RectTransform.localScale = Vector3.one;
        }

        public virtual void OnReturnToPool()
        {
            CancelAnimation();
            Target = null;
            _currentParams = null;
            IsVisible = false;
            
            if (_canvasGroup == null)
                _canvasGroup = GetComponent<CanvasGroup>();
            
            if (_canvasGroup != null)
            {
                _canvasGroup.interactable = false;
                _canvasGroup.blocksRaycasts = false;
            }
        }

        public virtual void SetData(IPopupData data)
        {
            _needsLayoutRebuild = true;
        }

        public virtual void Dispose()
        {
            CancelAnimation();
        }

        protected virtual void OnDestroy()
        {
            Dispose();
        }

        protected virtual void LateUpdate()
        {
            if (!IsVisible || _currentParams == null) return;

            if (_currentParams.FollowTarget && Target != null)
                UpdatePosition(_currentParams);

            if (_currentParams.Position == PopupPosition.Mouse)
                UpdatePosition(_currentParams);
        }
    }

    public abstract class PopupBase<TData> : PopupBase, IPopup<TData> where TData : IPopupData
    {
        public TData Data { get; private set; }

        public void SetData(TData data)
        {
            base.SetData(data);
            Data = data;
            OnDataSet(data);
        }

        public override void SetData(IPopupData data)
        {
            if (data is TData typedData)
                SetData(typedData);
        }

        protected abstract void OnDataSet(TData data);

        public override void OnReturnToPool()
        {
            base.OnReturnToPool();
            Data = default;
            OnDataCleared();
        }

        protected virtual void OnDataCleared() { }
    }
}
