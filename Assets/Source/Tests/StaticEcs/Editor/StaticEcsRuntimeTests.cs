using System;
using FFS.Libraries.StaticEcs;
using NUnit.Framework;
using Core.Infrastructure.StaticEcs;

namespace Core.Tests.StaticEcs
{
    public sealed class StaticEcsRuntimeTests
    {
        private struct TestWorld : IWorldType { }
        private struct TestSystems : ISystemsType { }
        private sealed class Counter : ISystem
        {
            public int InitCount, UpdateCount, DestroyCount;
            public void Init() => InitCount++;
            public void Update() => UpdateCount++;
            public void Destroy() => DestroyCount++;
        }

        private sealed class ExplicitSystem : ISystem
        {
            void ISystem.Update() { }
        }

        [Test]
        public void Add_ExplicitImplementation_IsRejectedRatherThanSilentlyIgnored()
        {
            using (var runtime = new StaticEcsRuntime<TestWorld, TestSystems>())
            {
                var error = Assert.Throws<NotSupportedException>(() => runtime.Add(new ExplicitSystem()));
                Assert.That(error.Message, Does.Contain("explicit"));
                var counter = new Counter();
                runtime.Add(counter);
                runtime.Initialize();
                runtime.Update();
                Assert.That(counter.UpdateCount, Is.EqualTo(1));
            }
            using (var retry = new StaticEcsRuntime<TestWorld, TestSystems>()) { }
        }

        private struct OtherSystems : ISystemsType { }
        private struct OtherWorld : IWorldType { }

        [Test]
        public void SecondWorld_CannotReuseLiveSystemsMarker()
        {
            using (var first = new StaticEcsRuntime<TestWorld, TestSystems>())
                Assert.Throws<InvalidOperationException>(() => new StaticEcsRuntime<OtherWorld, TestSystems>());
            using (var second = new StaticEcsRuntime<OtherWorld, TestSystems>()) { }
        }
        private struct ThrowWorld : IWorldType { }
        private struct ThrowSystems : ISystemsType { }
        private sealed class ThrowOnDestroy : ISystem
        {
            private bool _failed;
            public void Destroy()
            {
                if (_failed) return;
                _failed = true;
                throw new InvalidOperationException("destroy failed");
            }
        }

        [Test]
        public void FailedDestroy_DoesNotPermitReusingCorruptStaticPipeline()
        {
            var runtime = new StaticEcsRuntime<ThrowWorld, ThrowSystems>();
            runtime.Add(new ThrowOnDestroy());
            runtime.Initialize();
            Assert.Throws<InvalidOperationException>(() => runtime.Dispose());
            try
            {
                Assert.Throws<InvalidOperationException>(() => new StaticEcsRuntime<ThrowWorld, ThrowSystems>());
                Assert.Throws<InvalidOperationException>(() => new StaticEcsRuntime<OtherWorld, ThrowSystems>());
            }
            finally
            {
                // The failed hook deliberately poisons this private test world. Repair it so
                // repeated EditMode runs in the same domain do not inherit the failure.
                World<ThrowWorld>.Create();
                World<ThrowWorld>.Systems<ThrowSystems>.Destroy();
                World<ThrowWorld>.Destroy();
                var guard = typeof(StaticEcsRuntime<ThrowWorld, ThrowSystems>).Assembly
                    .GetType("Core.Infrastructure.StaticEcs.WorldOwnership`1")
                    .MakeGenericType(typeof(ThrowWorld));
                guard.GetField("Reserved", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic)
                    .SetValue(null, false);
                var systemsGuard = guard.Assembly.GetType("Core.Infrastructure.StaticEcs.SystemsOwnership`1")
                    .MakeGenericType(typeof(ThrowSystems));
                systemsGuard.GetField("Reserved", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic)
                    .SetValue(null, false);
            }
        }

        private struct CreateFailureWorld : IWorldType { }

        private sealed class Feature : IStaticEcsFeature
        {
            public readonly Counter System = new Counter();
            public void Configure(StaticEcsRuntime<DefaultStaticEcsWorld, DefaultStaticEcsSystems> runtime, Reflex.Core.Container container)
                => runtime.Add(System);
        }

