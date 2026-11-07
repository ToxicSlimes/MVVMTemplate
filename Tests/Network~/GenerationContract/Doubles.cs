using System.Runtime.CompilerServices;

namespace Cysharp.Threading.Tasks
{
    [AsyncMethodBuilder(typeof(UniTaskBuilder))]
    public readonly struct UniTask
    {
        private readonly Task _task;
        public UniTask(Task task) => _task = task;
        public TaskAwaiter GetAwaiter() => _task.GetAwaiter();
        public Task AsTask() => _task;
    }
    public struct UniTaskBuilder
    {
        private AsyncTaskMethodBuilder _builder;
        public static UniTaskBuilder Create() => new() { _builder = AsyncTaskMethodBuilder.Create() };
        public UniTask Task => new(_builder.Task);
        public void SetResult() => _builder.SetResult();
        public void SetException(Exception exception) => _builder.SetException(exception);
        public void SetStateMachine(IAsyncStateMachine stateMachine) => _builder.SetStateMachine(stateMachine);
        public void Start<T>(ref T stateMachine) where T : IAsyncStateMachine => _builder.Start(ref stateMachine);
        public void AwaitOnCompleted<TAwaiter,TStateMachine>(ref TAwaiter awaiter, ref TStateMachine stateMachine) where TAwaiter : INotifyCompletion where TStateMachine : IAsyncStateMachine => _builder.AwaitOnCompleted(ref awaiter, ref stateMachine);
        public void AwaitUnsafeOnCompleted<TAwaiter,TStateMachine>(ref TAwaiter awaiter, ref TStateMachine stateMachine) where TAwaiter : ICriticalNotifyCompletion where TStateMachine : IAsyncStateMachine => _builder.AwaitUnsafeOnCompleted(ref awaiter, ref stateMachine);
    }
}
namespace Core.Network.Abstractions.Types
{
    public readonly record struct NetworkPlayerId(int Raw) { public bool IsValid => Raw > 0; }
    public static class NetworkPrefabKeys { public static readonly string NetworkPlayer = "NetworkPlayer", PlayerAvatar = "PlayerAvatar"; }
}
namespace Core.Mediator.Abstractions { public interface INotification {} }
namespace Core.Mediator.Handlers
{
    using Cysharp.Threading.Tasks;
    public interface INotificationHandler<T> { UniTask Handle(T notification, CancellationToken ct); }
}
namespace Core.Network.Abstractions.Services
{
    using Core.Network.Abstractions.Types;
    using Cysharp.Threading.Tasks;
    public interface INetworkRunnerController { bool IsStateAuthority { get; } long SessionGeneration { get; } IReadOnlyList<NetworkPlayerId> ActivePlayers { get; } }
    public interface INetworkSpawner { UniTask SpawnAsync(string prefab, UnityEngine.Vector3? position = null, UnityEngine.Quaternion? rotation = null, NetworkPlayerId? inputAuthority = null, CancellationToken cancellationToken = default); }
    public interface INetworkSpawnPoints { int Count {get;} bool TryGet(int index, out UnityEngine.Vector3 position, out UnityEngine.Quaternion rotation); }
}
namespace UnityEngine { public struct Vector3 { public static Vector3 zero => default; } public struct Quaternion { public static Quaternion identity => default; } public static class Mathf { public static int Abs(int x) => Math.Abs(x); } }
