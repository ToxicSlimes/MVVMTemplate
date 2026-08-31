using System.Threading;
using Core.Abstractions.MVVM.Services;
using Core.Infrastructure.Mediator.Abstractions;
using Core.Infrastructure.Mediator.Handlers;
using Cysharp.Threading.Tasks;
using Core.Application.Commands.Elements;

namespace Core.Application.Handlers.Elements
{
    public class DestroyElementHandler : IRequestHandler<DestroyElementCommand, Nothing>
    {
        private readonly IUIElementService _elementService;

        public DestroyElementHandler(IUIElementService elementService)
        {
            _elementService = elementService;
        }

        public UniTask<Nothing> Handle(DestroyElementCommand request, CancellationToken cancellationToken)
        {
            _elementService.DestroyElement(request.Element);
            return NothingTask.Completed;
        }
    }
}
