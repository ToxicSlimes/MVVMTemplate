using System;
using Core.Abstractions.MVVM.ViewModels;
using Core.Abstractions.MVVM.Views;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Core.Abstractions.MVVM.Factory
{
    public interface IViewFactory
    {
        UniTask<IView> CreateAsync(Type viewModelType, ViewShowParams showParams = null);
        UniTask<TView> CreateAsync<TView, TViewModel>(Transform parent = null) where TView : BindableView<TViewModel> where TViewModel : DisposableObject;
        UniTask<TView> CreateAsync<TView, TViewModel>(string address, Transform parent = null) where TView : BindableView<TViewModel> where TViewModel : DisposableObject;
        UniTask<TView> CreateAsync<TView, TViewModel>(ViewShowParams showParams) where TView : BindableView<TViewModel> where TViewModel : DisposableObject;
        void Destroy(IView view);
    }
}
