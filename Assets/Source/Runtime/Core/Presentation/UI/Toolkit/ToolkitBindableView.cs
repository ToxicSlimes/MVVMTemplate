using System;
using Core.Abstractions.MVVM.ViewModels;
using Core.Abstractions.MVVM.Views;
using Cysharp.Threading.Tasks;
using R3;
using UnityEngine;
using UnityEngine.UIElements;

namespace Core.UI.Toolkit
{
    public abstract class ToolkitBindableView<TViewModel> : MonoBehaviour, IView, IShowable, IHideable, IUnbindable
        where TViewModel : DisposableObject
    {
        [SerializeField] private UIDocument _uiDocument;
#if UNITY_6000_5_OR_NEWER
        [SerializeField] private PanelRenderer _panelRenderer;
        private bool _rendererRegistered;
        private int _rendererVersion;
        private bool _hasRendererVersion;
#endif
        [SerializeField] private VisualTreeAsset _layout;
        [SerializeField] private StyleSheet[] _styleSheets;
        [SerializeField] private bool _isPersistent;

        private readonly ReactiveProperty<TViewModel> _viewModel = new(null);
        private DisposableBag _disposables;
        private bool _bound;
        private bool _destroyed;
        private int _visibilityOperation;
        private int _bindingGeneration;

        public TViewModel ViewModel => _viewModel.Value;
        IViewModel IView.ViewModel => _viewModel.Value;
        public bool IsVisible { get; protected set; }
        public virtual bool IsPersistent => _isPersistent;
        protected VisualElement Root { get; private set; }
        protected UIDocument Document => _uiDocument;

        protected bool IsQuitting { get; private set; }

        protected virtual void Awake()
        {
            ToolkitPanelSettingsBootstrap.Apply(_uiDocument);
            EnsureRoot();
        }

        protected virtual void OnApplicationQuit() => IsQuitting = true;

        protected virtual void OnEnable()
        {
            EnsureRoot();
            BindCurrent();
        }

        protected virtual void OnDisable()
        {
            ++_visibilityOperation;
            ReleaseBindings();
#if UNITY_6000_5_OR_NEWER
            if (_rendererRegistered && _panelRenderer != null)
                _panelRenderer.UnregisterUIReloadCallback(OnRendererReload);
            _rendererRegistered = false;
            _hasRendererVersion = false;
            if (_uiDocument == null && _panelRenderer != null)
                SetRoot(null);
#endif
        }

        public IDisposable BindTo(TViewModel viewModel)
        {
            if (_destroyed)
                throw new ObjectDisposedException(GetType().Name);
            var generation = ++_bindingGeneration;
            ReleaseBindings();
            _viewModel.Value = viewModel;
            EnsureVisualTreeReady();
            BindCurrent();
            return new BindingHandle(this, generation);
        }

        public Observable<TViewModel> ObserveViewModel(bool includeCurrent = true)
        {
            return includeCurrent ? _viewModel : _viewModel.Skip(1);
        }

        protected abstract void OnBind(ref DisposableBag disposables);

        public virtual void Unbind()
        {
            ++_bindingGeneration;
            if (_destroyed && _viewModel.Value == null) return;
            _viewModel.Value = null;
            ReleaseBindings();
        }

        private sealed class BindingHandle : IDisposable
        {
            private ToolkitBindableView<TViewModel> _owner;
            private readonly int _generation;

            public BindingHandle(ToolkitBindableView<TViewModel> owner, int generation)
            {
                _owner = owner;
                _generation = generation;
            }

            public void Dispose()
            {
                var owner = _owner;
                _owner = null;
                if (owner != null && !owner._destroyed && owner._bindingGeneration == _generation)
                    owner.Unbind();
            }
        }

        public virtual async UniTask ShowAsync()
        {
            var operation = ++_visibilityOperation;
            gameObject.SetActive(true);
            EnsureVisualTreeReady();
            if (Root != null)
            {
                Root.style.display = DisplayStyle.Flex;
                Root.style.opacity = 1f;
            }

            BindCurrent();

            IsVisible = true;
            await OnBeforeShow();
            if (_destroyed || operation != _visibilityOperation) return;
            await PlayShowAnimation();
            if (_destroyed || operation != _visibilityOperation) return;
            await OnAfterShow();
        }

        public virtual async UniTask HideAsync()
        {
            var operation = ++_visibilityOperation;
            await OnBeforeHide();
            if (_destroyed || operation != _visibilityOperation) return;
            await PlayHideAnimation();
            if (_destroyed || operation != _visibilityOperation) return;
            await OnAfterHide();
            if (_destroyed || operation != _visibilityOperation) return;

            if (Root != null)
            {
                Root.style.display = DisplayStyle.None;
                Root.style.opacity = 0f;
            }

            IsVisible = false;
            gameObject.SetActive(false);
        }

        protected virtual UniTask OnBeforeShow() => UniTask.CompletedTask;
        protected virtual UniTask OnAfterShow() => UniTask.CompletedTask;
        protected virtual UniTask OnBeforeHide() => UniTask.CompletedTask;
        protected virtual UniTask OnAfterHide() => UniTask.CompletedTask;

        protected virtual UniTask PlayShowAnimation() => UniTask.CompletedTask;
        protected virtual UniTask PlayHideAnimation() => UniTask.CompletedTask;

        protected virtual void OnDestroy()
        {
            Dispose();
        }

        public void Dispose()
        {
            if (_destroyed) return;
            _destroyed = true;
            ++_visibilityOperation;
            Unbind();
#if UNITY_6000_5_OR_NEWER
            if (_rendererRegistered && _panelRenderer != null)
                _panelRenderer.UnregisterUIReloadCallback(OnRendererReload);
            _rendererRegistered = false;
            _hasRendererVersion = false;
#endif
            SetRoot(null);
            _viewModel.Dispose();
        }

        private void EnsureVisualTreeReady()
        {
            if (!gameObject.activeSelf)
                gameObject.SetActive(true);

            EnsureRoot();
        }

        private void EnsureRoot()
        {
            if (_destroyed)
                return;
            if (_uiDocument == null)
                _uiDocument = GetComponent<UIDocument>();

#if UNITY_6000_5_OR_NEWER
            if (_uiDocument == null)
            {
                if (_panelRenderer == null)
                    _panelRenderer = GetComponent<PanelRenderer>();
                if (_panelRenderer == null || !isActiveAndEnabled)
                    return;
                ToolkitPanelSettingsBootstrap.Apply(_panelRenderer);
                if (_layout != null && _panelRenderer.visualTreeAsset != _layout)
                    _panelRenderer.visualTreeAsset = _layout;
                if (!_rendererRegistered)
                {
                    _rendererRegistered = true;
                    _panelRenderer.RegisterUIReloadCallback(OnRendererReload);
                }
                return;
            }
#endif

            if (_uiDocument == null)
                return;

            ToolkitPanelSettingsBootstrap.Apply(_uiDocument);

            if (_layout != null && _uiDocument.visualTreeAsset != _layout)
                _uiDocument.visualTreeAsset = _layout;

            SetRoot(_uiDocument.rootVisualElement);
        }

#if UNITY_6000_5_OR_NEWER
        private void OnRendererReload(PanelRenderer renderer, VisualElement root, int version)
        {
            if (_destroyed || !_rendererRegistered || renderer != _panelRenderer)
                return;
            if (_hasRendererVersion && version == _rendererVersion && ReferenceEquals(root, Root))
                return;
            _hasRendererVersion = true;
            _rendererVersion = version;
            ReleaseBindings();
            SetRoot(root);
            BindCurrent();
        }
#endif

        private void SetRoot(VisualElement root)
        {
            if (!ReferenceEquals(Root, root))
            {
                Root?.UnregisterCallback<AttachToPanelEvent>(OnRootAttachedToPanel);
                ReleaseBindings();
                Root = root;
            }
            if (Root == null)
                return;

#if UNITY_6000_5_OR_NEWER
            if ((_uiDocument != null || _panelRenderer == null) && Root.childCount == 0 && _layout != null)
#else
            if (Root.childCount == 0 && _layout != null)
#endif
                _layout.CloneTree(Root);

            Root.style.flexGrow = 1f;
            ApplyStyleSheets();
            EnsureDropdownPopupStyles(Root);
            if (IsVisible)
            {
                Root.style.display = DisplayStyle.Flex;
                Root.style.opacity = 1f;
            }
        }

        private void ReleaseBindings()
        {
            _disposables.Dispose();
            _disposables = new DisposableBag();
            _bound = false;
        }

        private void BindCurrent()
        {
            if (_bound || _destroyed || Root == null || _viewModel.Value == null || !isActiveAndEnabled)
                return;
            _bound = true;
            OnBind(ref _disposables);
        }

        private static void EnsureDropdownPopupStyles(VisualElement root)
        {
            if (root == null)
                return;

            if (root.panel?.visualTree != null)
            {
                ToolkitDropdownPopupStyles.EnsureApplied(root.panel.visualTree);
                return;
            }

            root.RegisterCallback<AttachToPanelEvent>(OnRootAttachedToPanel);
        }

        private static void OnRootAttachedToPanel(AttachToPanelEvent evt)
        {
            var target = evt.currentTarget as VisualElement;
            target?.UnregisterCallback<AttachToPanelEvent>(OnRootAttachedToPanel);
            if (evt.destinationPanel?.visualTree != null)
                ToolkitDropdownPopupStyles.EnsureApplied(evt.destinationPanel.visualTree);
        }

        private void ApplyStyleSheets()
        {
            if (Root == null || _styleSheets == null)
                return;

            foreach (var sheet in _styleSheets)
            {
                if (sheet != null && !Root.styleSheets.Contains(sheet))
                    Root.styleSheets.Add(sheet);
            }
        }
    }
}
