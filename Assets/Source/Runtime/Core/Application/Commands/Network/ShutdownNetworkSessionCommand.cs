using Core.Mediator.Abstractions;

namespace Core.Application.Commands.Network
{
    public readonly struct ShutdownNetworkSessionCommand : IRequest<Nothing>
    {
    }
}
