using Core.Data.Domain.Models.Elements;
using Core.Data.UI;
using Core.Abstractions.MVVM.Elements;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Core.MVVM.Elements
{
    public class EquipmentSlotElement : BindableElement<EquipmentSlotData>
    {
        [Header("Equipment Slot")]
        [SerializeField] private Image _itemIcon;
        [SerializeField] private TMP_Text _itemName;
        [SerializeField] private TMP_Text _slotTypeLabel;
        [SerializeField] private TMP_Text _itemLevelText;
        [SerializeField] private GameObject _emptyIndicator;
        [SerializeField] private Image _rarityFrame;

        protected override void OnBind(EquipmentSlotData data)
        {
            if (_itemIcon != null)
            {
                _itemIcon.sprite = data.ItemIcon;
                _itemIcon.enabled = !data.IsEmpty && data.ItemIcon != null;
            }

            if (_itemName != null)
                _itemName.text = data.IsEmpty ? string.Empty : data.ItemName;

            if (_slotTypeLabel != null)
                _slotTypeLabel.text = data.SlotType;

            if (_itemLevelText != null)
                _itemLevelText.text = data.IsEmpty ? string.Empty : $"+{data.ItemLevel}";

            if (_emptyIndicator != null)
                _emptyIndicator.SetActive(data.IsEmpty);

            if (_rarityFrame != null)
                _rarityFrame.color = GetRarityColor(data.Rarity);
        }

        protected override IPopupData GetPopupData(EquipmentSlotData data)
        {
            if (data.IsEmpty)
                return null;

            return new TooltipData(data.ItemName, data.SlotType);
        }

        protected override void OnUnbind()
        {
            if (_itemIcon != null)
            {
                _itemIcon.sprite = null;
                _itemIcon.enabled = false;
            }

            if (_itemName != null)
                _itemName.text = string.Empty;

            if (_slotTypeLabel != null)
                _slotTypeLabel.text = string.Empty;

            if (_itemLevelText != null)
                _itemLevelText.text = string.Empty;

            if (_emptyIndicator != null)
                _emptyIndicator.SetActive(true);

            if (_rarityFrame != null)
                _rarityFrame.color = Color.gray;
        }

        private Color GetRarityColor(int rarity)
        {
            return rarity switch
            {
                0 => new Color(0.6f, 0.6f, 0.6f),
                1 => new Color(0.2f, 0.8f, 0.2f),
                2 => new Color(0.2f, 0.4f, 1f),
                3 => new Color(0.6f, 0.2f, 0.8f),
                4 => new Color(1f, 0.6f, 0f),
                _ => Color.white
            };
        }
    }
}
