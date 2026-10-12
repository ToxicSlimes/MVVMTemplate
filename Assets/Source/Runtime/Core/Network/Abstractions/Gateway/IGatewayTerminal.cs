using System.Threading;
using Cysharp.Threading.Tasks;

namespace Core.Network.Abstractions.Gateway
{
    public interface IGatewayTerminal
    {
        UniTask<object> SendAsync(GatewayContext context, CancellationToken cancellationToken);
    }
}
