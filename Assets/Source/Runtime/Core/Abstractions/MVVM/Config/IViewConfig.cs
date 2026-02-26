using System;
using Core.Abstractions.MVVM.ViewModels;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Core.Abstractions.MVVM.Config
{
    public interface IViewConfig
    {
        string GetViewAddress<TViewModel>() where TViewModel : DisposableObject;
        string GetViewAddress(Type viewModelType);
        AssetReferenceT<GameObject> GetViewReference<TViewModel>() where TViewModel : class;
        AssetReferenceT<GameObject> GetViewReference(Type viewModelType);
    }
}