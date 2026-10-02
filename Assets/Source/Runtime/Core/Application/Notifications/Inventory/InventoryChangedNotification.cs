using System.Collections.Generic;
using Core.Domain.Models.Elements;
using Core.Mediator.Abstractions;

namespace Core.Application.Notifications.Inventory
{
    public readonly struct InventoryChangedNotification : INotification
    {
        public readonly List<InventoryItemData> Items;

        public InventoryChangedNotification(List<InventoryItemData> items)
        {
            Items = items;
        }
    }
}
