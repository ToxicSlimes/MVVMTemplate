using System.Threading;
using Cysharp.Threading.Tasks;

namespace Core.Network.Abstractions.Gateway
{
    public interface IGatewayPipe
    {
        /// <summary>
        /// Pipe implementations may be shared by multiple pipelines and concurrent sends.
        /// Per-request state belongs in <paramref name="context"/>, not in the pipe instance.
        /// </summary>
        UniTask<object> InvokeAsync(
            GatewayContext context,
            GatewayPipeDelegate next,
            CancellationToken cancellationToken);
    }
}
