using System;
using System.Collections.Generic;
using Core.Abstractions.MVVM.Canvas;
using Core.Abstractions.Popup;
using Core.Abstractions.Services.Input;
using Core.Domain.UI;
using Core.Infrastructure.Extensions;
using Cysharp.Threading.Tasks;
using R3;
using UnityEngine;
using UnityEngine.UI;

namespace Core.Infrastructure.Popup
{
    public class PopupService : IPopupService, IDisposable
    {
        private readonly ICanvasProvider _canvasProvider;
        private readonly IInputService _inputService;
        private readonly IPopupPoolFactory _poolFactory;
        private readonly Dictionary<Type, GameObject> _prefabs = new();
        private readonly Dictionary<Type, IPopupPool> _pools = new();
        private readonly List<IPopup> _activePopups = new();
        private readonly Dictionary<IPopup, IPopupPool> _popupPoolMap = new();
        private readonly Transform _poolContainer;
        private RectTransform _overlayCanvasRect;
        private bool _ownsOverlayCanvas;
        private IDisposable _cancelSubscription;

        public IReadOnlyCollection<IPopup> ActivePopups => _activePopups;

        public event Action<IPopup> PopupShown;
        public event Action<IPopup> PopupHidden;

        public PopupService(
            ICanvasProvider canvasProvider,
            IPopupPoolFactory poolFactory,
            IInputService inputService = null)
        {
            _canvasProvider = canvasProvider;
            _poolFactory = poolFactory;
            _inputService = inputService;

            var poolGo = new GameObject("[PopupPool]");
            poolGo.SetActive(true);
            if (UnityEngine.Application.isPlaying)
                UnityEngine.Object.DontDestroyOnLoad(poolGo);
            _poolContainer = poolGo.transform;

            InitializeOverlayCanvas();
            SubscribeToInput();
        }

        private void SubscribeToInput()
        {
            if (_inputService == null) return;
            
            _cancelSubscription = _inputService.OnCancel
                .Subscribe(this,static (_, self) => self.HideTopPopup());
        }

        public void HideTopPopup()
        {
            HideTopPopupAsync().Forget();
        }

        public async UniTask HideTopPopupAsync()
        {
            if (_activePopups.Count == 0) return;
            
            var topPopup = _activePopups[^1];
            await HideAsync(topPopup);
        }

        private void InitializeOverlayCanvas()
        {
            var overlayCanvas = _canvasProvider?.GetCanvas(CanvasLayer.Overlay);

            if (overlayCanvas == null)
            {
                var canvasGo = new GameObject("[PopupOverlay]");
                overlayCanvas = canvasGo.AddComponent<UnityEngine.Canvas>();
                overlayCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
                overlayCanvas.sortingOrder = 999;

                var scaler = canvasGo.AddComponent<CanvasScaler>();
                scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                scaler.referenceResolution = new Vector2(1920, 1080);
                scaler.matchWidthOrHeight = 0.5f;

                canvasGo.AddComponent<GraphicRaycaster>();

                if (UnityEngine.Application.isPlaying)
                    UnityEngine.Object.DontDestroyOnLoad(canvasGo);

                _ownsOverlayCanvas = true;
            }

            _overlayCanvasRect = overlayCanvas.GetComponent<RectTransform>();
        }

        private Vector2 GetMouseScreenPosition()
        {
            return _inputService?.MouseScreenPosition ?? Vector2.zero;
        }

        public void RegisterPrefab<TPopup>(GameObject prefab) where TPopup : class, IPopup
        {
            if (prefab == null)
                throw new ArgumentNullException(nameof(prefab));

            var popupType = typeof(TPopup);
            
            if (prefab.GetComponent<TPopup>() == null)
            {
                Debug.LogError($"Prefab {prefab.name} does not have component of type {popupType.Name}");
                return;
            }

            _prefabs[popupType] = prefab;
        }

        public async UniTask<TPopup> ShowAsync<TPopup, TData>(
            RectTransform target,
            TData data,
            PopupShowParams showParams = null)
            where TPopup : class, IPopup<TData>
            where TData : IPopupData
        {
            var popup = await ShowInternalAsync(typeof(TPopup), target, showParams, data);
            return popup as TPopup;
        }

        public async UniTask<TPopup> ShowAsync<TPopup>(
            RectTransform target,
            PopupShowParams showParams = null)
            where TPopup : class, IPopup
        {
            var popup = await ShowInternalAsync(typeof(TPopup), target, showParams);
            return popup as TPopup;
        }

