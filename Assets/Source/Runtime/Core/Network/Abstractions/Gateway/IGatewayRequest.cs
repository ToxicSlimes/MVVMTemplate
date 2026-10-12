namespace Core.Network.Abstractions.Gateway
{
    public interface IGatewayRequest
    {
        string Route { get; }
    }

    public interface IGatewayRequest<TResponse> : IGatewayRequest
    {
    }
}
