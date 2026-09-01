using System.Collections.Generic;
using Core.Abstractions.Popup;
using Core.Application.Commands.Elements;
using Core.Data.Domain.Models.Elements.Samples;
using Core.Data.UI;
using Core.Infrastructure.Mediator.Abstractions;
using Core.MVVM.ViewModels.Counter;
using Core.MVVM.Elements.Samples;
using Core.UI.Controls;
using R3;
using Reflex.Attributes;
using UnityEngine;

namespace Core.MVVM.Views.Counter
{
    public class CounterView : PanelView<CounterViewModel>
    {
        private const string IncrementsFormat = "Total actions: {0}";

        [Inject] private IMediator _mediator;

        [Header("Display")]
        [SerializeField] private BaseText _counterText;
        [SerializeField] private BaseText _incrementsText;

        [Header("Buttons")]
        [SerializeField] private BaseButton _closeButton;
        [SerializeField] private BaseButton _incrementButton;
        [SerializeField] private BaseButton _decrementButton;
        [SerializeField] private BaseButton _resetButton;
        [SerializeField] private BaseButton _toggleGalleryButton;
        [SerializeField] private BaseText _toggleButtonLabel;

        [Header("Panels")]
        [SerializeField] private GameObject _counterPanel;
        [SerializeField] private GameObject _galleryPanel;

        [Header("Tooltips (Hover)")]
        [SerializeField] private PopupTrigger _incrementTooltip;
        [SerializeField] private PopupTrigger _decrementTooltip;

        [Header("Confirm Popup (Click)")]
        [SerializeField] private PopupTrigger _resetConfirm;

        [Header("Gallery Containers")]
        [SerializeField] private Transform _textCardsContainer;
        [SerializeField] private Transform _statBarsContainer;
        [SerializeField] private Transform _actionItemsContainer;

        private bool _galleryPopulated;

        protected override void BindPanel(ref DisposableBag disposables)
        {
            Bind(_closeButton, ViewModel.BackCommand, ref disposables);
            BindCounterMode(ref disposables);
            BindModeToggle(ref disposables);
            SetupPopups();
        }

        private void BindCounterMode(ref DisposableBag disposables)
        {
            Bind(_counterText, ViewModel.DisplayText, ref disposables);
            Bind(_incrementsText, ViewModel.TotalIncrements, IncrementsFormat, ref disposables);

            Bind(_incrementButton, ViewModel.IncrementCommand, ref disposables);
            Bind(_decrementButton, ViewModel.DecrementCommand, ref disposables);

            if (_resetButton == null) return;

            ViewModel.CounterValue
                .Subscribe(_resetButton, static (v, btn) => btn.interactable = v != 0)
                .AddTo(ref disposables);
        }

        private void BindModeToggle(ref DisposableBag disposables)
        {
            Bind(_toggleGalleryButton, ViewModel.ToggleGalleryCommand, ref disposables);
            Bind(_toggleButtonLabel, ViewModel.ToggleButtonText, ref disposables);

            ViewModel.IsGalleryMode
                .Subscribe(this, static (isGallery, self) =>
                {
                    if (self._counterPanel != null) self._counterPanel.SetActive(!isGallery);
                    if (self._galleryPanel != null) self._galleryPanel.SetActive(isGallery);

                    if (isGallery && !self._galleryPopulated)
                    {
                        self._galleryPopulated = true;
                        self.PopulateGallery();
                    }
                })
                .AddTo(ref disposables);
        }

        private void SetupPopups()
        {
            if (_incrementTooltip != null)
                _incrementTooltip.SetTooltipData("Increment", "Add 1 to the counter");

            if (_decrementTooltip != null)
                _decrementTooltip.SetTooltipData("Decrement", "Subtract 1 from the counter");

            if (_resetConfirm == null) return;

            var vm = ViewModel;
            _resetConfirm.SetDataProvider(() => new ConfirmPopupData
            {
                Title = "Reset Counter?",
                Message = $"Current value: {vm.CounterValue.Value}. This will reset to zero.",
                ConfirmText = "Reset",
                CancelText = "Keep",
                OnConfirm = () => vm.ResetCommand.Execute(Unit.Default)
            });
        }

        private void PopulateGallery()
        {
            var textCards = new List<TextCardData>
            {
                new() { Id = "card-1", Title = "Welcome Card", Description = "This is a TextCardElement — displays a title, description, and optional icon." },
                new() { Id = "card-2", Title = "Architecture", Description = "Built with BindableElement<TData> base class. Data flows via ReconcileElementsCommand." },
                new() { Id = "card-3", Title = "Popup Support", Description = "Each element can provide GetPopupData() for automatic tooltip via PopupTrigger." }
            };

            _mediator.Send(new ReconcileElementsCommand<TextCardElement, TextCardData>(
                _textCardsContainer,
                textCards));

            var statBars = new List<StatBarData>
            {
                new() { Id = "stat-hp", Label = "Health", CurrentValue = 75f, MaxValue = 100f },
                new() { Id = "stat-mp", Label = "Mana", CurrentValue = 30f, MaxValue = 80f },
                new() { Id = "stat-xp", Label = "Experience", CurrentValue = 450f, MaxValue = 1000f, FormattedValue = "450/1000 XP" }
            };

            _mediator.Send(new ReconcileElementsCommand<StatBarElement, StatBarData>(
                _statBarsContainer,
                statBars));

            var actionItems = new List<ActionListItemData>
            {
                new() { Id = "action-1", Title = "Equip Sword", Subtitle = "+12 Attack Power", ActionLabel = "Equip", IsActionEnabled = true },
                new() { Id = "action-2", Title = "Use Potion", Subtitle = "Restore 50 HP", ActionLabel = "Use", IsActionEnabled = true },
                new() { Id = "action-3", Title = "Locked Skill", Subtitle = "Requires Level 10", ActionLabel = "Locked", IsActionEnabled = false }
            };

            _mediator.Send(new ReconcileElementsCommand<ActionListItemElement, ActionListItemData>(
                _actionItemsContainer,
                actionItems,
                OnActionItemClicked));
        }

        private static void OnActionItemClicked(ActionListItemData data)
        {
            Debug.Log($"[CounterView] Action clicked: {data.Title} ({data.ActionLabel})");
        }
    }
}
