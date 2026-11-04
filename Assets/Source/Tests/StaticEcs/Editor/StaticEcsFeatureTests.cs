using System;
using Core.Infrastructure.StaticEcs;
using FFS.Libraries.StaticEcs;
using NUnit.Framework;
using Reflex.Attributes;
using Reflex.Core;

namespace Core.Tests.StaticEcs
{
    public sealed class StaticEcsFeatureTests
    {
        private struct FeatureWorld : IWorldType { }
        private struct FeatureSystems : ISystemsType { }
        private struct Health : IComponent { public int Value; }

        private sealed class Dependency { }

        private sealed class CountingSystem : ISystem
        {
            public int InitCount, UpdateCount, DestroyCount;
            public void Init() => InitCount++;
            public void Update() => UpdateCount++;
            public void Destroy() => DestroyCount++;
        }

        private sealed class TestFeature : StaticEcsFeature
        {
            public readonly CountingSystem Counter = new CountingSystem();
            public readonly Dependency ConstructorDependency;

            [Inject] public Dependency PropertyDependency { get; private set; }

            public TestFeature(Dependency dependency)
            {
                ConstructorDependency = dependency;
                AddSystem(Counter);
            }
        }

        private sealed class HealthRepository : StaticEcsRepository<FeatureWorld>
        {
            public World<FeatureWorld>.Entity Spawn(int value)
            {
                var entity = CreateEntity();
                AddComponent<Health>(entity).Value = value;
                return entity;
            }

            public int Read(World<FeatureWorld>.Entity entity) => GetComponent<Health>(entity).Value;
            public bool Has(World<FeatureWorld>.Entity entity) => HasComponent<Health>(entity);
            public bool Remove(World<FeatureWorld>.Entity entity) => RemoveEntity(entity);
        }

        [Test]
        public void CreateFeature_ConstructsWithReflexDependencies_AndReturnsNewInstance()
        {
            var dependency = new Dependency();
            using var container = new ContainerBuilder().AddSingleton(dependency).Build();
            var first = container.CreateFeature<TestFeature>();
            var second = container.CreateFeature<TestFeature>();
            Assert.That(first.ConstructorDependency, Is.SameAs(dependency));
            Assert.That(first.PropertyDependency, Is.SameAs(dependency));
            Assert.That(second, Is.Not.SameAs(first));
        }

        [Test]
        public void Feature_RunsOnlyWhileEnabled_AsStateEnterAndExit()
        {
            using var container = new ContainerBuilder().AddSingleton(new Dependency()).Build();
            var feature = container.CreateFeature<TestFeature>();
            using (var runtime = new StaticEcsRuntime<FeatureWorld, FeatureSystems>())
            {
                runtime.Add(feature);
                runtime.Initialize();
                Assert.That(feature.Counter.InitCount, Is.EqualTo(1));
                runtime.Update();
                Assert.That(feature.Counter.UpdateCount, Is.EqualTo(0));
                feature.Enable();
                runtime.Update();
                runtime.Update();
                Assert.That(feature.Counter.UpdateCount, Is.EqualTo(2));
                feature.Disable();
                runtime.Update();
                Assert.That(feature.Counter.UpdateCount, Is.EqualTo(2));
            }
            Assert.That(feature.Counter.DestroyCount, Is.EqualTo(1));
        }

        [Test]
        public void Init_Twice_Throws()
        {
            var feature = new TestFeature(new Dependency());
            feature.Init();
            Assert.Throws<InvalidOperationException>(() => feature.Init());
            feature.Destroy();
        }

        [Test]
        public void Repository_CreatesReadsAndRemovesEntities()
        {
            using (var runtime = new StaticEcsRuntime<FeatureWorld, FeatureSystems>())
            {
                World<FeatureWorld>.Types().Component<Health>();
                runtime.Initialize();
                var repository = new HealthRepository();
                var entity = repository.Spawn(7);
                Assert.That(repository.Has(entity), Is.True);
                Assert.That(repository.Read(entity), Is.EqualTo(7));
                Assert.That(repository.Remove(entity), Is.True);
                Assert.That(repository.Remove(entity), Is.False);
            }
        }

        [Test]
        public void Repository_WithoutWorld_Throws()
        {
            Assert.Throws<InvalidOperationException>(() => new HealthRepository());
        }
    }
}
