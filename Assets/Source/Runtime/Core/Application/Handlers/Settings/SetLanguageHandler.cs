using System.Threading;
using Core.Abstractions.Services;
using Core.Application.Commands.Settings;
using Core.Infrastructure.Mediator.Abstractions;
using Core.Infrastructure.Mediator.Handlers;
using Cysharp.Threading.Tasks;

namespace Core.Application.Handlers.Settings
{
    public sealed class SetLanguageHandler : IRequestHandler<SetLanguageCommand, Nothing>
    {
        private readonly ILocalizationService _localizationService;

        public SetLanguageHandler(ILocalizationService localizationService)
        {
            _localizationService = localizationService;
        }

        public async UniTask<Nothing> Handle(SetLanguageCommand command, CancellationToken ct)
        {
            await _localizationService.SetLanguageAsync(command.Language, ct);
            return Nothing.Value;
        }
    }
}
