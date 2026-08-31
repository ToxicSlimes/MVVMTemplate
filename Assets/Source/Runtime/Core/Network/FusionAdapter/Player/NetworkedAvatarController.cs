using Fusion;

namespace Core.Network.FusionAdapter.Player
{
    public sealed class NetworkedAvatarController : NetworkBehaviour, IAvatarController
    {
        public PlayerRef Owner => Object.InputAuthority;
    }
}
