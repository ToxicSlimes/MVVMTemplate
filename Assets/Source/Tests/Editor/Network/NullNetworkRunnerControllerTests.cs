using System;
using Core.Infrastructure.Services.Network;
using Core.Network.Abstractions.Gateway.Transport;
using Core.Network.Abstractions.Services;
using Core.Network.Abstractions.Types;
using NUnit.Framework;
using Reflex.Core;

namespace Core.Tests.Network
{
    public sealed class NullNetworkRunnerControllerTests
    {
        [Test]
        public void ReportsIdleSession()
        {
            var controller = new NullNetworkRunnerController();

            Assert.That(controller.IsRunning, Is.False);
            Assert.That(controller.Mode, Is.EqualTo(NetworkMode.None));
            Assert.That(controller.LocalPlayer, Is.EqualTo(NetworkPlayerId.None));
            Assert.That(controller.ActivePlayers, Is.Empty);
            Assert.That(controller.IsStateAuthority, Is.False);
        }

        [Test]
        public void StartAndNetworkSceneLoad_FailFast()
        {
            var controller = new NullNetworkRunnerController();

            Assert.Throws<InvalidOperationException>(() => controller.StartAsync(default));
            Assert.Throws<InvalidOperationException>(() => controller.LoadNetworkSceneAsync("Game"));
        }

        [Test]
        public void AddCoreWithoutNetwork_RegistersNullControllerAndNoGateway()
        {
            using var container = new ContainerBuilder().AddCore(networkEnabled: false).Build();

            Assert.That(container.Resolve<INetworkRunnerController>(), Is.TypeOf<NullNetworkRunnerController>());
            Assert.That(container.HasBinding<GatewayServerIngress>(), Is.False);
        }
    }
}
