using System;
using Core.Abstractions.MVVM.Elements;
using Core.Abstractions.Popup;
using Core.Abstractions.UI.DragDrop;
using Core.Domain.Enums;
using Core.Domain.Models.Elements;
using Core.Domain.UI;
using Reflex.Attributes;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Core.Presentation.MVVM.Elements
{
    public class InventoryItemElement : BindableElement<InventoryItemData>
    {
        [Inject] private IDragDropService _dragDropService;

        [Header("Item")]
        [SerializeField] private TMP_Text _nameText;
        [SerializeField] private TMP_Text _quantityText;
        [SerializeField] private Image _rarityIndicator;
        [SerializeField] private Image _categoryIndicator;

        public static event Action<InventoryItemData, InventoryItemData> ItemDroppedOnItem;

        protected override void OnBind(InventoryItemData data)
        {
            _dragDropService?.SetupDraggable(gameObject, () => CurrentData);
            _dragDropService?.SetupDropZone(gameObject, OnDropReceived, CanAcceptDrop);

            UpdateVisuals(data);
        }

        protected override void OnRefresh(InventoryItemData oldData, InventoryItemData newData)
        {
            UpdateVisuals(newData);
        }

        private void UpdateVisuals(InventoryItemData data)
        {
            if (_nameText != null)
                _nameText.text = data.Name;

            if (_quantityText != null)
                _quantityText.text = data.Quantity > 1 ? data.Quantity.ToString() : string.Empty;

            if (_rarityIndicator != null)
                _rarityIndicator.color = GetRarityColor(data.Rarity);

            if (_categoryIndicator != null)
                _categoryIndicator.color = GetCategoryColor(data.Category);
        }

        protected override IPopupData GetPopupData(InventoryItemData data)
        {
            return new TooltipData(data.Name, data.Description);
        }

        protected override void OnUnbind()
        {
            base.OnUnbind();

            if (_nameText != null)
                _nameText.text = string.Empty;

            if (_quantityText != null)
                _quantityText.text = string.Empty;
        }

        private bool CanAcceptDrop(object data)
        {
            return data is InventoryItemData item && item.Id != CurrentData.Id;
        }

        private void OnDropReceived(object data)
        {
            if (data is InventoryItemData sourceData)
                ItemDroppedOnItem?.Invoke(sourceData, CurrentData);
        }

        private static Color GetRarityColor(ItemRarity rarity) => rarity switch
        {
            ItemRarity.Common => new Color(0.8f, 0.8f, 0.8f),
            ItemRarity.Uncommon => new Color(0.12f, 0.8f, 0.12f),
            ItemRarity.Rare => new Color(0.25f, 0.5f, 1f),
            ItemRarity.Epic => new Color(0.64f, 0.21f, 0.93f),
            ItemRarity.Legendary => new Color(1f, 0.5f, 0f),
            _ => Color.white
        };

        private static Color GetCategoryColor(ItemCategory category) => category switch
        {
            ItemCategory.Weapon => new Color(0.9f, 0.3f, 0.3f),
            ItemCategory.Armor => new Color(0.4f, 0.6f, 0.9f),
            ItemCategory.Consumable => new Color(0.3f, 0.9f, 0.4f),
            ItemCategory.Accessory => new Color(0.9f, 0.8f, 0.2f),
            ItemCategory.Material => new Color(0.7f, 0.5f, 0.3f),
            ItemCategory.Quest => new Color(0.9f, 0.6f, 0.9f),
            _ => Color.white
        };
    }
}
