using Core.Application.Handlers.Lobby;
using Core.Application.Notifications.Network;
using Core.Network.Abstractions.Services;
using Core.Network.Abstractions.Types;
using Cysharp.Threading.Tasks;

var controller = new Controller { IsStateAuthority = true, SessionGeneration = 2 };
var spawner = new Spawner();
var subject = new PlayerSpawnHandler(controller, spawner);
await subject.Handle(new NetworkPlayerJoinedNotification(new NetworkPlayerId(1), false, 1), default).AsTask();
if (spawner.Calls != 0) throw new Exception("Stale notification spawned a player in a different generation");
Console.WriteLine("PASS stale notification ignored");
await subject.Handle(new NetworkPlayerJoinedNotification(new NetworkPlayerId(1), false, 2), default).AsTask();
if (spawner.Calls != 1) throw new Exception("current notification ignored");
await subject.Handle(new NetworkPlayerJoinedNotification(new NetworkPlayerId(1), false), default).AsTask();
if (spawner.Calls != 1) throw new Exception("legacy unversioned notification accepted");
Console.WriteLine("PASS current accepted, legacy rejected");
var avatar = new PlayerAvatarSpawner(controller, spawner, new Points());
spawner.Calls = 0;
var pending = spawner.Pending = new TaskCompletionSource();
var old = avatar.SpawnAsync(new NetworkPlayerId(1), default).AsTask();
controller.SessionGeneration++;
spawner.Pending = null;
await avatar.SpawnAsync(new NetworkPlayerId(1), default).AsTask();
if (spawner.Calls != 2) throw new Exception("previous generation suppressed new avatar spawn");
pending.SetException(new InvalidOperationException());
try { await old; throw new Exception("old spawn did not fail"); } catch (InvalidOperationException) { }
await avatar.SpawnAsync(new NetworkPlayerId(1), default).AsTask();
if (spawner.Calls != 2) throw new Exception("old failure removed new registration");
Console.WriteLine("PASS new generation can spawn same player");

sealed class Controller : INetworkRunnerController
{
    public bool IsStateAuthority { get; set; }
    public long SessionGeneration { get; set; }
    public IReadOnlyList<NetworkPlayerId> ActivePlayers => new[] { new NetworkPlayerId(1) };
}
sealed class Spawner : INetworkSpawner
{
    public int Calls;
    public TaskCompletionSource Pending;
    public UniTask SpawnAsync(string prefab, UnityEngine.Vector3? position = null, UnityEngine.Quaternion? rotation = null, NetworkPlayerId? inputAuthority = null, CancellationToken cancellationToken = default)
    {
        Calls++;
        return new UniTask(Pending?.Task ?? Task.CompletedTask);
    }
}
sealed class Points : INetworkSpawnPoints { public int Count => 0; public bool TryGet(int i, out UnityEngine.Vector3 p, out UnityEngine.Quaternion q) { p=default; q=default; return false; } }
