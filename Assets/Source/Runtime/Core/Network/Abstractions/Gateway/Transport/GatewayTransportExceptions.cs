using System;

namespace Core.Network.Abstractions.Gateway.Transport
{
    public sealed class GatewayRemoteException : Exception
    {
        public string ErrorCode { get; }

        public GatewayRemoteException(string errorCode, string message)
            : base(string.IsNullOrWhiteSpace(message) ? "Remote gateway request failed" : message)
        {
            ErrorCode = string.IsNullOrWhiteSpace(errorCode) ? "remote_error" : errorCode;
        }
    }

    public sealed class GatewayProtocolException : Exception
    {
        public GatewayProtocolException(string message) : base(message)
        {
        }
    }
}
