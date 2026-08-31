using System;

namespace Core.Network.Abstractions.Types
{
    public readonly struct NetworkPlayerId : IEquatable<NetworkPlayerId>
    {
        public static readonly NetworkPlayerId None = new(0);

        public readonly int Raw;

        public NetworkPlayerId(int raw)
        {
            Raw = raw;
        }

        public bool IsValid => Raw != 0;

        public bool Equals(NetworkPlayerId other) => Raw == other.Raw;

        public override bool Equals(object obj) => obj is NetworkPlayerId other && Equals(other);

        public override int GetHashCode() => Raw;

        public override string ToString() => IsValid ? $"Player({Raw})" : "Player(None)";

        public static bool operator ==(NetworkPlayerId left, NetworkPlayerId right) => left.Equals(right);

        public static bool operator !=(NetworkPlayerId left, NetworkPlayerId right) => !left.Equals(right);
    }
}
