using Core.Domain.Enums;
using Core.Mediator.Abstractions;

namespace Core.Application.Queries.Settings
{
    public readonly struct GetLocalizationStateQuery : IRequest<LocalizationState>
    {
    }

    public readonly struct LocalizationState
    {
        public GameLanguage CurrentLanguage { get; }

        public LocalizationState(GameLanguage currentLanguage)
        {
            CurrentLanguage = currentLanguage;
        }
    }
}
