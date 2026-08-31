using Fusion;

namespace Core.Network.FusionAdapter.Player
{
    public static class BotIdentity
    {
        public const int NoBot = 0;

        private const int ReservedBase = 64;

        public static PlayerRef FromNumber(int number) => PlayerRef.FromIndex(ReservedBase + number - 1);

        public static bool IsBot(PlayerRef player) => player.AsIndex >= ReservedBase;
    }
}
