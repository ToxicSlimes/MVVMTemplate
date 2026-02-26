using System;
using System.Collections.Generic;
using Core.Abstractions.MVVM.Elements;
using Cysharp.Threading.Tasks;
using Core.Data.Domain.Models.Elements;
using UnityEngine;

namespace Core.Abstractions.MVVM.Services
{
    public interface IUIElementService
    {
        UniTask<TElement> CreateElementAsync<TElement, TData>(
            Transform container,
            TData data,
            Action<TData> onClick = null)
            where TElement : MonoBehaviour, IBindableElement<TData>
            where TData : struct, IElementData;

        void DestroyElement(IBindableElement element);

        void ClearContainer(Transform container);

        IReadOnlyList<TElement> GetElementsInContainer<TElement, TData>(Transform container)
            where TElement : MonoBehaviour, IBindableElement<TData>
            where TData : struct, IElementData;

        UniTask ReconcileElementsAsync<TElement, TData>(
            Transform container,
            IReadOnlyList<TData> newData,
            Action<TData> onClick = null)
            where TElement : MonoBehaviour, IBindableElement<TData>
            where TData : struct, IElementData, IEquatable<TData>;
    }
}
