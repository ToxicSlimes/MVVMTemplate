using System.Collections.Generic;
using Core.Data.Domain.Models.Elements;
using Core.Infrastructure.Mediator.Abstractions;

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
