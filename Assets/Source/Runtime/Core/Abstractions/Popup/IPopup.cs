using System;
using Core.Domain.UI;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Core.Abstractions.Popup
{


    public interface IPopup : IDisposable
    {
        RectTransform RectTransform { get; }
        RectTransform Target { get; }
        bool IsVisible { get; }
        
        void SetTarget(RectTransform target);
        void SetCanvasRect(RectTransform canvasRect);
        void SetMousePositionProvider(Func<Vector2> provider);
        void UpdatePosition(PopupShowParams showParams);
        
        UniTask ShowAsync();
        UniTask HideAsync();
        
        void OnGetFromPool();
        void OnReturnToPool();
        
        void SetData(IPopupData data);
    }

    public interface IPopup<TData> : IPopup where TData : IPopupData
    {
        TData Data { get; }
        void SetData(TData data);
    }
}
