using System;
using System.Collections.Generic;
using System.Linq;
using Core.Abstractions.MVVM.Models.Inventory;
using Core.Data.Domain.Enums;
using Core.Data.Domain.Models.Elements;

namespace Core.MVVM.Models.Inventory
{
    /// <summary>
    /// Concrete inventory model with sample data.
    /// </summary>
    public class InventoryModel : IInventoryModel
    {
        public List<InventoryItemData> Items { get; private set; }
        public InventoryItemData? SelectedItem { get; private set; }
        public event Action<List<InventoryItemData>> InventoryChanged;
        public event Action<InventoryItemData?> SelectionChanged;
        public event Action<int, int> ItemMoved;

        public InventoryModel()
        {
            Items = new List<InventoryItemData>
            {
                new() { Id = "sword-01", Name = "Iron Sword", Description = "A sturdy iron blade.", Quantity = 1, Rarity = ItemRarity.Common, Category = ItemCategory.Weapon },
                new() { Id = "shield-01", Name = "Wooden Shield", Description = "A basic wooden shield.", Quantity = 1, Rarity = ItemRarity.Common, Category = ItemCategory.Armor },
                new() { Id = "potion-hp", Name = "Health Potion", Description = "Restores 50 HP.", Quantity = 5, Rarity = ItemRarity.Common, Category = ItemCategory.Consumable },
                new() { Id = "potion-mp", Name = "Mana Potion", Description = "Restores 30 MP.", Quantity = 3, Rarity = ItemRarity.Uncommon, Category = ItemCategory.Consumable },
                new() { Id = "bow-01", Name = "Hunter's Bow", Description = "A lightweight hunting bow.", Quantity = 1, Rarity = ItemRarity.Uncommon, Category = ItemCategory.Weapon },
                new() { Id = "armor-01", Name = "Chainmail Armor", Description = "Provides decent protection.", Quantity = 1, Rarity = ItemRarity.Rare, Category = ItemCategory.Armor },
                new() { Id = "ring-01", Name = "Ring of Fortitude", Description = "Increases max HP by 20.", Quantity = 1, Rarity = ItemRarity.Rare, Category = ItemCategory.Accessory },
                new() { Id = "staff-01", Name = "Arcane Staff", Description = "Channels powerful magic.", Quantity = 1, Rarity = ItemRarity.Epic, Category = ItemCategory.Weapon },
                new() { Id = "amulet-01", Name = "Dragon Amulet", Description = "Grants fire resistance.", Quantity = 1, Rarity = ItemRarity.Epic, Category = ItemCategory.Accessory },
                new() { Id = "sword-legend", Name = "Excalibur", Description = "The legendary sword of kings.", Quantity = 1, Rarity = ItemRarity.Legendary, Category = ItemCategory.Weapon },
                new() { Id = "helmet-01", Name = "Steel Helmet", Description = "Protects from head blows.", Quantity = 1, Rarity = ItemRarity.Common, Category = ItemCategory.Armor },
                new() { Id = "boots-01", Name = "Leather Boots", Description = "Light and comfortable.", Quantity = 1, Rarity = ItemRarity.Common, Category = ItemCategory.Armor },
                new() { Id = "scroll-fire", Name = "Scroll of Fireball", Description = "Casts a powerful fireball.", Quantity = 2, Rarity = ItemRarity.Rare, Category = ItemCategory.Consumable },
                new() { Id = "scroll-heal", Name = "Scroll of Healing", Description = "Heals all party members.", Quantity = 1, Rarity = ItemRarity.Uncommon, Category = ItemCategory.Consumable },
                new() { Id = "ring-02", Name = "Shadow Ring", Description = "Grants stealth in darkness.", Quantity = 1, Rarity = ItemRarity.Epic, Category = ItemCategory.Accessory },
                new() { Id = "amulet-02", Name = "Amulet of Wisdom", Description = "Increases mana regeneration.", Quantity = 1, Rarity = ItemRarity.Uncommon, Category = ItemCategory.Accessory },
                new() { Id = "gem-ruby", Name = "Ruby Gem", Description = "A precious red gemstone.", Quantity = 3, Rarity = ItemRarity.Rare, Category = ItemCategory.Material },
                new() { Id = "gem-sapphire", Name = "Sapphire Gem", Description = "A brilliant blue gemstone.", Quantity = 2, Rarity = ItemRarity.Rare, Category = ItemCategory.Material },
                new() { Id = "ore-iron", Name = "Iron Ore", Description = "Raw iron for smelting.", Quantity = 10, Rarity = ItemRarity.Common, Category = ItemCategory.Material },
                new() { Id = "ore-mithril", Name = "Mithril Ore", Description = "Extremely rare magical ore.", Quantity = 1, Rarity = ItemRarity.Legendary, Category = ItemCategory.Material },
                new() { Id = "quest-key", Name = "Ancient Key", Description = "Opens the sealed dungeon door.", Quantity = 1, Rarity = ItemRarity.Rare, Category = ItemCategory.Quest },
                new() { Id = "quest-letter", Name = "Royal Letter", Description = "A sealed letter from the king.", Quantity = 1, Rarity = ItemRarity.Uncommon, Category = ItemCategory.Quest },
                new() { Id = "quest-map", Name = "Treasure Map", Description = "Marks the location of hidden treasure.", Quantity = 1, Rarity = ItemRarity.Epic, Category = ItemCategory.Quest },
                new() { Id = "dagger-01", Name = "Poison Dagger", Description = "A dagger coated in venom.", Quantity = 1, Rarity = ItemRarity.Uncommon, Category = ItemCategory.Weapon },
                new() { Id = "shield-tower", Name = "Tower Shield", Description = "A massive shield for full cover.", Quantity = 1, Rarity = ItemRarity.Rare, Category = ItemCategory.Armor },
                new() { Id = "potion-elixir", Name = "Elixir of Life", Description = "Fully restores HP and MP.", Quantity = 1, Rarity = ItemRarity.Legendary, Category = ItemCategory.Consumable },
            };
        }

        public void AddItem(InventoryItemData item)
        {
            Items.Add(item);
            InventoryChanged?.Invoke(Items);
        }

        public void RemoveItem(string id)
        {
            var index = Items.FindIndex(i => i.Id == id);
            if (index < 0) return;

            Items.RemoveAt(index);

            if (SelectedItem.HasValue && SelectedItem.Value.Id == id)
                ClearSelection();

            InventoryChanged?.Invoke(Items);
        }

        public void MoveItem(int fromIndex, int toIndex)
        {
            if (fromIndex < 0 || fromIndex >= Items.Count) return;
            if (toIndex < 0 || toIndex >= Items.Count) return;
            if (fromIndex == toIndex) return;

            var item = Items[fromIndex];
            Items.RemoveAt(fromIndex);
            Items.Insert(toIndex, item);

            ItemMoved?.Invoke(fromIndex, toIndex);
            InventoryChanged?.Invoke(Items);
        }

        public void SelectItem(string id)
        {
            var item = Items.FirstOrDefault(i => i.Id == id);
            if (item.Id == null) return;

            SelectedItem = item;
            SelectionChanged?.Invoke(SelectedItem);
        }

        public void ClearSelection()
        {
            SelectedItem = null;
            SelectionChanged?.Invoke(null);
        }

        public void Dispose()
        {
            InventoryChanged = null;
            SelectionChanged = null;
            ItemMoved = null;
        }
    }
}
