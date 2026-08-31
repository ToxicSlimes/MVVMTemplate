namespace Core.Network.Abstractions.Types
{
    public readonly struct NetworkTarget
    {
        public static readonly NetworkTarget All = new(NetworkTargetKind.All, NetworkPlayerId.None);
        public static readonly NetworkTarget StateAuthority = new(NetworkTargetKind.StateAuthority, NetworkPlayerId.None);

        public readonly NetworkTargetKind Kind;
        public readonly NetworkPlayerId Player;

        private NetworkTarget(NetworkTargetKind kind, NetworkPlayerId player)
        {
            Kind = kind;
            Player = player;
        }

        public static NetworkTarget ToPlayer(NetworkPlayerId player) => new(NetworkTargetKind.Player, player);

        public override string ToString() =>
            Kind == NetworkTargetKind.Player ? $"Target({Player})" : $"Target({Kind})";
    }
}
