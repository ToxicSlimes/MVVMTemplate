using System;

namespace Core.Network.Abstractions.Types
{
    public readonly struct NetworkEntityId : IEquatable<NetworkEntityId>
    {
        public static readonly NetworkEntityId None = new(0u);

        public readonly uint Raw;

        public NetworkEntityId(uint raw)
        {
            Raw = raw;
        }

        public bool IsValid => Raw != 0u;

        public bool Equals(NetworkEntityId other) => Raw == other.Raw;

        public override bool Equals(object obj) => obj is NetworkEntityId other && Equals(other);

        public override int GetHashCode() => (int)Raw;

        public override string ToString() => IsValid ? $"Entity({Raw})" : "Entity(None)";

        public static bool operator ==(NetworkEntityId left, NetworkEntityId right) => left.Equals(right);

        public static bool operator !=(NetworkEntityId left, NetworkEntityId right) => !left.Equals(right);
    }
}
