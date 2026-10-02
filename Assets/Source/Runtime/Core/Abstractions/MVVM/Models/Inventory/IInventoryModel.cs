using System;
using System.Collections.Generic;
using Core.Domain.Models.Elements;

namespace Core.Abstractions.MVVM.Models.Inventory
{
    public interface IInventoryModel : IDisposable
    {
        List<InventoryItemData> Items { get; }
        InventoryItemData? SelectedItem { get; }
        event Action<List<InventoryItemData>> InventoryChanged;
        event Action<InventoryItemData?> SelectionChanged;
        event Action<int, int> ItemMoved;

        void AddItem(InventoryItemData item);
        void RemoveItem(string id);
        void SelectItem(string id);
        void ClearSelection();
        void MoveItem(int fromIndex, int toIndex);
    }
}
