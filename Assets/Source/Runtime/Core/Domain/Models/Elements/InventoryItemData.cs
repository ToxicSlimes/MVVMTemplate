using System;
using Core.Domain.Enums;

namespace Core.Domain.Models.Elements
{
    public struct InventoryItemData : IElementData, IEquatable<InventoryItemData>
    {
        public string Id { get; set; }
        public string Name;
        public string Description;
        public int Quantity;
        public ItemRarity Rarity;
        public ItemCategory Category;
        public string IconAddress;

        public bool Equals(InventoryItemData other) =>
            Id == other.Id && Name == other.Name && Description == other.Description &&
            Quantity == other.Quantity && Rarity == other.Rarity && Category == other.Category &&
            IconAddress == other.IconAddress;

        public override int GetHashCode() => Id != null ? Id.GetHashCode() : 0;
    }
}
