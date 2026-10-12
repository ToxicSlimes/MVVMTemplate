using System.Reflection;
using Core.DependencyInjection.Bindings;
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
    }
}
