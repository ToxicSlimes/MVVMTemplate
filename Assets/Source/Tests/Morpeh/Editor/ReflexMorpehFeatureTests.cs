using Core.Infrastructure.Morpeh;
using NUnit.Framework;
using Reflex.Attributes;
using Reflex.Core;
using Scellecs.Morpeh.Addons.Feature;

namespace Core.Tests
{
    public sealed class ReflexMorpehFeatureTests
    {
        [Test]
        public void CreateFeature_ConstructsFeatureWithReflexDependencies()
        {
            var dependency = new FeatureDependency();

            using var container = new ContainerBuilder()
                .AddSingleton(dependency)
                .Build();

            var feature = container.CreateFeature<TestFeature>();

            Assert.That(feature.ConstructorDependency, Is.SameAs(dependency));
            Assert.That(feature.PropertyDependency, Is.SameAs(dependency));
        }

        [Test]
        public void CreateFeature_ReturnsNewFeatureForEveryCall()
        {
            using var container = new ContainerBuilder()
                .AddSingleton(new FeatureDependency())
                .Build();

            var first = container.CreateFeature<TestFeature>();
            var second = container.CreateFeature<TestFeature>();

            Assert.That(second, Is.Not.SameAs(first));
        }

        public sealed class FeatureDependency
        {
        }

        public sealed class TestFeature : UpdateFeature
        {
            public FeatureDependency ConstructorDependency { get; }

            [Inject]
            public FeatureDependency PropertyDependency { get; private set; }

            public TestFeature(FeatureDependency constructorDependency)
            {
                ConstructorDependency = constructorDependency;
            }

            protected override void Initialize()
            {
            }
        }
    }
}
