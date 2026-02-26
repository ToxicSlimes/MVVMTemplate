using Core.Data.Domain.Models.Elements;
using Core.Data.UI;
using Core.Abstractions.MVVM.Elements;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Core.MVVM.Elements
{
    public class StatRowElement : BindableElement<StatRowData>
    {
        [Header("Stat Row")]
        [SerializeField] private Image _statIcon;
        [SerializeField] private TMP_Text _labelText;
        [SerializeField] private TMP_Text _valueText;
        [SerializeField] private Image _progressBar;
        [SerializeField] private GameObject _barContainer;

        protected override void OnBind(StatRowData data)
        {
            if (_labelText != null)
                _labelText.text = data.Label;

            if (_valueText != null)
                _valueText.text = data.Value;

            if (_statIcon != null)
            {
                _statIcon.sprite = data.Icon;
                _statIcon.enabled = data.Icon != null;
            }

            if (_barContainer != null)
                _barContainer.SetActive(data.BarPercent.HasValue);

            if (_progressBar != null && data.BarPercent.HasValue)
                _progressBar.fillAmount = data.BarPercent.Value;
        }

        protected override IPopupData GetPopupData(StatRowData data)
        {
            if (string.IsNullOrEmpty(data.Label))
                return null;

            return new TooltipData(data.Label, data.Value);
        }

        protected override void OnUnbind()
        {
            if (_labelText != null)
                _labelText.text = string.Empty;

            if (_valueText != null)
                _valueText.text = string.Empty;

            if (_statIcon != null)
            {
                _statIcon.sprite = null;
                _statIcon.enabled = false;
            }

            if (_barContainer != null)
                _barContainer.SetActive(false);

            if (_progressBar != null)
                _progressBar.fillAmount = 0f;
        }
    }
}
