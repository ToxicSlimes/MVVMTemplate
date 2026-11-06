using System;
using Core.Network.FusionAdapter.Spawning;
using Fusion;
using Reflex.Core;
using Reflex.Injectors;
using UnityEngine;

namespace UnityEngine
{
    public enum FindObjectsInactive { Include }
    public class Object
    {
        public bool Destroyed;
        public static NetworkObject[] Scene = Array.Empty<NetworkObject>();
        public static T[] FindObjectsByType<T>(FindObjectsInactive _) => (T[])(object)Scene;
        public static bool operator ==(Object a, Object b) => (ReferenceEquals(a, null) || a.Destroyed) && (ReferenceEquals(b, null) || b.Destroyed) || !ReferenceEquals(a, null) && !ReferenceEquals(b, null) && !a.Destroyed && !b.Destroyed && ReferenceEquals(a, b);
        public static bool operator !=(Object a, Object b) => !(a == b);
        public override bool Equals(object other) => this == other as Object;
        public override int GetHashCode() => base.GetHashCode();
    }
    public class GameObject : Object
    {
        private readonly int _id;
        public GameObject(int id) { _id = id; }
        public int GetEntityId() => _id;
    }
}
namespace Fusion
{
    public class NetworkObject : UnityEngine.Object
    {
        public GameObject gameObject;
        public string name = "probe";
        public NetworkObject(int id) { gameObject = new GameObject(id); }
    }
    public class NetworkRunner { }
    public class NetworkObjectProviderDefault
    {
        protected virtual NetworkObject InstantiatePrefab(NetworkRunner runner, NetworkObject prefab) => prefab;
    }
}
namespace Reflex.Core { public class Container { } }
namespace Reflex.Injectors
{
    public static class GameObjectInjector
    {
        public static Action<GameObject, Container> OnInject;
        public static void InjectRecursive(GameObject obj, Container container) => OnInject(obj, container);
    }
}
class Program
{
    static int failures;
    static void Check(bool condition, string message) { if (!condition) throw new Exception(message); }
    static void Run(string name, Action test)
    {
        try { test(); Console.WriteLine("PASS " + name); }
        catch (Exception e) { failures++; Console.WriteLine("FAIL " + name + ": " + e.Message); }
    }
    static ReflexNetworkObjectProvider Setup(NetworkObject obj)
    {
        UnityEngine.Object.Scene = new[] { obj };
        var provider = new ReflexNetworkObjectProvider();
        provider.SetContainer(new Container());
        return provider;
    }
    static int Main()
    {
        Run("success deduplicates", () => {
            var provider = Setup(new NetworkObject(1)); int calls = 0;
            GameObjectInjector.OnInject = (_, _) => calls++;
            provider.InjectSceneObjects(); provider.InjectSceneObjects();
            Check(calls == 1, "expected one injection, got " + calls);
        });
        Run("partial injection failure is permanent", () => {
            var provider = Setup(new NetworkObject(2)); int firstComponent = 0, secondComponent = 0;
            var expected = new InvalidOperationException("injection failed");
            GameObjectInjector.OnInject = (_, _) => {
                firstComponent++; // First component's [Inject] has already succeeded.
                secondComponent++;
                throw expected; // A later component fails.
            };
            try { provider.InjectSceneObjects(); throw new Exception("exception was swallowed"); }
            catch (InvalidOperationException ex) { Check(ReferenceEquals(ex, expected), "first exception changed"); }
            for (int i = 0; i < 2; i++) {
                try { provider.InjectSceneObjects(); throw new Exception("previous failure was ignored"); }
                catch (InvalidOperationException ex) {
                    Check(!ReferenceEquals(ex, expected) && ReferenceEquals(ex.InnerException, expected), "previous failure must retain original cause");
                }
            }
            Check(firstComponent == 1 && secondComponent == 1, "partial injection was repeated");
        });
        Run("reentrant call deduplicates", () => {
            var provider = Setup(new NetworkObject(3)); int calls = 0;
            GameObjectInjector.OnInject = (_, _) => { calls++; if (calls == 1) provider.InjectSceneObjects(); };
            provider.InjectSceneObjects(); provider.InjectSceneObjects();
            Check(calls == 1, "expected one injection despite reentrancy, got " + calls);
        });
        Run("destroyed Unity object is skipped", () => {
            var obj = new NetworkObject(4) { Destroyed = true }; var provider = Setup(obj); int calls = 0;
            GameObjectInjector.OnInject = (_, _) => calls++;
            provider.InjectSceneObjects(); Check(calls == 0, "destroyed object was injected");
        });
        return failures == 0 ? 0 : 1;
    }
}
