using System;
using System.Collections.Generic;
using Core.Abstractions.MVVM.ViewModels;
using Core.Abstractions.MVVM.Views;
using Cysharp.Threading.Tasks;

namespace Core.Abstractions.MVVM.Services
{
    public interface IViewService
    {
        IView CurrentView { get; }
        IReadOnlyCollection<IView> ViewHistory { get; }
        IReadOnlyCollection<IView> ActiveViews { get; }
        UniTask<IView> ShowAsync<TViewModel>(ViewShowParams showParams = null) where TViewModel : DisposableObject;
        UniTask<TView> ShowAsync<TView, TViewModel>(bool hideCurrentView = true) where TView : BindableView<TViewModel> where TViewModel : DisposableObject;
        UniTask<TView> ShowAsync<TView, TViewModel>(ViewShowParams showParams) where TView : BindableView<TViewModel> where TViewModel : DisposableObject;
        UniTask<bool> HideViewAsync<TViewModel>() where TViewModel : DisposableObject;
        UniTask HideAsync<TView>() where TView : IView;
        UniTask HideAsync(IView view);
        TViewModel GetViewModel<TViewModel>() where TViewModel : DisposableObject;
        UniTask HideCurrentAsync();
        UniTask GoBackAsync();
        UniTask GoToRootAsync();
        UniTask CloseAllNonPersistentAsync();
        void ClearHistory();
        void ClearCache();
        bool IsViewActive<TView>() where TView : IView;
        TView GetActiveView<TView>() where TView : IView;
        event Action<IView> ViewShown;
        event Action<IView> ViewHidden;
    }
}