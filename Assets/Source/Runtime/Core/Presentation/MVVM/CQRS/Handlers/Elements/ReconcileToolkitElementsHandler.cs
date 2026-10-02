using System.Threading;
using Core.Abstractions.MVVM.Elements;
using Core.Abstractions.MVVM.Services;
using Core.Application.Commands.Elements;
using Core.Domain.Models.Elements;
using Core.Mediator.Abstractions;
using Core.Mediator.Handlers;
using Cysharp.Threading.Tasks;

namespace Core.Application.Handlers.Elements
{
    public class ReconcileToolkitElementsHandler<TElement, TData> : IRequestHandler<ReconcileToolkitElementsCommand<TElement, TData>, Nothing>
        where TElement : class, IToolkitBindableElement<TData>
        where TData : struct, IElementData, System.IEquatable<TData>
    {
        private readonly IToolkitElementService _elementService;

        public ReconcileToolkitElementsHandler(IToolkitElementService elementService)
        {
            _elementService = elementService;
        }

        public async UniTask<Nothing> Handle(ReconcileToolkitElementsCommand<TElement, TData> request, CancellationToken cancellationToken)
        {
            await _elementService.ReconcileElementsAsync<TElement, TData>(
                request.Container,
                request.NewData,
                request.OnClick);

            return Nothing.Value;
        }
    }
}
