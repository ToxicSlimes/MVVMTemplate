using Core.Infrastructure.Mediator.Abstractions;

namespace Core.Application.Commands.Matchmaking
{
    public readonly struct LeaveRoomCommand : IRequest<Nothing>
    {
    }
}
