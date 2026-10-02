using System.Threading;
using Core.Abstractions.MVVM.Services;
using Core.Application.Commands.Common;
using Core.Mediator.Abstractions;
using Core.Mediator.Handlers;
using Cysharp.Threading.Tasks;

namespace Core.Application.Handlers.Common
{
    public sealed class NavigateBackHandler : IRequestHandler<NavigateBackCommand, Nothing>
    {
        private readonly IViewService _viewService;

        public NavigateBackHandler(IViewService viewService)
        {
            _viewService = viewService;
        }

        public async UniTask<Nothing> Handle(NavigateBackCommand command, CancellationToken ct)
        {
            await _viewService.GoBackAsync();
            return Nothing.Value;
        }
    }
}
