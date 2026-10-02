using System.Threading;
using Core.Abstractions.Services.Platform;
using Core.Mediator.Handlers;
using Cysharp.Threading.Tasks;
using Core.Application.Queries.Common;

namespace Core.Application.Handlers.Common
{
    public sealed class ConfirmDialogHandler : IRequestHandler<ConfirmDialogQuery, bool>
    {
        private readonly IDialogService _dialogService;

        public ConfirmDialogHandler(IDialogService dialogService)
        {
            _dialogService = dialogService;
        }

        public async UniTask<bool> Handle(ConfirmDialogQuery query, CancellationToken ct)
        {
            return await _dialogService.ConfirmAsync(
                query.Title,
                query.Message,
                query.ConfirmText,
                query.CancelText);
        }
    }
}
