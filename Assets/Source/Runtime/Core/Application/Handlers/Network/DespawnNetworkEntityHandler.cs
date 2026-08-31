using System;
using System.Threading;
using Core.Application.Commands.Network;
using Core.Infrastructure.Mediator.Abstractions;
using Core.Infrastructure.Mediator.Handlers;
using Core.Network.Abstractions.Services;
using Cysharp.Threading.Tasks;

namespace Core.Application.Handlers.Network
{
    public sealed class DespawnNetworkEntityHandler : IRequestHandler<DespawnNetworkEntityCommand, Nothing>
    {
        private readonly INetworkSpawner _spawner;

        public DespawnNetworkEntityHandler(INetworkSpawner spawner)
        {
            _spawner = spawner;
        }

        public UniTask<Nothing> Handle(DespawnNetworkEntityCommand command, CancellationToken ct)
        {
            if (!_spawner.TryGetEntity(command.Id, out var entity))
            {
                throw new InvalidOperationException($"Сетевая сущность {command.Id} не найдена");
            }

            _spawner.Despawn(entity);
            return UniTask.FromResult(Nothing.Value);
        }
    }
}
