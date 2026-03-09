using Core.Infrastructure.Mediator.Abstractions;

namespace Core.Application.Commands.Inventory
{
    public readonly struct RemoveItemCommand : IRequest<Nothing>
    {
        public readonly string ItemId;

        public RemoveItemCommand(string itemId)
        {
            ItemId = itemId;
        }
    }
}
