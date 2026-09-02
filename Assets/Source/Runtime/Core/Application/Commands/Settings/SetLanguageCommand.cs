using Core.Data.Domain.Enums;
using Core.Infrastructure.Mediator.Abstractions;

namespace Core.Application.Commands.Settings
{
    public readonly struct SetLanguageCommand : IRequest<Nothing>
    {
        public GameLanguage Language { get; }

        public SetLanguageCommand(GameLanguage language)
        {
            Language = language;
        }
    }
}
