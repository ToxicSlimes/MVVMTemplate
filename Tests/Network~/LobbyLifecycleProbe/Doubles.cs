using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace Cysharp.Threading.Tasks {
    [AsyncMethodBuilder(typeof(AsyncUniTaskMethodBuilder))] public readonly struct UniTask {
        readonly Task task; public UniTask(Task task) => this.task = task;
        public TaskAwaiter GetAwaiter() => task.GetAwaiter(); public void Forget() { }
        public UniTask AttachExternalCancellation(CancellationToken token) => new(task.WaitAsync(token));
    }
    [AsyncMethodBuilder(typeof(AsyncUniTaskMethodBuilder<>))] public readonly struct UniTask<T> {
        readonly Task<T> task; public UniTask(Task<T> task) => this.task = task;
        public TaskAwaiter<T> GetAwaiter() => task.GetAwaiter(); public Task<T> AsTask() => task;
    }
    public struct AsyncUniTaskMethodBuilder {
        AsyncTaskMethodBuilder builder; public static AsyncUniTaskMethodBuilder Create() => new() { builder = AsyncTaskMethodBuilder.Create() };
        public UniTask Task => new(builder.Task); public void SetResult() => builder.SetResult(); public void SetException(Exception e) => builder.SetException(e);
        public void SetStateMachine(IAsyncStateMachine s) => builder.SetStateMachine(s); public void Start<T>(ref T s) where T:IAsyncStateMachine => builder.Start(ref s);
        public void AwaitOnCompleted<T,U>(ref T a, ref U s) where T:INotifyCompletion where U:IAsyncStateMachine => builder.AwaitOnCompleted(ref a, ref s);
        public void AwaitUnsafeOnCompleted<T,U>(ref T a, ref U s) where T:ICriticalNotifyCompletion where U:IAsyncStateMachine => builder.AwaitUnsafeOnCompleted(ref a, ref s);
    }
    public struct AsyncUniTaskMethodBuilder<T> {
        AsyncTaskMethodBuilder<T> builder; public static AsyncUniTaskMethodBuilder<T> Create() => new() { builder = AsyncTaskMethodBuilder<T>.Create() };
        public UniTask<T> Task => new(builder.Task); public void SetResult(T value) => builder.SetResult(value); public void SetException(Exception e) => builder.SetException(e);
        public void SetStateMachine(IAsyncStateMachine s) => builder.SetStateMachine(s); public void Start<U>(ref U s) where U:IAsyncStateMachine => builder.Start(ref s);
        public void AwaitOnCompleted<U,V>(ref U a, ref V s) where U:INotifyCompletion where V:IAsyncStateMachine => builder.AwaitOnCompleted(ref a, ref s);
        public void AwaitUnsafeOnCompleted<U,V>(ref U a, ref V s) where U:ICriticalNotifyCompletion where V:IAsyncStateMachine => builder.AwaitUnsafeOnCompleted(ref a, ref s);
    }
    public static class Extensions { public static UniTask AsUniTask(this Task task) => new(task); }
}
namespace UnityEngine {
    public class Object { public static void DontDestroyOnLoad(object o) {} public static void Destroy(object o) { ((GameObject)o).Destroyed = true; } }
    public class GameObject : Object { public static readonly List<GameObject> All = new(); public bool Destroyed; public GameObject(string name) { All.Add(this); } public T AddComponent<T>() where T:new() => new(); }
}
namespace Fusion {
    public enum SessionLobby { ClientServer } public enum ShutdownReason { None }
    public class LobbyInfo { public bool IsValid; public string Name = "test", Region = "test"; }
    public class SessionInfo { }
    public class StartGameResult { public bool Ok; }
    public class NetworkRunner {
        public static readonly List<NetworkRunner> All = new(); public static Func<NetworkRunner,CancellationToken,Task<StartGameResult>> Join;
        public NetworkRunner() { All.Add(this); } public bool IsShutdown; public bool ProvideInput; public LobbyInfo LobbyInfo = new(); public int Calls;
        public void AddCallbacks(object callback) { } public Task Shutdown() => Task.CompletedTask;
        public Task<StartGameResult> JoinSessionLobby(SessionLobby lobby, string name, CancellationToken cancellationToken = default) { Calls++; return Join(this, cancellationToken); }
    }
}
namespace Core.Network.Abstractions.Types { public class NetworkSessionInfo { } public struct NetworkStartResult { public bool IsOk; public string ShutdownReason, ErrorMessage; public static NetworkStartResult Success => new() { IsOk = true }; } }
namespace Core.Network.Abstractions.Services { public interface INetworkLobby { } }
namespace Core.Abstractions.Services { public interface ILogger { void Error(string format, params object[] args); void Info(string format, params object[] args); } }
namespace Core.Mediator.Abstractions { public interface IMediator { Cysharp.Threading.Tasks.UniTask Publish(object notification); } }
namespace Core.Application.Notifications.Network { public class NetworkSessionListUpdatedNotification { public NetworkSessionListUpdatedNotification(object sessions) { } } }
namespace Core.Network.FusionAdapter.Callbacks { public class FusionCallbacksBase { protected virtual void OnSessionListUpdated(Fusion.NetworkRunner runner, List<Fusion.SessionInfo> sessions) { } protected virtual void OnShutdown(Fusion.NetworkRunner runner, Fusion.ShutdownReason reason) { } } }
namespace Core.Network.FusionAdapter.Mapping { public static class Mapping { public static Core.Network.Abstractions.Types.NetworkStartResult ToCore(this Fusion.StartGameResult result) => new() { IsOk = result.Ok }; public static Core.Network.Abstractions.Types.NetworkSessionInfo ToCore(this Fusion.SessionInfo info) => new(); } }
