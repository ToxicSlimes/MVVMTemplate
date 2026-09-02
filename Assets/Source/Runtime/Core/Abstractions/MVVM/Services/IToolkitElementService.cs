using System;
using System.Collections.Generic;
using Core.Abstractions.MVVM.Elements;
using Core.Data.Domain.Models.Elements;
using Cysharp.Threading.Tasks;
using UnityEngine.UIElements;

namespace Core.Abstractions.MVVM.Services
{
    public interface IToolkitElementService
    {
        UniTask<TElement> CreateElementAsync<TElement, TData>(
            VisualElement container,
            TData data,
            Action<TData> onClick = null)
            where TElement : class, IToolkitBindableElement<TData>
            where TData : struct, IElementData;

        void DestroyElement(IToolkitBindableElement element);

        void ClearContainer(VisualElement container);

        IReadOnlyList<TElement> GetElementsInContainer<TElement, TData>(VisualElement container)
            where TElement : class, IToolkitBindableElement<TData>
            where TData : struct, IElementData;

        UniTask ReconcileElementsAsync<TElement, TData>(
            VisualElement container,
            IReadOnlyList<TData> newData,
            Action<TData> onClick = null)
            where TElement : class, IToolkitBindableElement<TData>
            where TData : struct, IElementData, IEquatable<TData>;
    }
}
