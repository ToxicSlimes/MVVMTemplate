using System;
using System.Collections.Generic;
using Core.Abstractions.MVVM.Elements;
using Core.Mediator.Abstractions;
using Core.Domain.Models.Elements;
using UnityEngine;

namespace Core.Application.Commands.Elements
{
    public readonly struct ReconcileElementsCommand<TElement, TData> : IRequest<Nothing>
        where TElement : MonoBehaviour, IBindableElement<TData>
        where TData : struct, IElementData, IEquatable<TData>
    {
        public readonly Transform Container;
        public readonly IReadOnlyList<TData> NewData;
        public readonly Action<TData> OnClick;

        public ReconcileElementsCommand(
            Transform container,
            IReadOnlyList<TData> newData,
            Action<TData> onClick = null)
        {
            Container = container;
            NewData = newData;
            OnClick = onClick;
        }
    }
}
