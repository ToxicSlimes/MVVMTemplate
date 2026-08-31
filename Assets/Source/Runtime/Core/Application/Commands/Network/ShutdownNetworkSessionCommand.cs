using Core.Infrastructure.Mediator.Abstractions;

namespace Core.Application.Commands.Network
{
    public readonly struct ShutdownNetworkSessionCommand : IRequest<Nothing>
    {
    }
}
