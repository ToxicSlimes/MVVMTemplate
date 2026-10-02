using Core.Domain.Models.Elements;
using Core.Mediator.Abstractions;

namespace Core.Application.Commands.Inventory
{
    public readonly struct AddItemCommand : IRequest<Nothing>
    {
        public readonly InventoryItemData Item;

        public AddItemCommand(InventoryItemData item)
        {
            Item = item;
        }
    }
}
