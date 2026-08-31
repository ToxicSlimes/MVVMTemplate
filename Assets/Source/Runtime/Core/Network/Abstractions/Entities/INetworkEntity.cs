using Core.Network.Abstractions.Types;
using UnityEngine;

namespace Core.Network.Abstractions.Entities
{
    public interface INetworkEntity
    {
        NetworkEntityId Id { get; }
        NetworkPlayerId InputAuthority { get; }
        NetworkAuthority Authority { get; }
        bool IsValid { get; }
        GameObject GameObject { get; }
    }
}
