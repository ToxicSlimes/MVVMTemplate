namespace Core.Application.Notifications.Matchmaking
{
    /// <summary>
    /// Чем кончился матч. <see cref="None"/> — это «ещё не кончился», а не третья концовка:
    /// по нему хост отличает нерешённый матч от уже завершённого и не завершает его дважды.
    /// </summary>
    public enum MatchOutcome
    {
        None = 0,
        Victory,
        Defeat
    }
}
