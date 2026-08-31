using Fusion;
using Reflex.Attributes;

namespace Core.Network.FusionAdapter.Player
{
    public sealed class BotAvatarController : NetworkBehaviour, IAvatarController
    {
        [Inject] private PlayerAvatarRegistry _registry;

        [Networked] public int BotNumber { get; set; }

        private PlayerRef _owner;

        public PlayerRef Owner => BotIdentity.FromNumber(BotNumber);

        public override void Spawned()
        {
            _owner = Owner;
            _registry.Add(_owner, transform);
        }

        public override void Despawned(NetworkRunner runner, bool hasState)
        {
            _registry.Remove(_owner);
        }
    }
}
