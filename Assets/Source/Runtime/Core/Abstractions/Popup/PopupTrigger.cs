using System;
using System.Threading;
using Core.Abstractions.Popup;
using Core.Abstractions.UI;
using Core.Data.UI;
using Cysharp.Threading.Tasks;
using Reflex.Attributes;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Core.Abstractions.Popup
{
    public class PopupTrigger : MonoBehaviour,
        IPointerEnterHandler,
        IPointerExitHandler,
        IPointerClickHandler,
        ITooltipTarget
    {
        [Header("Popup Settings")]
        [SerializeField] private GameObject _popupPrefab;
        [SerializeField] private PopupTriggerType _triggerType = PopupTriggerType.Hover;

        [Header("Position")]
        [SerializeField] private PopupPosition _position = PopupPosition.Auto;
        [SerializeField] private Vector2 _offset;
        [SerializeField] private float _screenPadding = 10f;

        [Header("Timing")]
        [SerializeField] private float _showDelay = 0.3f;
        [SerializeField] private float _hideDelay = 0f;

        [Inject] private IPopupService _popupService;

        private IPopup _currentPopup;
        private RectTransform _rectTransform;
        private CancellationTokenSource _enableCts;
        private bool _isPointerOver;
        private Func<IPopupData> _dataProvider;

        public IPopup CurrentPopup => _currentPopup;
        public bool IsShowing => _currentPopup != null && _currentPopup.IsVisible;

        private void Awake()
        {
            _rectTransform = GetComponent<RectTransform>();
        }

        private void OnEnable()
        {
            _enableCts = new CancellationTokenSource();
        }

        private void OnDisable()
        {
            _isPointerOver = false;
            _enableCts?.Cancel();
            _enableCts?.Dispose();
            _enableCts = null;

            if (_currentPopup != null)
                HideAsync().Forget();
        }

        public void SetDataProvider<TData>(Func<TData> provider) where TData : IPopupData
        {
            _dataProvider = () => provider();
        }

        public void SetDataProvider(Func<IPopupData> provider)
        {
            _dataProvider = provider;
        }

        public void SetTooltipData(string title, string description = null)
        {
            var data = new TooltipData(title, description);
            _dataProvider = () => data;
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            _isPointerOver = true;

            if (_triggerType == PopupTriggerType.Hover)
                ShowWithDelay().Forget();
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            _isPointerOver = false;

            if (_triggerType == PopupTriggerType.Hover)
                HideWithDelay().Forget();
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (_triggerType == PopupTriggerType.Click)
                Toggle().Forget();
        }

        public async UniTask ShowAsync()
        {
            if (_popupService == null || _popupPrefab == null)
                return;

            if (_currentPopup != null)
            {
                if (_currentPopup.IsVisible)
                    return;

                _currentPopup = null;
            }

            var showParams = CreateShowParams();

            if (_dataProvider != null)
            {
                var data = _dataProvider();
                _currentPopup = await _popupService.ShowFromPrefabAsync(_popupPrefab, _rectTransform, data, showParams);
            }
            else
            {
                _currentPopup = await _popupService.ShowFromPrefabAsync(_popupPrefab, _rectTransform, showParams);
            }

            if (_currentPopup != null && _currentPopup.IsVisible && ShouldForceHideAfterAwait())
            {
                await _popupService.HideAsync(_currentPopup);
                _currentPopup = null;
            }
        }

        private bool ShouldForceHideAfterAwait()
        {
            if (!isActiveAndEnabled)
                return true;

            if (_triggerType == PopupTriggerType.Hover && !_isPointerOver)
                return true;

            return false;
        }

        public async UniTask HideAsync()
        {
            if (_currentPopup == null || !_currentPopup.IsVisible)
                return;

            if (_popupService != null)
                await _popupService.HideAsync(_currentPopup);

            _currentPopup = null;
        }

        private async UniTask Toggle()
        {
            if (_currentPopup != null && _currentPopup.IsVisible)
                await HideAsync();
            else
                await ShowAsync();
        }

        private async UniTask ShowWithDelay()
        {
            var token = GetEnableToken();
            if (token.IsCancellationRequested) return;

            try
            {
                if (_showDelay > 0)
                    await UniTask.Delay(TimeSpan.FromSeconds(_showDelay), cancellationToken: token);

                if (_isPointerOver)
                    await ShowAsync();
            }
            catch (OperationCanceledException) { }
        }

        private async UniTask HideWithDelay()
        {
            var token = GetEnableToken();
            if (token.IsCancellationRequested) return;

            try
            {
                if (_hideDelay > 0)
                    await UniTask.Delay(TimeSpan.FromSeconds(_hideDelay), cancellationToken: token);

                if (!_isPointerOver)
                    await HideAsync();
            }
            catch (OperationCanceledException) { }
        }

        private CancellationToken GetEnableToken()
        {
            if (_enableCts == null)
                return destroyCancellationToken;

            return _enableCts.Token;
        }

        private PopupShowParams CreateShowParams()
        {
            var showParams = new PopupShowParams()
                .SetPosition(_position)
                .SetOffset(_offset)
                .SetScreenPadding(_screenPadding);

            if (_triggerType == PopupTriggerType.Hover)
                showParams.SetExclusive();

            return showParams;
        }
    }
}
