using System.Threading;
using Cysharp.Threading.Tasks;

namespace Core.Abstractions.Services
{
    public interface ILoadSceneService
    {
        UniTask LoadAsync(SceneLoadPayload payload, CancellationToken cancellationToken = default);
    }
}
