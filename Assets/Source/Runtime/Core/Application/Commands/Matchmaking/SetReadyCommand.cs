using Core.Mediator.Abstractions;

namespace Core.Application.Commands.Matchmaking
{
    public readonly struct SetReadyCommand : IRequest<Nothing>
    {
        public readonly bool IsReady;

        public SetReadyCommand(bool isReady)
        {
            IsReady = isReady;
        }
    }
}
