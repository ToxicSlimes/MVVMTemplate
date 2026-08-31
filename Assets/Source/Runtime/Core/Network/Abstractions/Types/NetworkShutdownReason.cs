namespace Core.Network.Abstractions.Types
{
    public enum NetworkShutdownReason
    {
        Unknown = 0,
        Ok,
        Error,
        ConnectionTimeout,
        ConnectionRefused,
        ServerInRoom,
        GameIdAlreadyExists,
        GameNotFound,
        GameIsFull,
        GameClosed,
        DisconnectedByPluginLogic,
        MaxCcuReached,
        InvalidRegion,
        InvalidAuthentication,
        CustomAuthenticationFailed,
        AuthenticationTicketExpired,
        PhotonCloudTimeout,
        AlreadyRunning,
        InvalidArguments,
        HostMigration,
        IncompatibleConfiguration,
        OperationTimeout,
        OperationCanceled
    }
}
