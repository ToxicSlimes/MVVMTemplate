using System;
using System.Threading;
using Core.Abstractions.MVVM.Elements;
using Core.Abstractions.MVVM.Services;
using Core.Infrastructure.Mediator.Abstractions;
using Core.Infrastructure.Mediator.Handlers;
using Cysharp.Threading.Tasks;
using Core.Data.Domain.Models.Elements;
using Core.Application.Commands.Elements;
using UnityEngine;

namespace Core.Application.Handlers.Elements
{
    public class ReconcileElementsHandler<TElement, TData> : IRequestHandler<ReconcileElementsCommand<TElement, TData>, Nothing>
        where TElement : MonoBehaviour, IBindableElement<TData>
        where TData : struct, IElementData, IEquatable<TData>
    {
        private readonly IUIElementService _elementService;

        public ReconcileElementsHandler(IUIElementService elementService)
        {
            _elementService = elementService;
        }

        public async UniTask<Nothing> Handle(ReconcileElementsCommand<TElement, TData> request, CancellationToken cancellationToken)
        {
            await _elementService.ReconcileElementsAsync<TElement, TData>(
                request.Container,
                request.NewData,
                request.OnClick);

            return Nothing.Value;
        }
    }
}
