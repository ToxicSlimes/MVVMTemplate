using System.Threading;
using Core.Abstractions.UI;
using Core.Application.Commands.UI;
using Core.Infrastructure.Mediator.Abstractions;
using Core.Infrastructure.Mediator.Handlers;
using Cysharp.Threading.Tasks;

namespace Core.Application.Handlers.UI
{
    public sealed class ApplyThemeHandler : IRequestHandler<ApplyThemeCommand, Nothing>
    {
        private readonly IUIControlService _controlService;

        public ApplyThemeHandler(IUIControlService controlService)
        {
            _controlService = controlService;
        }

        public UniTask<Nothing> Handle(ApplyThemeCommand command, CancellationToken ct)
        {
            _controlService.ApplyThemeToAll();
            return UniTask.FromResult(Nothing.Value);
        }
    }
}
