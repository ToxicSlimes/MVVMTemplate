using Core.Infrastructure.Mediator.Abstractions;

namespace Core.Application.Commands.Inventory
{
    public readonly struct MoveItemCommand : IRequest<Nothing>
    {
        public readonly int FromIndex;
        public readonly int ToIndex;

        public MoveItemCommand(int fromIndex, int toIndex)
        {
            FromIndex = fromIndex;
            ToIndex = toIndex;
        }
    }
}
