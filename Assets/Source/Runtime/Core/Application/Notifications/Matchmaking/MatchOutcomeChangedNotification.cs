using Core.Infrastructure.Mediator.Abstractions;

namespace Core.Application.Notifications.Matchmaking
{
    /// <summary>
    /// Исход матча. Отдельно от <see cref="SessionPhaseChangedNotification"/>: фаза
    /// <c>Ended</c> одинакова для победы и поражения, а показывать их надо по-разному.
    /// </summary>
    public readonly struct MatchOutcomeChangedNotification : INotification
    {
        public readonly MatchOutcome Outcome;

        public MatchOutcomeChangedNotification(MatchOutcome outcome)
        {
            Outcome = outcome;
        }
    }
}
