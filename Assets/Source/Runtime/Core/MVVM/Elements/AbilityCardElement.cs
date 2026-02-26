using Core.Data.Domain.Models.Elements;
using Core.Data.UI;
using Core.Abstractions.MVVM.Elements;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Core.MVVM.Elements
{
public class AbilityCardElement : BindableElement<HeroAbilityData>
    {
        [Header("Ability Card")]
        [SerializeField] private Image _abilityIcon;
        [SerializeField] private TMP_Text _abilityName;
        [SerializeField] private TMP_Text _description;
        [SerializeField] private GameObject _lockOverlay;
        [SerializeField] private TMP_Text _cooldownText;
        [SerializeField] private TMP_Text _manaCostText;

        protected override void OnBind(HeroAbilityData data)
        {
            if (_abilityIcon != null)
                _abilityIcon.sprite = data.Icon;

            if (_abilityName != null)
                _abilityName.text = data.Name;

            if (_description != null)
                _description.text = data.Description;

            if (_lockOverlay != null)
                _lockOverlay.SetActive(!data.IsUnlocked);

            if (_cooldownText != null)
                _cooldownText.text = data.CooldownTurns > 0 ? data.CooldownTurns.ToString() : string.Empty;

            if (_manaCostText != null)
                _manaCostText.text = data.ManaCost > 0 ? data.ManaCost.ToString() : string.Empty;
        }

        protected override IPopupData GetPopupData(HeroAbilityData data)
        {
            if (string.IsNullOrEmpty(data.Name))
                return null;

            return new TooltipData(data.Name, data.Description);
        }

        protected override void OnUnbind()
        {
            if (_abilityIcon != null)
                _abilityIcon.sprite = null;

            if (_abilityName != null)
                _abilityName.text = string.Empty;

            if (_description != null)
                _description.text = string.Empty;

            if (_lockOverlay != null)
                _lockOverlay.SetActive(false);

            if (_cooldownText != null)
                _cooldownText.text = string.Empty;

            if (_manaCostText != null)
                _manaCostText.text = string.Empty;
        }
    }
}
