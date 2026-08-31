using System;
using System.Collections.Generic;
using System.Text;
using Core.Network.Abstractions.Messaging;
using Core.Network.Abstractions.Services;
using Core.Network.Abstractions.Types;
using Core.Network.FusionAdapter.Callbacks;
using Core.Network.FusionAdapter.Mapping;
using Core.Network.FusionAdapter.Runner;
using Fusion;
using Fusion.Sockets;
using Newtonsoft.Json;
using ILogger = Core.Abstractions.Services.ILogger;

namespace Core.Network.FusionAdapter.Messaging
{
    /// <summary>
    /// Типизированный обмен сообщениями поверх надёжного канала Fusion.
    /// Не требует NetworkObject и кодогенерации RPC.
    /// </summary>
    public sealed class FusionMessenger : FusionCallbacksBase, INetworkMessenger, IDisposable
    {
        private const int ChannelMagic = 0x46534D47;

        private readonly INetworkRunnerAccessor _accessor;
        private readonly ILogger _logger;
        private readonly NetworkMessageRegistry _registry = new();
        private readonly Dictionary<int, List<Action<NetworkPlayerId, object>>> _handlers = new();

        public FusionMessenger(INetworkRunnerAccessor accessor, ILogger logger)
        {
            _accessor = accessor;
            _logger = logger;
            _accessor.RegisterCallbacks(this);
        }

        public void Send<TMessage>(TMessage message, NetworkTarget target) where TMessage : INetworkMessage
        {
            var runner = RequireRunner();
            var typeId = _registry.Register(typeof(TMessage));
            var payload = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(message));

            switch (target.Kind)
            {
                case NetworkTargetKind.StateAuthority:
                    SendToStateAuthority(runner, typeId, payload);
                    break;

                case NetworkTargetKind.Player:
                    SendToPlayer(runner, typeId, target.Player, payload);
                    break;

                default:
                    SendToAll(runner, typeId, payload);
                    break;
            }
        }

        public IDisposable Subscribe<TMessage>(Action<NetworkPlayerId, TMessage> handler) where TMessage : INetworkMessage
        {
            if (handler == null)
            {
                throw new ArgumentNullException(nameof(handler));
            }

            var typeId = _registry.Register(typeof(TMessage));

            if (!_handlers.TryGetValue(typeId, out var list))
            {
                list = new List<Action<NetworkPlayerId, object>>();
                _handlers[typeId] = list;
            }

            void Wrapper(NetworkPlayerId sender, object payload) => handler(sender, (TMessage)payload);

            list.Add(Wrapper);
            return new Subscription(list, Wrapper);
        }

        public void Dispose()
        {
            _handlers.Clear();
        }

