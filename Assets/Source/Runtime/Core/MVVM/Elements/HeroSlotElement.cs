using Core.Data.Domain.Models.Elements;
using Core.Data.UI;
using Core.Abstractions.MVVM.Elements;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Core.MVVM.Elements
{
    public class HeroSlotElement : BindableElement<HeroSlotData>
    {
        [Header("Hero Slot")]
        [SerializeField] private RawImage _portrait;
        [SerializeField] private TMP_Text _heroName;
        [SerializeField] private TMP_Text _levelText;
        [SerializeField] private Image _healthBar;
        [SerializeField] private GameObject _deadOverlay;

        protected override void OnBind(HeroSlotData data)
        {
            if (_portrait != null)
                _portrait.texture = data.Portrait;

            if (_heroName != null)
                _heroName.text = data.Name;

            if (_levelText != null)
                _levelText.text = data.Level > 0 ? $"Lv.{data.Level}" : string.Empty;

            if (_healthBar != null)
                _healthBar.fillAmount = data.HealthPercent;

            SetSelected(data.IsSelected);

            if (_deadOverlay != null)
                _deadOverlay.SetActive(data.IsDead);
        }

        protected override IPopupData GetPopupData(HeroSlotData data)
        {
            var description = $"Уровень: {data.Level}";
            if (data.IsDead)
                description += " (Мёртв)";
            return new TooltipData(data.Name, description);
        }

        protected override void OnUnbind()
        {
            if (_portrait != null)
                _portrait.texture = null;

            if (_heroName != null)
                _heroName.text = string.Empty;

            if (_levelText != null)
                _levelText.text = string.Empty;

            if (_healthBar != null)
                _healthBar.fillAmount = 1f;

            if (_deadOverlay != null)
                _deadOverlay.SetActive(false);
        }
    }
}
