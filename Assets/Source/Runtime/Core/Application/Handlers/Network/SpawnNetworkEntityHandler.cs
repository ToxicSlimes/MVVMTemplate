using System.Threading;
using Core.Application.Commands.Network;
using Core.Infrastructure.Mediator.Handlers;
using Core.Network.Abstractions.Entities;
using Core.Network.Abstractions.Services;
using Cysharp.Threading.Tasks;

namespace Core.Application.Handlers.Network
{
    public sealed class SpawnNetworkEntityHandler : IRequestHandler<SpawnNetworkEntityCommand, INetworkEntity>
    {
        private readonly INetworkSpawner _spawner;

        public SpawnNetworkEntityHandler(INetworkSpawner spawner)
        {
            _spawner = spawner;
        }

        public UniTask<INetworkEntity> Handle(SpawnNetworkEntityCommand command, CancellationToken ct) =>
            _spawner.SpawnAsync(
                command.Prefab,
                command.Position,
                command.Rotation,
                command.InputAuthority,
                cancellationToken: ct);
    }
}
