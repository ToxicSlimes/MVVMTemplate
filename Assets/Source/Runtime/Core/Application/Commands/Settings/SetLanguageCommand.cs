using Core.Domain.Enums;
using Core.Mediator.Abstractions;

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
