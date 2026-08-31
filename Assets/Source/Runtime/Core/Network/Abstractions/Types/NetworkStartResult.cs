namespace Core.Network.Abstractions.Types
{
    public readonly struct NetworkStartResult
    {
        public static readonly NetworkStartResult Success = new(true, null, NetworkShutdownReason.Ok);

        public readonly bool IsOk;
        public readonly string ErrorMessage;
        public readonly NetworkShutdownReason ShutdownReason;

        public NetworkStartResult(bool isOk, string errorMessage, NetworkShutdownReason shutdownReason)
        {
            IsOk = isOk;
            ErrorMessage = errorMessage;
            ShutdownReason = shutdownReason;
        }

        public static NetworkStartResult Failure(string errorMessage, NetworkShutdownReason reason) =>
            new(false, errorMessage, reason);

        public override string ToString() => IsOk ? "StartResult(Ok)" : $"StartResult({ShutdownReason}: {ErrorMessage})";
    }
}
