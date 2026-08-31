using Fusion;

namespace Core.Network.FusionAdapter.Player
{
    public sealed class BotAvatarController : NetworkBehaviour, IAvatarController
    {
        [Networked] public int BotNumber { get; set; }

        public PlayerRef Owner => BotIdentity.FromNumber(BotNumber);
    }
}