        [Test]
        public void Feature_RegistersSystemBeforeInitializationAndContainerDisposesIt()
        {
            var feature = new Feature();
            var builder = new Reflex.Core.ContainerBuilder();
            builder.AddSingleton(feature, typeof(IStaticEcsFeature));
            var contributor = new StaticEcsRegistrationContributor();
            contributor.RegisterBindings(builder);
            using (var container = builder.Build())
            {
                var owner = container.Resolve<StaticEcsOwner>();
                owner.Initialize(container, false);
                owner.Update();
                Assert.That(feature.System.InitCount, Is.EqualTo(1));
                Assert.That(feature.System.UpdateCount, Is.EqualTo(1));
            }
            Assert.That(feature.System.DestroyCount, Is.EqualTo(1));
        }

        [Test]
        public void Contributor_RegistersSingletonOwnerAndStartupInitializesIt()
        {
            var builder = new Reflex.Core.ContainerBuilder();
            var contributor = new StaticEcsRegistrationContributor();
            contributor.RegisterBindings(builder);
            using (var container = builder.Build())
            {
                var owner = container.Resolve<StaticEcsOwner>();
                Assert.That(container.Resolve<StaticEcsOwner>(), Is.SameAs(owner));
                contributor.Initialize(container);
                Assert.That(World<DefaultStaticEcsWorld>.Status, Is.EqualTo(WorldStatus.Initialized));
            }
            Assert.That(World<DefaultStaticEcsWorld>.Status, Is.EqualTo(WorldStatus.NotCreated));
        }

        [Test]
        public void FailedWorldCreate_DoesNotLeakReservationWhenWorldWasNotCreated()
        {
            Assert.Catch(() => new StaticEcsRuntime<CreateFailureWorld, TestSystems>(
                new WorldConfig { BaseComponentTypesCount = uint.MaxValue }));
            Assert.That(World<CreateFailureWorld>.Status, Is.EqualTo(WorldStatus.NotCreated));
            using (var retry = new StaticEcsRuntime<CreateFailureWorld, TestSystems>()) { }
        }

        [Test]
        public void SecondOwner_IsRejectedEvenWithDifferentPipeline()
        {
            using (var runtime = new StaticEcsRuntime<TestWorld, TestSystems>())
                Assert.Throws<InvalidOperationException>(() => new StaticEcsRuntime<TestWorld, OtherSystems>());
        }

        [Test]
        public void Dispose_TwiceDoesNotDestroyAnotherOwner()
        {
            var first = new StaticEcsRuntime<TestWorld, TestSystems>();
            first.Dispose();
            using (var second = new StaticEcsRuntime<TestWorld, TestSystems>())
            {
                first.Dispose();
                Assert.That(World<TestWorld>.Status, Is.EqualTo(WorldStatus.Created));
            }
        }

        [Test]
        public void InvalidTransitions_AreRejected()
        {
            var runtime = new StaticEcsRuntime<TestWorld, TestSystems>();
            Assert.Throws<InvalidOperationException>(() => runtime.Update());
            runtime.Initialize();
            Assert.Throws<InvalidOperationException>(() => runtime.Add(new Counter()));
            Assert.Throws<InvalidOperationException>(() => runtime.Initialize());
            runtime.Dispose();
            Assert.Throws<ObjectDisposedException>(() => runtime.Update());
        }

        [Test]
        public void Update_WithTrackingDisabled_DoesNotTick()
        {
            using (var runtime = new StaticEcsRuntime<TestWorld, TestSystems>(new WorldConfig { TrackingBufferSize = 0 }))
            {
                runtime.Initialize();
                Assert.DoesNotThrow(() => runtime.Update());
            }
        }

        [Test]
        public void DisposeBeforeInitialize_ReleasesWorld()
        {
            var runtime = new StaticEcsRuntime<TestWorld, TestSystems>();
            runtime.Add(new Counter());
            runtime.Dispose();
            Assert.That(World<TestWorld>.Status, Is.EqualTo(WorldStatus.NotCreated));
        }

        [Test]
        public void Lifecycle_InitializesUpdatesAndDestroysOnce()
        {
            var system = new Counter();
            using (var runtime = new StaticEcsRuntime<TestWorld, TestSystems>())
            {
                runtime.Add(system);
                runtime.Initialize();
                runtime.Update();
                Assert.That(system.InitCount, Is.EqualTo(1));
                Assert.That(system.UpdateCount, Is.EqualTo(1));
            }
            Assert.That(system.DestroyCount, Is.EqualTo(1));
            Assert.That(World<TestWorld>.Status, Is.EqualTo(WorldStatus.NotCreated));
        }
    }
}
