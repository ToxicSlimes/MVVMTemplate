using System;
using System.Collections.Generic;
using Core.Abstractions.MVVM.Elements;
using Core.Infrastructure.Mediator.Abstractions;
using Core.Data.Domain.Models.Elements;
using UnityEngine.UIElements;

namespace Core.Application.Commands.Elements
{
    public readonly struct ReconcileToolkitElementsCommand<TElement, TData> : IRequest<Nothing>
        where TElement : class, IToolkitBindableElement<TData>
        where TData : struct, IElementData, IEquatable<TData>
    {
        public readonly VisualElement Container;
        public readonly IReadOnlyList<TData> NewData;
        public readonly Action<TData> OnClick;

        public ReconcileToolkitElementsCommand(
            VisualElement container,
            IReadOnlyList<TData> newData,
            Action<TData> onClick = null)
        {
            Container = container;
            NewData = newData;
            OnClick = onClick;
        }
    }
}
