namespace Core.Network.Abstractions.Gateway.Transport
{
    public interface IGatewayAuthority
    {
        bool CanHandleRequests { get; }
    }
}
