using Core.Data.Domain.Enums;
using Core.Infrastructure.Mediator.Abstractions;

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
