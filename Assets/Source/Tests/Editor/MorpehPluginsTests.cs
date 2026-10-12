using NUnit.Framework;
using Scellecs.Morpeh;
using Scellecs.Morpeh.Addons;
using Scellecs.Morpeh.Helpers;
using Scellecs.Morpeh.SourceGenerator.FilterSystemGenerator;

namespace Core.Tests
{
    public sealed class MorpehPluginsTests
    {
        [Test]
        public void PluginApis_AreAvailableFromPinnedPackages()
        {
            Assert.That(typeof(IEventData).Assembly, Is.EqualTo(typeof(World).Assembly));
            Assert.That(typeof(ISystemStateComponent).Assembly, Is.EqualTo(typeof(World).Assembly));
            Assert.That(typeof(QuerySystem).Assembly, Is.EqualTo(typeof(World).Assembly));
            Assert.That(typeof(EntityHelperExtensions).Assembly, Is.EqualTo(typeof(World).Assembly));
            Assert.That(typeof(MorpehAddons).Assembly, Is.EqualTo(typeof(World).Assembly));
        }

        [Test]
        public void SystemStateProcessor_CanBeCreatedAndProcessed()
        {
            var world = World.Create();

            try
            {
                var componentStash = world.GetStash<SourceGenerationPosition>();
                var stateStash = world.GetStash<PluginState>();
                var entity = world.CreateEntity();
                componentStash.Set(entity, new SourceGenerationPosition { Value = 1 });
                world.Commit();

                var processor = world.Filter
                    .With<SourceGenerationPosition>()
                    .ToSystemStateProcessor(_ => new PluginState { Value = 2 });

                processor.Process();

                Assert.That(stateStash.Has(entity), Is.True);
                Assert.That(stateStash.Get(entity).Value, Is.EqualTo(2));
                processor.Dispose();
            }
            finally
            {
                world.Dispose();
            }
        }

        [Test]
        public void SourceGenerators_GenerateAspectAndFilterSystem()
        {
            var world = World.Create();

            try
            {
                var positionStash = world.GetStash<SourceGenerationPosition>();
                var velocityStash = world.GetStash<SourceGenerationVelocity>();
                var entity = world.CreateEntity();
                positionStash.Set(entity, new SourceGenerationPosition { Value = 3 });
                velocityStash.Set(entity, new SourceGenerationVelocity { Value = 4 });
                world.Commit();

                var aspectFactory = world.GetAspectFactory<SourceGenerationAspect>();
                var aspect = aspectFactory.Get(entity);
                Assert.That(aspect.SourceGenerationPosition.Value, Is.EqualTo(3));
                Assert.That(aspect.SourceGenerationVelocity.Value, Is.EqualTo(4));

                var system = new SourceGenerationFilterSystem { World = world };
                system.OnAwake();
                system.OnUpdate(0f);

                Assert.That(positionStash.Get(entity).Value, Is.EqualTo(7));
                system.Dispose();
            }
            finally
            {
                world.Dispose();
            }
        }
    }

    public struct SourceGenerationPosition : IComponent
    {
        public int Value;
    }

    public struct SourceGenerationVelocity : IComponent
    {
        public int Value;
    }

    public struct PluginState : ISystemStateComponent
    {
        public int Value;
    }

    public partial struct SourceGenerationAspect :
        Scellecs.Morpeh.SourceGenerator.AspectGenerator.IAspect<SourceGenerationPosition, SourceGenerationVelocity>
    {
    }

    public sealed partial class SourceGenerationFilterSystem : FilterSystem
    {
        protected override FilterBuilder GetFilter(FilterBuilder builder)
        {
            return builder
                .With<SourceGenerationPosition>()
                .With<SourceGenerationVelocity>();
        }

        partial void OnUpdate(
            Entity entity,
            ref SourceGenerationPosition position,
            ref SourceGenerationVelocity velocity)
        {
            position.Value += velocity.Value;
        }
    }
}
