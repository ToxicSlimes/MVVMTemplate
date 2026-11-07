using Core.Mediator.Abstractions;
using Core.Network.Abstractions.Gateway;
using Core.Network.Abstractions.Gateway.Transport;
using Core.Network.Abstractions.Services;
using Core.Network.FusionAdapter.Gateway;
using Core.Network.FusionAdapter.Input;
using Core.Network.FusionAdapter.Messaging;
using Core.Network.FusionAdapter.Player;
using Core.Network.FusionAdapter.Runner;
using Core.Network.FusionAdapter.Session;
using Core.Network.FusionAdapter.Spawning;
using Reflex.Core;
using ILogger = Core.Abstractions.Services.ILogger;

namespace Core.Network.FusionAdapter.DependencyInjection
{
    public static class FusionNetworkInstaller
    {
        public static ContainerBuilder AddFusionNetwork(this ContainerBuilder builder)
        {
            builder.AddSingleton<INetworkRunnerController>(container => new FusionRunnerController(
                container,
                container.Resolve<IMediator>(),
                container.Resolve<ILogger>()
            ));

            builder.AddSingleton(container => (INetworkRunnerAccessor)container.Resolve<INetworkRunnerController>());

            builder.AddSingleton(_ => new NetworkTickSource());

            builder.AddSingleton(container => (INetworkTickSource)container.Resolve<NetworkTickSource>());

            builder.AddSingleton<INetworkLobby>(container => new FusionLobbyController(
                container.Resolve<IMediator>(),
                container.Resolve<ILogger>()
            ));

            builder.AddSingleton(container => new NetworkPlayerRegistry(container.Resolve<IMediator>()));

            builder.AddSingleton(_ => new PlayerAvatarRegistry());

            builder.AddSingleton(container => new BotRoster(
                container.Resolve<INetworkRunnerController>(),
                container.Resolve<INetworkSpawner>(),
                container.Resolve<NetworkPlayerRegistry>(),
                container.Resolve<ILogger>()
            ));

            builder.AddSingleton(container => (INetworkLocalPlayer)container.Resolve<NetworkPlayerRegistry>());

            builder.AddSingleton<INetworkSpawnPoints>(_ => new SceneSpawnPoints());

            builder.AddSingleton(_ => new GameSessionRegistry());

            builder.AddSingleton(container => (INetworkSessionControl)container.Resolve<GameSessionRegistry>());

            builder.AddSingleton<INetworkSpawner>(container => new FusionSpawner(
                container.Resolve<INetworkRunnerAccessor>()
            ));

            builder.AddSingleton<INetworkMessenger>(container => new FusionMessenger(
                container.Resolve<INetworkRunnerAccessor>(),
                container.Resolve<ILogger>()
            ));

            builder.AddSingleton<IGatewayPayloadCodec>(_ => new JsonGatewayPayloadCodec());
            builder.AddSingleton<IGatewayAuthority>(container => new FusionGatewayAuthority(
                container.Resolve<INetworkRunnerController>()));
            builder.AddSingleton<IGatewayErrorReporter>(container => new GatewayErrorReporter(
                container.Resolve<ILogger>()));
            builder.AddSingleton(container => new GatewayRouter(
                container.All<IGatewayRoute>()));
            builder.AddSingleton(container => new GatewayClientTerminal(
                container.Resolve<INetworkMessenger>(),
                container.Resolve<IGatewayPayloadCodec>()));
            builder.AddSingleton<IGateway>(container => new GatewayPipeline(
                container.All<IGatewayPipe>(),
                container.Resolve<GatewayClientTerminal>()));
            builder.AddSingleton(container => new GatewayServerIngress(
                container.Resolve<INetworkMessenger>(),
                container.Resolve<IGatewayPayloadCodec>(),
                container.Resolve<GatewayRouter>(),
                new GatewayPipeline(
                    container.All<IGatewayPipe>(),
                    container.Resolve<GatewayRouter>()),
                    container.Resolve<IGatewayAuthority>(),
                    container.Resolve<IGatewayErrorReporter>()));

            builder.AddSingleton<INetworkInputSource>(container => new FusionInputSource(
                container.Resolve<INetworkRunnerAccessor>()));

            builder.AddSingleton<INetworkService>(container => new NetworkService(
                container.Resolve<INetworkRunnerController>(),
                container.Resolve<INetworkSpawner>(),
                container.Resolve<INetworkMessenger>(),
                container.Resolve<INetworkInputSource>()
            ));

            return builder;
        }
    }
}
