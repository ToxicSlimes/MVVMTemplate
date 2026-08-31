using Core.Infrastructure.Mediator.Abstractions;
using Core.Network.Abstractions.Types;

namespace Core.Application.Commands.Network
{
    public readonly struct DespawnNetworkEntityCommand : IRequest<Nothing>
    {
        public readonly NetworkEntityId Id;

        public DespawnNetworkEntityCommand(NetworkEntityId id)
        {
            Id = id;
        }
    }
}
