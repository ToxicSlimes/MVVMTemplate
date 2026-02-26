using System.Threading;
using Core.Abstractions.Services.Platform;
using Core.Application.Commands.Platform;
using Core.Infrastructure.Mediator.Abstractions;
using Core.Infrastructure.Mediator.Handlers;
using Cysharp.Threading.Tasks;

namespace Core.Application.Handlers.Platform
{
    public sealed class CopyToClipboardHandler : IRequestHandler<CopyToClipboardCommand, Nothing>
    {
        private readonly IClipboardService _clipboardService;

        public CopyToClipboardHandler(IClipboardService clipboardService)
        {
            _clipboardService = clipboardService;
        }

        public UniTask<Nothing> Handle(CopyToClipboardCommand command, CancellationToken ct)
        {
            _clipboardService.SetText(command.Text);
            return UniTask.FromResult(Nothing.Value);
        }
    }
}
