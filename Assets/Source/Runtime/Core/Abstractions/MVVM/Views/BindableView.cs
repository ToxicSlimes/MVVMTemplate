using System;
using Core.Abstractions.MVVM.ViewModels;
using Cysharp.Threading.Tasks;
using R3;
using UnityEngine;

namespace Core.Abstractions.MVVM.Views
{
    public abstract class BindableView<TViewModel> : MonoBehaviour, IView, IShowable, IHideable, IUnbindable where TViewModel : DisposableObject

    {
    [SerializeField] private CanvasGroup _canvasGroup;
    [SerializeField] private bool _isPersistent;

    public TViewModel ViewModel => _viewModel.Value;
    
    IViewModel IView.ViewModel => _viewModel.Value;

    public bool IsVisible { get; protected set; }

    public virtual bool IsPersistent => _isPersistent;

    private readonly ReactiveProperty<TViewModel> _viewModel = new(null);
    private DisposableBag _disposables;

    protected bool IsQuitting { get; private set; }

    protected virtual void OnApplicationQuit() => IsQuitting = true;

    public IDisposable BindTo(TViewModel viewModel)
    {
        _disposables.Dispose();
        _disposables = new DisposableBag();

        _viewModel.Value = viewModel;

        OnBind(ref _disposables);

        return _disposables;
    }

    public Observable<TViewModel> ObserveViewModel(bool includeCurrent = true)
    {
        return includeCurrent ? _viewModel : _viewModel.Skip(1);
    }

    protected abstract void OnBind(ref DisposableBag disposables);

    /// <summary>
    /// Вьюмодель здесь не освобождается: её создаёт и держит контейнер, а вьюха только
    /// подписывается. Освободи её тут — и повторный показ экрана получит уже мёртвый синглтон.
    /// </summary>
    public virtual void Unbind()
    {
        _viewModel.Value = null;
        _disposables.Dispose();
        _disposables = new DisposableBag();
    }

    public virtual async UniTask ShowAsync()
    {
        gameObject.SetActive(true);
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
        IsVisible = false;
        gameObject.SetActive(false);
    }

    protected virtual UniTask OnBeforeShow() => UniTask.CompletedTask;

    protected virtual UniTask OnAfterShow() => UniTask.CompletedTask;

    protected virtual UniTask OnBeforeHide() => UniTask.CompletedTask;

    protected virtual UniTask OnAfterHide() => UniTask.CompletedTask;

    protected virtual UniTask PlayShowAnimation()
    {
        if (_canvasGroup != null)
        {
            _canvasGroup.alpha = 1f;
            _canvasGroup.interactable = true;
            _canvasGroup.blocksRaycasts = true;
        }
        return UniTask.CompletedTask;
    }

    protected virtual UniTask PlayHideAnimation()
    {
        if (_canvasGroup != null)
        {
            _canvasGroup.alpha = 0f;
            _canvasGroup.interactable = false;
            _canvasGroup.blocksRaycasts = false;
        }
        return UniTask.CompletedTask;
    }

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
    }
}
