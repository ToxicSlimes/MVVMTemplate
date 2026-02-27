using Core.Abstractions.MVVM.Elements;
using Core.Abstractions.Popup;
using Core.Data.Domain.Models.Elements.Samples;
using Core.Data.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Core.Presentation.Elements.Samples
{
    public class TextCardElement : BindableElement<TextCardData>
    {
        [Header("Card")]
        [SerializeField] private TMP_Text _titleText;
        [SerializeField] private TMP_Text _descriptionText;
        [SerializeField] private Image _iconImage;

        protected override void OnBind(TextCardData data)
        {
            if (_titleText != null)
                _titleText.text = data.Title;

            if (_descriptionText != null)
                _descriptionText.text = data.Description;

            if (_iconImage != null && data.Icon != null)
                _iconImage.sprite = data.Icon;
        }

        protected override IPopupData GetPopupData(TextCardData data)
        {
            return new TooltipData(data.Title, data.Description);
        }

        protected override void OnUnbind()
        {
            base.OnUnbind();
            
            if (_titleText != null)
                _titleText.text = string.Empty;

            if (_descriptionText != null)
                _descriptionText.text = string.Empty;
        }
    }
}
