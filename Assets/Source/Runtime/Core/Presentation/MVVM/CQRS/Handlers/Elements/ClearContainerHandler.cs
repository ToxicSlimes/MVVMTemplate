using System.Threading;
using Core.Abstractions.MVVM.Services;
using Core.Mediator.Abstractions;
using Core.Mediator.Handlers;
using Cysharp.Threading.Tasks;
using Core.Application.Commands.Elements;

namespace Core.Application.Handlers.Elements
{
    public class ClearContainerHandler : IRequestHandler<ClearContainerCommand, Nothing>
    {
        private readonly IUIElementService _elementService;

        public ClearContainerHandler(IUIElementService elementService)
        {
            _elementService = elementService;
        }

        public UniTask<Nothing> Handle(ClearContainerCommand request, CancellationToken cancellationToken)
        {
            _elementService.ClearContainer(request.Container);
            return NothingTask.Completed;
        }
    }
}
