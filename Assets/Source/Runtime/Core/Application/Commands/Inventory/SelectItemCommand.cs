using Core.Infrastructure.Mediator.Abstractions;

namespace Core.Application.Commands.Inventory
{
    public readonly struct SelectItemCommand : IRequest<Nothing>
    {
        public readonly string ItemId;

        public SelectItemCommand(string itemId)
        {
            ItemId = itemId;
        }
    }
}
