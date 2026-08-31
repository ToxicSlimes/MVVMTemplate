namespace Core.Network.Abstractions.Types
{
    public static class NetworkPrefabKeys
    {
        public const string GameSessionName = "GameSessionController";
        public const string NetworkPlayerName = "NetworkPlayer";
        public const string PlayerAvatarName = "PlayerAvatar";
        public const string BotAvatarName = "BotAvatar";

        public static NetworkPrefabKey GameSession => new(GameSessionName);

        public static NetworkPrefabKey NetworkPlayer => new(NetworkPlayerName);

        public static NetworkPrefabKey PlayerAvatar => new(PlayerAvatarName);

        public static NetworkPrefabKey BotAvatar => new(BotAvatarName);
    }
}
