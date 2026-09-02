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
        [SerializeField] private VisualTreeAsset _layout;
        [SerializeField] private StyleSheet[] _styleSheets;
        [SerializeField] private bool _isPersistent;

        private readonly ReactiveProperty<TViewModel> _viewModel = new(null);
        private DisposableBag _disposables;

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

        public IDisposable BindTo(TViewModel viewModel)
        {
            _disposables.Dispose();
            _disposables = new DisposableBag();
            EnsureVisualTreeReady();
            _viewModel.Value = viewModel;
            OnBind(ref _disposables);
            return _disposables;
        }

        public Observable<TViewModel> ObserveViewModel(bool includeCurrent = true)
        {
            return includeCurrent ? _viewModel : _viewModel.Skip(1);
        }

        protected abstract void OnBind(ref DisposableBag disposables);

        public virtual void Unbind()
        {
            _viewModel.Value = null;
            _disposables.Dispose();
            _disposables = new DisposableBag();
        }

        public virtual async UniTask ShowAsync()
        {
            gameObject.SetActive(true);
            EnsureVisualTreeReady();
            if (Root != null)
            {
                Root.style.display = DisplayStyle.Flex;
                Root.style.opacity = 1f;
            }

            if (_viewModel.Value != null)
            {
                _disposables.Dispose();
                _disposables = new DisposableBag();
                OnBind(ref _disposables);
            }

            IsVisible = true;
            await OnBeforeShow();
            await PlayShowAnimation();
            await OnAfterShow();
        }

        public virtual async UniTask HideAsync()
        {
            await OnBeforeHide();
            await PlayHideAnimation();
            await OnAfterHide();

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
            Unbind();
            _disposables.Dispose();
        }

        public void Dispose()
        {
            _viewModel?.Dispose();
            _disposables.Dispose();
        }

        private void EnsureVisualTreeReady()
        {
            if (!gameObject.activeSelf)
                gameObject.SetActive(true);

            EnsureRoot();
        }

        private void EnsureRoot()
        {
            if (_uiDocument == null)
                _uiDocument = GetComponent<UIDocument>();

            if (_uiDocument == null)
                return;

            ToolkitPanelSettingsBootstrap.Apply(_uiDocument);

            if (_layout != null && _uiDocument.visualTreeAsset != _layout)
                _uiDocument.visualTreeAsset = _layout;

            Root = _uiDocument.rootVisualElement;
            if (Root == null)
                return;

            if (Root.childCount == 0 && _layout != null)
                _layout.CloneTree(Root);

            Root.style.flexGrow = 1f;
            ApplyStyleSheets();
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
