using System;
using System.Collections.Generic;
using Core.Domain.UI;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Core.Abstractions.Popup
{
    public interface IPopupService
    {
        UniTask<TPopup> ShowAsync<TPopup, TData>(
            RectTransform target,
            TData data,
            PopupShowParams showParams = null
        ) where TPopup : class, IPopup<TData> where TData : IPopupData;

        UniTask<TPopup> ShowAsync<TPopup>(
            RectTransform target,
            PopupShowParams showParams = null
        ) where TPopup : class, IPopup;

        UniTask<IPopup> ShowFromPrefabAsync(
            GameObject prefab,
            RectTransform target,
            PopupShowParams showParams = null);

        UniTask<IPopup> ShowFromPrefabAsync(
            GameObject prefab,
            RectTransform target,
            IPopupData data,
            PopupShowParams showParams = null);

        UniTask HideAsync(IPopup popup);
        
        UniTask HideAllAsync();
        
        UniTask HideTopPopupAsync();
        
        void HideTopPopup();
        
        void HideAll();
        
        void RegisterPrefab<TPopup>(GameObject prefab) where TPopup : class, IPopup;
        
        IReadOnlyCollection<IPopup> ActivePopups { get; }
        
        event Action<IPopup> PopupShown;
        event Action<IPopup> PopupHidden;
    }
}
