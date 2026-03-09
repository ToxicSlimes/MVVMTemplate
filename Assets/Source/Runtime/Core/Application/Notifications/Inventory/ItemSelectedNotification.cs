using Core.Data.Domain.Models.Elements;
using Core.Infrastructure.Mediator.Abstractions;

namespace Core.Application.Notifications.Inventory
{
    public readonly struct ItemSelectedNotification : INotification
    {
        public readonly InventoryItemData? Item;

        public ItemSelectedNotification(InventoryItemData? item)
        {
            Item = item;
        }
    }
}
