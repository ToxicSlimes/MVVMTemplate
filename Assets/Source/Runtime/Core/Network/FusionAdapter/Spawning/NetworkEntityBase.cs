using Core.Network.Abstractions.Entities;
using Core.Network.Abstractions.Types;
using Core.Network.FusionAdapter.Mapping;
using Fusion;
using UnityEngine;

namespace Core.Network.FusionAdapter.Spawning
{
    /// <summary>
    /// База сетевого объекта. Запечатывает точки входа Fusion и открывает вместо них
    /// хуки жизненного цикла, не требующие знания сигнатур Fusion.
    /// Id и IsValid реализованы явно, чтобы не перекрывать одноимённые члены NetworkBehaviour.
    /// </summary>
    public abstract class NetworkEntityBase : NetworkBehaviour, INetworkEntity
    {
        NetworkEntityId INetworkEntity.Id => IsSpawned ? Object.Id.ToCore() : NetworkEntityId.None;

        bool INetworkEntity.IsValid => IsSpawned;

        public NetworkPlayerId InputAuthority => IsSpawned ? Object.InputAuthority.ToCore() : NetworkPlayerId.None;

        public NetworkAuthority Authority => Object.ToAuthority();

        public GameObject GameObject => gameObject;

        protected bool IsSpawned => Object != null && Object.IsValid;

        public sealed override void Spawned() => OnNetworkSpawn();

        public sealed override void Despawned(NetworkRunner runner, bool hasState) => OnNetworkDespawn(hasState);

        public sealed override void FixedUpdateNetwork() => OnNetworkTick();

        public sealed override void Render() => OnNetworkRender();

        protected virtual void OnNetworkSpawn()
        {
        }

        protected virtual void OnNetworkDespawn(bool hasState)
        {
        }

        protected virtual void OnNetworkTick()
        {
        }

        protected virtual void OnNetworkRender()
        {
        }

        protected bool TryGetNetworkInput<TInput>(out TInput input) where TInput : unmanaged, INetworkInput =>
            GetInput(out input);
    }
}
