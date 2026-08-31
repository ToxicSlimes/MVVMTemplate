using Core.Abstractions.MVVM.Models.Lobby;
using Core.Infrastructure.Mediator.Abstractions;
using Core.Network.FusionAdapter.Player;
using Fusion;
using Reflex.Attributes;

namespace Core.Network.FusionAdapter.Session
{
    public sealed class NetworkPlayer : NetworkBehaviour
    {
        [Networked] public NetworkString<_16> Nickname { get; set; }
        [Networked] public bool IsReady { get; set; }
        [Networked] public bool IsHost { get; set; }
        [Networked] public int ColorIndex { get; set; }
        [Networked] public int BotNumber { get; set; }

        public PlayerRef Owner => BotNumber == BotIdentity.NoBot
            ? Object.InputAuthority
            : BotIdentity.FromNumber(BotNumber);

        public bool IsBot => BotNumber != BotIdentity.NoBot;

        [Inject] private NetworkPlayerRegistry _registry;
        [Inject] private ILobbyModel _lobby;

        private ChangeDetector _changes;

        public override void Spawned()
        {
            Runner.MakeDontDestroyOnLoad(gameObject);

            _changes = GetChangeDetector(ChangeDetector.Source.SnapshotFrom, false);

            if (HasStateAuthority)
            {
                ColorIndex = _registry.Count;
                IsHost = !IsBot && Object.InputAuthority == Runner.LocalPlayer;
            }

            _registry.Add(this);

            if (HasInputAuthority)
            {
                RpcSetNickname(_lobby.Nickname);
            }
        }

        public override void Despawned(NetworkRunner runner, bool hasState)
        {
            _registry.Remove(this);
        }

        public override void Render()
        {
            foreach (var _ in _changes.DetectChanges(this, true))
            {
                _registry.NotifyChanged();
                return;
            }
        }

        [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
        public void RpcSetNickname(NetworkString<_16> nickname)
        {
            Nickname = nickname;
        }

        [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
        public void RpcSetReady(bool isReady)
        {
            IsReady = isReady;
        }
    }
}
