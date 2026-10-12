using System.Threading;
using Cysharp.Threading.Tasks;

namespace Core.Network.Abstractions.Gateway
{
    public interface IGateway
    {
        UniTask<TResponse> SendAsync<TResponse>(
            IGatewayRequest<TResponse> request,
            CancellationToken cancellationToken = default);

        UniTask<object> SendAsync(
            GatewayContext context,
            CancellationToken cancellationToken = default);
    }
}
