using System.Threading;
using Core.Abstractions.MVVM.Elements;
using Core.Abstractions.MVVM.Services;
using Core.Infrastructure.Mediator.Handlers;
using Cysharp.Threading.Tasks;
using Core.Data.Domain.Models.Elements;
using Core.Application.Commands.Elements;
using UnityEngine;

namespace Core.Application.Handlers.Elements
{
    public class CreateElementHandler<TElement, TData> : IRequestHandler<CreateElementCommand<TElement, TData>, TElement>
        where TElement : MonoBehaviour, IBindableElement<TData>
        where TData : struct, IElementData
    {
        private readonly IUIElementService _elementService;

        public CreateElementHandler(IUIElementService elementService)
        {
            _elementService = elementService;
        }

        public async UniTask<TElement> Handle(CreateElementCommand<TElement, TData> request, CancellationToken cancellationToken)
        {
            var element = await _elementService.CreateElementAsync<TElement, TData>(
                request.Container,
                request.Data,
                request.OnClick);

            return element;
        }
    }
}
