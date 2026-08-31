using Fusion;
using Reflex.Attributes;

namespace Core.Network.FusionAdapter.Player
{
    public sealed class NetworkedAvatarController : NetworkBehaviour, IAvatarController
    {
        [Inject] private PlayerAvatarRegistry _registry;

        private PlayerRef _owner;

        public PlayerRef Owner => Object != null && Object.IsValid ? Object.InputAuthority : _owner;

        public override void Spawned()
        {
            _owner = Object.InputAuthority;
            _registry.Add(_owner, transform);
        }

        public override void Despawned(NetworkRunner runner, bool hasState)
        {
            _registry.Remove(_owner);
        }
    }
}
