using Core.Network.Abstractions.Entities;
using Core.Network.Abstractions.Types;
using Core.Network.FusionAdapter.Mapping;
using Fusion;
using UnityEngine;

namespace Core.Network.FusionAdapter.Spawning
{
    /// <summary>
    /// Обёртка сетевого объекта, не унаследованного от <see cref="NetworkEntityBase"/>.
    /// </summary>
    public sealed class NetworkEntityAdapter : INetworkEntity
    {
        private readonly NetworkObject _networkObject;

        public NetworkEntityAdapter(NetworkObject networkObject)
        {
            _networkObject = networkObject;
        }

        public NetworkEntityId Id => IsValid ? _networkObject.Id.ToCore() : NetworkEntityId.None;

        public NetworkPlayerId InputAuthority => IsValid ? _networkObject.InputAuthority.ToCore() : NetworkPlayerId.None;

        public NetworkAuthority Authority => _networkObject.ToAuthority();

        public bool IsValid => _networkObject != null && _networkObject.IsValid;

        public GameObject GameObject => IsValid ? _networkObject.gameObject : null;

        public static INetworkEntity Wrap(NetworkObject networkObject)
        {
            if (networkObject == null)
            {
                return null;
            }

            var entity = networkObject.GetComponent<NetworkEntityBase>();
            return entity != null ? entity : new NetworkEntityAdapter(networkObject);
        }

        public static NetworkObject Unwrap(INetworkEntity entity) => entity switch
        {
            NetworkEntityAdapter adapter => adapter._networkObject,
            NetworkEntityBase entityBase => entityBase.Object,
            _ => null
        };
    }
}
