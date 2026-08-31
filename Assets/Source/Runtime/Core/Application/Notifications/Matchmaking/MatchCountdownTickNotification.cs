using Core.Infrastructure.Mediator.Abstractions;

namespace Core.Application.Notifications.Matchmaking
{
    public readonly struct MatchCountdownTickNotification : INotification
    {
        public readonly int SecondsLeft;

        public MatchCountdownTickNotification(int secondsLeft)
        {
            SecondsLeft = secondsLeft;
        }
    }
}
