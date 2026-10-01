using System;
using Core.Abstractions.MVVM.Elements;
using Core.Infrastructure.Mediator.Abstractions;
using Core.Data.Domain.Models.Elements;
using UnityEngine;

namespace Core.Application.Commands.Elements
{
    public readonly struct CreateElementCommand<TElement, TData> : IRequest<TElement>
        where TElement : MonoBehaviour, IBindableElement<TData>
        where TData : struct, IElementData
    {
        public readonly Transform Container;
        public readonly TData Data;
        public readonly Action<TData> OnClick;

        public CreateElementCommand(Transform container, TData data, Action<TData> onClick = null)
        {
            Container = container;
            Data = data;
            OnClick = onClick;
        }
    }
}
