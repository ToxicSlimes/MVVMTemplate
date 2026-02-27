using Core.Abstractions.MVVM.Elements;
using Core.Abstractions.Popup;
using Core.Data.Domain.Models.Elements.Samples;
using Core.Data.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Core.Presentation.Elements.Samples
{
    public class StatBarElement : BindableElement<StatBarData>
    {
        [Header("Stat Bar")]
        [SerializeField] private TMP_Text _labelText;
        [SerializeField] private TMP_Text _valueText;
        [SerializeField] private Image _fillImage;

        protected override void OnBind(StatBarData data)
        {
            if (_labelText != null)
                _labelText.text = data.Label;

            UpdateValue(data);
        }

        protected override void OnRefresh(StatBarData oldData, StatBarData newData)
        {
            UpdateValue(newData);
        }

        private void UpdateValue(StatBarData data)
        {
            if (_valueText != null)
            {
                _valueText.text = !string.IsNullOrEmpty(data.FormattedValue)
                    ? data.FormattedValue
                    : $"{data.CurrentValue:F0}/{data.MaxValue:F0}";
            }

            if (_fillImage != null && data.MaxValue > 0f)
                _fillImage.fillAmount = data.CurrentValue / data.MaxValue;
        }

        protected override IPopupData GetPopupData(StatBarData data)
        {
            var valueText = !string.IsNullOrEmpty(data.FormattedValue)
                ? data.FormattedValue
                : $"{data.CurrentValue:F0}/{data.MaxValue:F0}";
            return new TooltipData(data.Label, valueText);
        }

        protected override void OnUnbind()
        {
            if (_labelText != null)
                _labelText.text = string.Empty;

            if (_valueText != null)
                _valueText.text = string.Empty;

            if (_fillImage != null)
                _fillImage.fillAmount = 0f;
        }
    }
}
