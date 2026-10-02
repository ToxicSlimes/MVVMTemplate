using Core.Mediator.Abstractions;

namespace Core.Application.Commands.Matchmaking
{
    /// <summary>
    /// Заходит в лобби, если ещё не там. Список приходит пушем через
    /// <c>NetworkSessionListUpdatedNotification</c>, поэтому команда ничего не возвращает:
    /// иначе вызывающему пришлось бы знать сетевые типы.
    /// </summary>
    public readonly struct RefreshSessionListCommand : IRequest<Nothing>
    {
    }
}
