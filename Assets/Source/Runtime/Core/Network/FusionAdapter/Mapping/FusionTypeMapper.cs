using System;
using Core.Network.Abstractions.Types;
using Fusion;
using Fusion.Sockets;

namespace Core.Network.FusionAdapter.Mapping
{
    public static class FusionTypeMapper
    {
        public static NetworkPlayerId ToCore(this PlayerRef player) =>
            player.IsNone ? NetworkPlayerId.None : new NetworkPlayerId(player.RawEncoded);

        public static PlayerRef ToFusion(this NetworkPlayerId player) =>
            player.IsValid ? PlayerRef.FromEncoded(player.Raw) : PlayerRef.None;

        public static NetworkEntityId ToCore(this NetworkId id) => new(id.Raw);

        public static NetworkId ToFusion(this NetworkEntityId id) => NetworkId.FromRaw(id.Raw);

        public static NetworkMode ToCore(this GameMode mode) => mode switch
        {
            GameMode.Single => NetworkMode.Single,
            GameMode.Shared => NetworkMode.Shared,
            GameMode.Host => NetworkMode.Host,
            GameMode.Client => NetworkMode.Client,
            GameMode.Server => NetworkMode.Server,
            GameMode.AutoHostOrClient => NetworkMode.AutoHostOrClient,
            _ => NetworkMode.None
        };

        public static GameMode ToFusion(this NetworkMode mode) => mode switch
        {
            NetworkMode.Single => GameMode.Single,
            NetworkMode.Shared => GameMode.Shared,
            NetworkMode.Host => GameMode.Host,
            NetworkMode.Client => GameMode.Client,
            NetworkMode.Server => GameMode.Server,
            NetworkMode.AutoHostOrClient => GameMode.AutoHostOrClient,
            _ => throw new ArgumentOutOfRangeException(nameof(mode), mode, "NetworkMode.None не является валидным режимом запуска")
        };

        public static NetworkShutdownReason ToCore(this ShutdownReason reason) => reason switch
        {
            ShutdownReason.Ok => NetworkShutdownReason.Ok,
            ShutdownReason.Error => NetworkShutdownReason.Error,
            ShutdownReason.ConnectionTimeout => NetworkShutdownReason.ConnectionTimeout,
            ShutdownReason.ConnectionRefused => NetworkShutdownReason.ConnectionRefused,
            ShutdownReason.ServerInRoom => NetworkShutdownReason.ServerInRoom,
            ShutdownReason.GameIdAlreadyExists => NetworkShutdownReason.GameIdAlreadyExists,
            ShutdownReason.GameNotFound => NetworkShutdownReason.GameNotFound,
            ShutdownReason.GameIsFull => NetworkShutdownReason.GameIsFull,
            ShutdownReason.GameClosed => NetworkShutdownReason.GameClosed,
            ShutdownReason.DisconnectedByPluginLogic => NetworkShutdownReason.DisconnectedByPluginLogic,
            ShutdownReason.MaxCcuReached => NetworkShutdownReason.MaxCcuReached,
            ShutdownReason.InvalidRegion => NetworkShutdownReason.InvalidRegion,
            ShutdownReason.InvalidAuthentication => NetworkShutdownReason.InvalidAuthentication,
            ShutdownReason.CustomAuthenticationFailed => NetworkShutdownReason.CustomAuthenticationFailed,
            ShutdownReason.AuthenticationTicketExpired => NetworkShutdownReason.AuthenticationTicketExpired,
            ShutdownReason.PhotonCloudTimeout => NetworkShutdownReason.PhotonCloudTimeout,
            ShutdownReason.AlreadyRunning => NetworkShutdownReason.AlreadyRunning,
            ShutdownReason.InvalidArguments => NetworkShutdownReason.InvalidArguments,
            ShutdownReason.HostMigration => NetworkShutdownReason.HostMigration,
            ShutdownReason.IncompatibleConfiguration => NetworkShutdownReason.IncompatibleConfiguration,
            ShutdownReason.OperationTimeout => NetworkShutdownReason.OperationTimeout,
            ShutdownReason.OperationCanceled => NetworkShutdownReason.OperationCanceled,
            _ => NetworkShutdownReason.Unknown
        };

        public static NetworkDisconnectReason ToCore(this NetDisconnectReason reason) => reason switch
        {
            NetDisconnectReason.Requested => NetworkDisconnectReason.Requested,
            NetDisconnectReason.ByRemote => NetworkDisconnectReason.ByRemote,
            NetDisconnectReason.Timeout => NetworkDisconnectReason.Timeout,
            NetDisconnectReason.ProtocolError => NetworkDisconnectReason.ProtocolError,
            NetDisconnectReason.SendWindowFull => NetworkDisconnectReason.SendWindowFull,
            NetDisconnectReason.SequenceOutOfBounds => NetworkDisconnectReason.SequenceOutOfBounds,
            _ => NetworkDisconnectReason.Unknown
        };

        public static NetworkConnectFailedReason ToCore(this NetConnectFailedReason reason) => reason switch
        {
            NetConnectFailedReason.Timeout => NetworkConnectFailedReason.Timeout,
            NetConnectFailedReason.ServerRefused => NetworkConnectFailedReason.ServerRefused,
            NetConnectFailedReason.ServerFull => NetworkConnectFailedReason.ServerFull,
            _ => NetworkConnectFailedReason.Unknown
        };

        public static NetworkSessionInfo ToCore(this SessionInfo info)
        {
            if (info == null || !info.IsValid)
            {
                return NetworkSessionInfo.Empty;
            }

            return new NetworkSessionInfo(
                info.Name,
                info.Region,
                info.PlayerCount,
                info.MaxPlayers,
                info.IsOpen,
                info.IsVisible);
        }

        public static NetworkStartResult ToCore(this StartGameResult result) =>
            result.Ok
                ? NetworkStartResult.Success
                : NetworkStartResult.Failure(result.ErrorMessage, result.ShutdownReason.ToCore());

        public static NetworkAuthority ToAuthority(this NetworkObject networkObject)
        {
            if (networkObject == null || !networkObject.IsValid)
            {
                return NetworkAuthority.None;
            }

            if (networkObject.HasStateAuthority)
            {
                return NetworkAuthority.State;
            }

            return networkObject.HasInputAuthority ? NetworkAuthority.Input : NetworkAuthority.Proxy;
        }
    }
}