        protected override void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ReliableKey key, ReadOnlySpan<byte> data)
        {
            key.GetInts(out var magic, out var typeId, out var routeCode, out var routeArg);

            if (magic != ChannelMagic)
            {
                return;
            }

            var payload = data.ToArray();

            switch ((NetworkMessageRoute)routeCode)
            {
                case NetworkMessageRoute.RelayToAll:
                    RelayToAll(runner, typeId, player, payload);
                    Dispatch(typeId, player.ToCore(), payload);
                    break;

                case NetworkMessageRoute.RelayToPlayer:
                    RelayToPlayer(runner, typeId, PlayerRef.FromEncoded(routeArg), payload, player);
                    break;

                default:
                    Dispatch(typeId, ResolveOrigin(player, routeArg).ToCore(), payload);
                    break;
            }
        }

        private void SendToAll(NetworkRunner runner, int typeId, byte[] payload)
        {
            if (CanReachPeersDirectly(runner))
            {
                BroadcastToPeers(runner, typeId, PlayerRef.None, payload, runner.LocalPlayer);
            }
            else
            {
                runner.SendReliableDataToServer(BuildKey(typeId, NetworkMessageRoute.RelayToAll), payload);
            }

            Dispatch(typeId, runner.LocalPlayer.ToCore(), payload);
        }

        private void SendToPlayer(NetworkRunner runner, int typeId, NetworkPlayerId targetPlayer, byte[] payload)
        {
            var target = targetPlayer.ToFusion();

            if (target == runner.LocalPlayer)
            {
                Dispatch(typeId, runner.LocalPlayer.ToCore(), payload);
                return;
            }

            if (CanReachPeersDirectly(runner))
            {
                runner.SendReliableDataToPlayer(target, BuildDirectKey(typeId, runner.LocalPlayer), payload);
                return;
            }

            runner.SendReliableDataToServer(
                BuildKey(typeId, NetworkMessageRoute.RelayToPlayer, target.RawEncoded),
                payload);
        }

        private void SendToStateAuthority(NetworkRunner runner, int typeId, byte[] payload)
        {
            if (runner.IsServer || runner.IsSharedModeMasterClient)
            {
                Dispatch(typeId, runner.LocalPlayer.ToCore(), payload);
                return;
            }

            if (runner.GameMode == GameMode.Shared)
            {
                runner.SendReliableDataToPlayer(
                    PlayerRef.InternalMasterClientIdentifier,
                    BuildDirectKey(typeId, runner.LocalPlayer),
                    payload);
                return;
            }

            runner.SendReliableDataToServer(BuildDirectKey(typeId, runner.LocalPlayer), payload);
        }

        private void RelayToAll(NetworkRunner runner, int typeId, PlayerRef origin, byte[] payload)
        {
            if (!CanReachPeersDirectly(runner))
            {
                _logger.Warn("[Network] Ретрансляция запрошена у пира без прав рассылки, сообщение отброшено");
                return;
            }

            BroadcastToPeers(runner, typeId, origin, payload, origin);
        }

        private void RelayToPlayer(NetworkRunner runner, int typeId, PlayerRef target, byte[] payload, PlayerRef origin)
        {
            if (target == runner.LocalPlayer)
            {
                Dispatch(typeId, origin.ToCore(), payload);
                return;
            }

            runner.SendReliableDataToPlayer(target, BuildDirectKey(typeId, origin), payload);
        }

        private void BroadcastToPeers(NetworkRunner runner, int typeId, PlayerRef excluded, byte[] payload, PlayerRef origin)
        {
            var key = BuildDirectKey(typeId, origin);

            foreach (var player in runner.ActivePlayers)
            {
                if (player == runner.LocalPlayer || player == excluded)
                {
                    continue;
                }

                runner.SendReliableDataToPlayer(player, key, payload);
            }
        }

        private void Dispatch(int typeId, NetworkPlayerId sender, byte[] payload)
        {
            if (!_handlers.TryGetValue(typeId, out var list) || list.Count == 0)
            {
                return;
            }

            if (!_registry.TryResolve(typeId, out var type))
            {
                _logger.Warn("[Network] Получено сообщение с неизвестным идентификатором {0}", typeId);
                return;
            }

            var message = JsonConvert.DeserializeObject(Encoding.UTF8.GetString(payload), type);
            var snapshot = list.ToArray();

            for (var i = 0; i < snapshot.Length; i++)
            {
                snapshot[i](sender, message);
            }
        }

        private static bool CanReachPeersDirectly(NetworkRunner runner) =>
            runner.GameMode == GameMode.Shared || runner.IsServer;

        private static ReliableKey BuildKey(int typeId, NetworkMessageRoute route, int routeArg = 0) =>
            ReliableKey.FromInts(ChannelMagic, typeId, (int)route, routeArg);

        /// <summary>
        /// В прямом сообщении routeArg несёт исходного отправителя: после ретрансляции
        /// транспортным отправителем становится сервер, и без этого подписчик увидел бы его.
        /// </summary>
        private static ReliableKey BuildDirectKey(int typeId, PlayerRef origin) =>
            BuildKey(typeId, NetworkMessageRoute.Direct, origin.RawEncoded);

        private static PlayerRef ResolveOrigin(PlayerRef transportSender, int routeArg) =>
            routeArg != 0 ? PlayerRef.FromEncoded(routeArg) : transportSender;

        private NetworkRunner RequireRunner()
        {
            if (!_accessor.IsRunning)
            {
                throw new InvalidOperationException("Отправка сетевого сообщения невозможна: сессия не запущена");
            }

            return _accessor.Runner;
        }

        private sealed class Subscription : IDisposable
        {
            private readonly List<Action<NetworkPlayerId, object>> _list;
            private readonly Action<NetworkPlayerId, object> _handler;

            public Subscription(List<Action<NetworkPlayerId, object>> list, Action<NetworkPlayerId, object> handler)
            {
                _list = list;
                _handler = handler;
            }

            public void Dispose() => _list.Remove(_handler);
        }
    }
}
