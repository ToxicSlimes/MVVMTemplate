using Core.Data.Domain.Models.Elements;
using Core.Data.UI;
using Core.Abstractions.MVVM.Elements;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Core.MVVM.Elements
{
    public class ResourceItemElement : BindableElement<ResourceItemData>
    {
        [Header("Resource Item")]
        [SerializeField] private Image _icon;
        [SerializeField] private TMP_Text _amountText;
        [SerializeField] private TMP_Text _nameText;

        protected override void OnBind(ResourceItemData data)
        {
            if (_icon != null)
            {
                _icon.sprite = data.Icon;
                _icon.enabled = data.Icon != null;
            }

            if (_amountText != null)
                _amountText.text = data.Amount.ToString("N0");

            if (_nameText != null)
                _nameText.text = data.Name;
        }

        protected override IPopupData GetPopupData(ResourceItemData data)
        {
            if (string.IsNullOrEmpty(data.Name) && string.IsNullOrEmpty(data.Description))
                return null;

            return new TooltipData(data.Name, data.Description);
        }

        protected override void OnUnbind()
        {
            if (_icon != null)
            {
                _icon.sprite = null;
                _icon.enabled = false;
            }

            if (_amountText != null)
                _amountText.text = string.Empty;

            if (_nameText != null)
                _nameText.text = string.Empty;
        }
    }
}
