using System;

namespace Core.Network.Abstractions.Types
{
    /// <summary>
    /// Идентификатор сетевого префаба. Имя префаба, зарегистрированного в таблице Fusion.
    /// </summary>
    public readonly struct NetworkPrefabKey : IEquatable<NetworkPrefabKey>
    {
        public readonly string Value;

        public NetworkPrefabKey(string value)
        {
            Value = value;
        }

        public bool IsValid => !string.IsNullOrEmpty(Value);

        public bool Equals(NetworkPrefabKey other) => string.Equals(Value, other.Value, StringComparison.Ordinal);

        public override bool Equals(object obj) => obj is NetworkPrefabKey other && Equals(other);

        public override int GetHashCode() => Value == null ? 0 : StringComparer.Ordinal.GetHashCode(Value);

        public override string ToString() => Value ?? string.Empty;

        public static bool operator ==(NetworkPrefabKey left, NetworkPrefabKey right) => left.Equals(right);

        public static bool operator !=(NetworkPrefabKey left, NetworkPrefabKey right) => !left.Equals(right);
    }
}
