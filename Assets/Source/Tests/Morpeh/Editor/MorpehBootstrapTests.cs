using System.Reflection;
using Core.DependencyInjection.Bindings;
using Core.Infrastructure.Morpeh;
using NUnit.Framework;
using Reflex.Core;
using Scellecs.Morpeh;
using UnityEngine;

namespace Core.Tests.CompositionRoot
{
    public sealed class MorpehBootstrapTests
    {
        [TearDown]
        public void TearDown()
        {
            if (World.Default != null && !World.Default.IsDisposed)
            {
                World.Default.Dispose();
            }
        }

        [Test]
        public void ContainerDispose_DoesNotDisposeMorpehDefaultWorld()
        {
            var defaultWorld = World.Create();

            using (var container = new ContainerBuilder()
                       .AddMorpehEcs()
                       .Build())
            {
                var provider = container.Resolve<MorpehWorldProvider>();

                Assert.That(provider.World, Is.SameAs(defaultWorld));
            }

            Assert.That(defaultWorld.IsDisposed, Is.False);
        }

        [Test]
        public void Contributor_RegistersAndEagerlyInitializesDefaultWorldProvider()
        {
            var world = World.Create();
            var contributor = new MorpehRegistrationContributor();
            var builder = new ContainerBuilder();
            contributor.RegisterBindings(builder);
            using (var container = builder.Build())
            {
                contributor.Initialize(container);
                Assert.That(container.Resolve<MorpehWorldProvider>().World, Is.SameAs(world));
            }
        }

        [Test]
        public void Repository_ProvidesNamedAccessToWorldData()
        {
            World.Create();
            var repository = new TestRepository(new MorpehWorldProvider());

            var entity = repository.Create(42);

            Assert.That(repository.Values.Get(entity).Value, Is.EqualTo(42));

            repository.Remove(entity);

            Assert.That(entity.IsNullOrDisposed(), Is.True);
        }

        [Test]
        public void EnsureProjectScope_WhenInstallerIsMissing_InstantiatesProjectScope()
        {
            var existingInstaller = Object.FindFirstObjectByType<ProjectInstaller>();
            if (existingInstaller != null)
            {
                Object.DestroyImmediate(existingInstaller.gameObject);
            }

            var ensureProjectScope = typeof(ProjectScopeBootstrapper).GetMethod(
                "EnsureProjectScope",
                BindingFlags.NonPublic | BindingFlags.Static);

            Assert.That(ensureProjectScope, Is.Not.Null);
            ensureProjectScope.Invoke(null, null);

            var installer = Object.FindFirstObjectByType<ProjectInstaller>();
            try
            {
                Assert.That(installer, Is.Not.Null);
            }
            finally
            {
                if (installer != null)
                {
                    Object.DestroyImmediate(installer.gameObject);
                }
            }
        }

        private struct TestComponent : IComponent
        {
            public int Value;
        }

        private sealed class TestRepository : MorpehRepository
        {
            public TestRepository(MorpehWorldProvider worldProvider)
                : base(worldProvider)
            {
                Values = GetStash<TestComponent>();
            }

            public Stash<TestComponent> Values { get; }

            public Entity Create(int value)
            {
                var entity = CreateEntity();
                Values.Add(entity, new TestComponent { Value = value });
                return entity;
            }

            public void Remove(Entity entity) => RemoveEntity(entity);
        }
    }
}
