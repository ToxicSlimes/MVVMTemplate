using Core.Data.Domain.Enums;
using Core.Infrastructure.Mediator.Abstractions;

namespace Core.Application.Notifications.Matchmaking
{
    /// <summary>
    /// Приходит на каждом пире: фаза реплицирована, и вход в состояние происходит у всех.
    /// </summary>
    public readonly struct SessionPhaseChangedNotification : INotification
    {
        public readonly SessionPhase Phase;

        public SessionPhaseChangedNotification(SessionPhase phase)
        {
            Phase = phase;
        }
    }
}
