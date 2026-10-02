using System.Threading;
using Core.Abstractions.Services;
using Core.Application.Queries.Settings;
using Core.Mediator.Abstractions;
using Core.Mediator.Handlers;
using Cysharp.Threading.Tasks;

namespace Core.Application.Handlers.Settings
{
    public sealed class GetLocalizationStateHandler : IRequestHandler<GetLocalizationStateQuery, LocalizationState>
    {
        private readonly ILocalizationService _localizationService;

        public GetLocalizationStateHandler(ILocalizationService localizationService)
        {
            _localizationService = localizationService;
        }

        public UniTask<LocalizationState> Handle(GetLocalizationStateQuery query, CancellationToken ct)
        {
            return UniTask.FromResult(new LocalizationState(_localizationService.CurrentLanguage));
        }
    }
}
