using System;
using System.Collections.Generic;

namespace Core.Network.FusionAdapter.Messaging
{
    /// <summary>
    /// Сопоставляет типы сетевых сообщений со стабильными идентификаторами.
    /// Хеш детерминирован между платформами, в отличие от <see cref="string.GetHashCode"/>.
    /// </summary>
    public sealed class NetworkMessageRegistry
    {
        private readonly Dictionary<int, Type> _byId = new();
        private readonly Dictionary<Type, int> _byType = new();

        public int Register(Type type)
        {
            if (_byType.TryGetValue(type, out var existingId))
            {
                return existingId;
            }

            var id = ComputeId(type);

            if (_byId.TryGetValue(id, out var clash) && clash != type)
            {
                throw new InvalidOperationException(
                    $"Коллизия идентификаторов сетевых сообщений: '{type.FullName}' и '{clash.FullName}'");
            }

            _byId[id] = type;
            _byType[type] = id;
            return id;
        }

        public bool TryResolve(int id, out Type type) => _byId.TryGetValue(id, out type);

        private static int ComputeId(Type type)
        {
            unchecked
            {
                const uint offsetBasis = 2166136261;
                const uint prime = 16777619;

                var name = type.FullName ?? type.Name;
                var hash = offsetBasis;

                for (var i = 0; i < name.Length; i++)
                {
                    hash ^= name[i];
                    hash *= prime;
                }

                return (int)hash;
            }
        }
    }
}
