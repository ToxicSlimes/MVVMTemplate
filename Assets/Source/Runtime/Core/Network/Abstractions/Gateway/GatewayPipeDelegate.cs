using System.Threading;
using Cysharp.Threading.Tasks;

namespace Core.Network.Abstractions.Gateway
{
    public delegate UniTask<object> GatewayPipeDelegate(
        GatewayContext context,
        CancellationToken cancellationToken);
}
