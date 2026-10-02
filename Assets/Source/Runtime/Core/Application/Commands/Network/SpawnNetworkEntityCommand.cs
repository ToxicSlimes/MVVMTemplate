using Core.Mediator.Abstractions;
using Core.Network.Abstractions.Entities;
using Core.Network.Abstractions.Types;
using UnityEngine;

namespace Core.Application.Commands.Network
{
    public readonly struct SpawnNetworkEntityCommand : IRequest<INetworkEntity>
    {
        public readonly NetworkPrefabKey Prefab;
        public readonly Vector3? Position;
        public readonly Quaternion? Rotation;
        public readonly NetworkPlayerId? InputAuthority;

        public SpawnNetworkEntityCommand(
            NetworkPrefabKey prefab,
            Vector3? position = null,
            Quaternion? rotation = null,
            NetworkPlayerId? inputAuthority = null)
        {
            Prefab = prefab;
            Position = position;
            Rotation = rotation;
            InputAuthority = inputAuthority;
        }
    }
}