        public async UniTask<IPopup> ShowFromPrefabAsync(
            GameObject prefab,
            RectTransform target,
            PopupShowParams showParams = null)
        {
            if (prefab == null) return null;

            var popupComponent = prefab.GetComponent<PopupBase>();
            if (popupComponent == null) return null;

            var popupType = popupComponent.GetType();
            
            _prefabs.TryAdd(popupType, prefab);

            return await ShowInternalAsync(popupType, target, showParams);
        }

        public async UniTask<IPopup> ShowFromPrefabAsync(
            GameObject prefab,
            RectTransform target,
            IPopupData data,
            PopupShowParams showParams = null)
        {
            if (prefab == null) return null;

            var popupComponent = prefab.GetComponent<PopupBase>();
            if (popupComponent == null) return null;

            var popupType = popupComponent.GetType();
            _prefabs.TryAdd(popupType, prefab);

            return await ShowInternalAsync(popupType, target, showParams, data);
        }

        private async UniTask<IPopup> ShowInternalAsync(
            Type popupType,
            RectTransform target,
            PopupShowParams showParams,
            IPopupData data = null)
        {
            showParams ??= PopupShowParams.Default();

            if (showParams.Exclusive && _activePopups.Count > 0)
                await HideAllAsync();

            var pool = GetOrCreatePool(popupType);

            if (pool == null)
            {
                Debug.LogError($"Failed to get pool for popup type {popupType.Name}. Make sure prefab is registered.");
                return null;
            }

            var popup = pool.Get();
            if (popup == null)
            {
                Debug.LogError($"[PopupService] pool.Get() returned NULL for {popupType.Name}");
                return null;
            }

            if (target == null)
            {
                Debug.LogError($"[PopupService] Target is NULL! Cannot show popup.");
                return null;
            }

            if (popup.RectTransform != null)
            {
                var parentTransform = _overlayCanvasRect != null
                    ? (Transform)_overlayCanvasRect
                    : target;
                popup.RectTransform.SetParent(parentTransform, false);
                popup.RectTransform.gameObject.SetActive(true);
                popup.RectTransform.SetAsLastSibling();

                var canvasRect = _overlayCanvasRect
                    ?? target.GetComponentInParent<UnityEngine.Canvas>()?.GetComponent<RectTransform>();
                if (canvasRect != null)
                    popup.SetCanvasRect(canvasRect);

                popup.SetMousePositionProvider(GetMouseScreenPosition);
                popup.SetTarget(target);

                if (data != null)
                    popup.SetData(data);

                popup.UpdatePosition(showParams);
            }

            if (showParams.ShowDelay > 0)
                await UniTask.Delay(TimeSpan.FromSeconds(showParams.ShowDelay));

            _activePopups.Add(popup);
            _popupPoolMap[popup] = pool;

            await popup.ShowAsync();

            PopupShown?.Invoke(popup);

            return popup;
        }

        public async UniTask HideAsync(IPopup popup)
        {
            if (popup == null || !_activePopups.Contains(popup))
                return;

            await popup.HideAsync();

            _activePopups.Remove(popup);

            if (_popupPoolMap.Remove(popup, out var pool))
                pool.Return(popup);

            PopupHidden?.Invoke(popup);
        }

        public void HideAll()
        {
            HideAllAsync().Forget();
        }

        public async UniTask HideAllAsync()
        {
            var popupsToHide = new List<IPopup>(_activePopups);
            var hideTasks = new List<UniTask>();
            
            foreach (var popup in popupsToHide)
                hideTasks.Add(HideAsync(popup));
            
            await UniTask.WhenAll(hideTasks);
        }

        private IPopupPool GetOrCreatePool(Type popupType)
        {
            if (_pools.TryGetValue(popupType, out var existingPool))
                return existingPool;

            if (!_prefabs.TryGetValue(popupType, out var prefab))
            {
                Debug.LogError($"No prefab registered for popup type {popupType.Name}. Call RegisterPrefab<{popupType.Name}>() first.");
                return null;
            }

            var pool = _poolFactory.CreatePool(prefab, _poolContainer);
            _pools[popupType] = pool;
            return pool;
        }

        public void Dispose()
        {
            _cancelSubscription?.Dispose();
            
            HideAll();
            
            foreach (var pool in _pools.Values)
                pool.Clear();
            
            _pools.Clear();
            _prefabs.Clear();
            _activePopups.Clear();
            _popupPoolMap.Clear();

            if (_poolContainer != null)
                _poolContainer.gameObject.SafeDestroy();

            if (_ownsOverlayCanvas && _overlayCanvasRect != null)
                _overlayCanvasRect.gameObject.SafeDestroy();
        }
    }
}
