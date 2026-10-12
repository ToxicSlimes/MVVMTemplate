using System;
using Core.Network.Abstractions.Messaging;

namespace Core.Network.Abstractions.Gateway.Transport
{
    public static class GatewayProtocol
    {
        public const int CurrentVersion = 1;
        public const int MaxRouteLength = 256;
        public const int MaxRequestIdLength = 64;
        public const int MaxPayloadLength = 262144;
    }

    [Serializable]
    public sealed class GatewayRequestMessage : INetworkMessage
    {
        public int Version { get; set; }
        public string RequestId { get; set; }
        public string Route { get; set; }
        public string Payload { get; set; }

        public GatewayRequestMessage()
        {
        }

        public GatewayRequestMessage(string requestId, string route, string payload)
        {
            Version = GatewayProtocol.CurrentVersion;
            RequestId = requestId;
            Route = route;
            Payload = payload;
        }
    }

    [Serializable]
    public sealed class GatewayResponseMessage : INetworkMessage
    {
        public int Version { get; set; }
        public string RequestId { get; set; }
        public bool IsSuccess { get; set; }
        public string Payload { get; set; }
        public string ErrorCode { get; set; }
        public string Error { get; set; }

        public GatewayResponseMessage()
        {
        }

        public static GatewayResponseMessage Success(string requestId, string payload) => new()
        {
            Version = GatewayProtocol.CurrentVersion,
            RequestId = requestId,
            IsSuccess = true,
            Payload = payload
        };

        public static GatewayResponseMessage Failure(
            string requestId,
            string errorCode,
            string error) => new()
        {
            Version = GatewayProtocol.CurrentVersion,
            RequestId = requestId,
            IsSuccess = false,
            ErrorCode = errorCode,
            Error = error
        };
    }
}
