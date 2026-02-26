using Core.Abstractions.MVVM.Elements;
using Core.Data.Domain.Models.Elements;
using Core.Data.Domain.Models.ServicePanel;
using Core.Data.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Core.MVVM.Elements
{
    public class ServiceSlotElement : BindableElement<ServiceSlotElementData>
    {
        [Header("State Containers")]
        [SerializeField] private GameObject _emptyState;
        [SerializeField] private GameObject _occupiedState;
        [SerializeField] private GameObject _lockedState;

        [Header("Empty State")]
        [SerializeField] private TMP_Text _addLabel;

        [Header("Occupied State")]
        [SerializeField] private RawImage _heroAvatar;
        [SerializeField] private TMP_Text _heroName;
        [SerializeField] private Slider _progressBar;
        [SerializeField] private TMP_Text _timeText;

        [Header("Locked State")]
        [SerializeField] private Image _lockIcon;
        [SerializeField] private TMP_Text _costText;

        protected override void OnBind(ServiceSlotElementData data)
        {
            SetState(data.State);

            switch (data.State)
            {
                case ServiceSlotState.Empty:
                    BindEmptyState();
                    break;
                case ServiceSlotState.Occupied:
                    BindOccupiedState(data);
                    break;
                case ServiceSlotState.Locked:
                    BindLockedState(data);
                    break;
            }
        }

        protected override void OnUnbind()
        {
            SetState(ServiceSlotState.Empty);
            ClearAllFields();
        }

        protected override IPopupData GetPopupData(ServiceSlotElementData data)
        {
            return data.State switch
            {
                ServiceSlotState.Empty => new TooltipData(
                    "Свободный слот",
                    "Нажмите чтобы назначить героя"),
                ServiceSlotState.Occupied => new TooltipData(
                    data.HeroName ?? "Герой",
                    $"Осталось: {data.TimeText}"),
                ServiceSlotState.Locked => new TooltipData(
                    "Заблокировано",
                    $"Стоимость: {data.UnlockCost}"),
                _ => null
            };
        }

        private void SetState(ServiceSlotState state)
        {
            if (_emptyState != null)
                _emptyState.SetActive(state == ServiceSlotState.Empty);

            if (_occupiedState != null)
                _occupiedState.SetActive(state == ServiceSlotState.Occupied);

            if (_lockedState != null)
                _lockedState.SetActive(state == ServiceSlotState.Locked);
        }

        private void BindEmptyState()
        {
            if (_addLabel != null)
                _addLabel.text = "+";
        }

        private void BindOccupiedState(ServiceSlotElementData data)
        {
            if (_heroAvatar != null)
                _heroAvatar.texture = data.HeroAvatar;

            if (_heroName != null)
                _heroName.text = data.HeroName ?? string.Empty;

            if (_progressBar != null)
                _progressBar.value = data.Progress;

            if (_timeText != null)
                _timeText.text = data.TimeText;
        }

        private void BindLockedState(ServiceSlotElementData data)
        {
            if (_costText != null)
                _costText.text = data.UnlockCost > 0 ? $"{data.UnlockCost}" : "Купить";
        }

        private void ClearAllFields()
        {
            if (_heroAvatar != null)
                _heroAvatar.texture = null;

            if (_heroName != null)
                _heroName.text = string.Empty;

            if (_progressBar != null)
                _progressBar.value = 0f;

            if (_timeText != null)
                _timeText.text = string.Empty;

            if (_costText != null)
                _costText.text = string.Empty;

            if (_addLabel != null)
                _addLabel.text = "+";
        }
    }
}
