using Core.Infrastructure.Mediator.Abstractions;
using Core.Network.Abstractions.Types;

namespace Core.Application.Commands.Network
{
    public readonly struct StartNetworkSessionCommand : IRequest<NetworkStartResult>
    {
        public readonly NetworkStartRequest Request;

        public StartNetworkSessionCommand(NetworkStartRequest request)
        {
            Request = request;
        }
    }
}
