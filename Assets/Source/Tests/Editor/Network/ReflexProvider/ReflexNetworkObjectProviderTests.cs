using System;
using System.Reflection;
using Core.Network.FusionAdapter.Spawning;
using Fusion;
using NUnit.Framework;
using Reflex.Attributes;
using Reflex.Core;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Core.Tests.Network
{
    public sealed class ReflexNetworkObjectProviderTests
    {
        private GameObject _providerObject;
        private GameObject _networkObject;
        private Container _container;
        private ReflexNetworkObjectProvider _provider;
        private InjectionCounter _counter;

        [SetUp]
        public void SetUp()
        {
            Assert.That(Object.FindObjectsByType<NetworkObject>(FindObjectsInactive.Include, FindObjectsSortMode.None),
                Is.Empty, "The provider scans all loaded scenes; refusing to inject unrelated NetworkObjects.");
            _counter = new InjectionCounter();
            _container = new ContainerBuilder().AddSingleton(_counter).Build();
            _providerObject = new GameObject("test provider");
            _provider = _providerObject.AddComponent<ReflexNetworkObjectProvider>();
            _provider.SetContainer(_container);
            _networkObject = new GameObject("test network object");
            _networkObject.SetActive(false);
            _networkObject.AddComponent<NetworkObject>();
        }

        [TearDown]
        public void TearDown()
        {
            if (_networkObject != null) Object.DestroyImmediate(_networkObject);
            if (_providerObject != null) Object.DestroyImmediate(_providerObject);
            _container?.Dispose();
        }

        [Test]
        public void FailedPartialInjection_IsNotRetried_AndOriginalFailureIsPreserved()
        {
            _networkObject.AddComponent<CountingInjectionComponent>();
            _networkObject.AddComponent<ThrowingInjectionComponent>();

            var first = Assert.Catch(() => _provider.InjectSceneObjects());
            Assert.That(first, Is.Not.Null);
            Assert.That(_counter.Successes, Is.EqualTo(1));
            Assert.That(_counter.Failures, Is.EqualTo(1));
            Assert.That(ContainsException(first, _counter.Failure), Is.True);

            var second = Assert.Throws<InvalidOperationException>(() => _provider.InjectSceneObjects());
            Assert.That(second.InnerException, Is.SameAs(first));
            Assert.That(_counter.Successes, Is.EqualTo(1));
            Assert.That(_counter.Failures, Is.EqualTo(1));
        }

        [Test]
        public void SuccessfulInjection_IsDeduplicatedAcrossScenePasses()
        {
            _networkObject.AddComponent<CountingInjectionComponent>();

            _provider.InjectSceneObjects();
            _provider.InjectSceneObjects();

            Assert.That(_counter.Successes, Is.EqualTo(1));
        }

        [Test]
        public void DestroyedNetworkObject_IsIgnoredByInjection()
        {
            var instance = _networkObject.GetComponent<NetworkObject>();
            Object.DestroyImmediate(_networkObject);
            Assert.That(instance == null, Is.True, "Unity destroyed-object null semantics");
            var inject = typeof(ReflexNetworkObjectProvider).GetMethod(
                "Inject", BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(inject, Is.Not.Null);

            Assert.DoesNotThrow(() => inject.Invoke(_provider, new object[] { instance }));
        }

        private static bool ContainsException(Exception exception, Exception expected)
        {
            for (var current = exception; current != null; current = current.InnerException)
                if (ReferenceEquals(current, expected)) return true;
            return false;
        }
    }

    public sealed class InjectionCounter
    {
        public int Successes;
        public int Failures;
        public readonly Exception Failure = new InvalidOperationException("injection failure marker");
    }

    public sealed class CountingInjectionComponent : MonoBehaviour
    {
        [Inject]
        private void Inject(InjectionCounter counter) => counter.Successes++;
    }

    public sealed class ThrowingInjectionComponent : MonoBehaviour
    {
        [Inject]
        private void Inject(InjectionCounter counter)
        {
            counter.Failures++;
            throw counter.Failure;
        }
    }
}
