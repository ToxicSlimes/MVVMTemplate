using System;
using System.Threading;
using Cysharp.Threading.Tasks;

namespace Core.Network.Abstractions.Gateway
{
    public interface IGatewayRoute
    {
        string Route { get; }
        Type RequestType { get; }
        Type ResponseType { get; }

        UniTask<object> HandleAsync(GatewayContext context, CancellationToken cancellationToken);
    }
}
