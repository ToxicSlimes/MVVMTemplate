using System.Collections.Generic;
using Core.Abstractions.MVVM.Services;
using Core.Domain.Models.Elements.Samples;
using Core.Presentation.MVVM.ViewModels.Counter;
using Core.UI.Toolkit;
using Core.UI.Toolkit.Elements.Samples;
using Cysharp.Threading.Tasks;
using R3;
using Reflex.Attributes;
using UnityEngine;
using UnityEngine.UIElements;

namespace Core.Presentation.MVVM.Views.Counter
{
    public class CounterToolkitView : ToolkitPanelView<CounterViewModel>
    {
        private const string IncrementsFormat = "Total actions: {0}";

        [Inject] private IToolkitElementService _elementService;

        private Label _counterLabel;
        private Label _incrementsLabel;
        private Button _closeButton;
        private Button _incrementButton;
        private Button _decrementButton;
        private Button _resetButton;
        private Button _toggleGalleryButton;
        private Button _galleryBackButton;
        private Label _toggleGalleryLabel;
        private VisualElement _counterPanel;
        private VisualElement _galleryPanel;
        private ScrollView _textCardsContainer;
        private ScrollView _statBarsContainer;
        private ScrollView _actionItemsContainer;

        private ToolkitConfirmOverlay _confirmOverlay;
        private bool _galleryPopulated;

        protected override void BindPanel(ref DisposableBag disposables)
        {
            QueryElements();
            _confirmOverlay ??= Root != null ? new ToolkitConfirmOverlay(Root) : null;

            Bind(_closeButton, ViewModel.BackCommand, ref disposables);
            BindCounterMode(ref disposables);
            BindModeToggle(ref disposables);
            BindResetConfirm(ref disposables);
        }

        private void QueryElements()
        {
            if (Root == null) return;

            _counterPanel = Root.Q<VisualElement>("counter-panel");
            _galleryPanel = Root.Q<VisualElement>("gallery-panel");
            _counterLabel = Root.Q<Label>("counter-label");
            _incrementsLabel = Root.Q<Label>("increments-label");
            _closeButton = Root.Q<Button>("close-button");
            _incrementButton = Root.Q<Button>("increment-button");
            _decrementButton = Root.Q<Button>("decrement-button");
            _resetButton = Root.Q<Button>("reset-button");
            _toggleGalleryButton = Root.Q<Button>("toggle-gallery-button");
            _galleryBackButton = Root.Q<Button>("gallery-back-button");
            _toggleGalleryLabel = Root.Q<Label>("toggle-gallery-label");
            _textCardsContainer = Root.Q<ScrollView>("text-cards-container");
            _statBarsContainer = Root.Q<ScrollView>("stat-bars-container");
            _actionItemsContainer = Root.Q<ScrollView>("action-items-container");
        }

        private void BindCounterMode(ref DisposableBag disposables)
        {
            Bind(_counterLabel, ViewModel.DisplayText, ref disposables);
            Bind(_incrementsLabel, ViewModel.TotalIncrements, IncrementsFormat, ref disposables);
            Bind(_incrementButton, ViewModel.IncrementCommand, ref disposables);
            Bind(_decrementButton, ViewModel.DecrementCommand, ref disposables);

            if (_resetButton == null) return;

            ViewModel.CounterValue
                .Subscribe(_resetButton, static (value, button) => button.SetEnabled(value != 0))
                .AddTo(ref disposables);
        }

        private void BindModeToggle(ref DisposableBag disposables)
        {
            Bind(_toggleGalleryButton, ViewModel.ToggleGalleryCommand, ref disposables);
            Bind(_galleryBackButton, ViewModel.ToggleGalleryCommand, ref disposables);
            Bind(_toggleGalleryLabel, ViewModel.ToggleButtonText, ref disposables);

            ViewModel.IsGalleryMode
                .Subscribe(this, static (isGallery, self) =>
                {
                    if (self._counterPanel != null)
                        self._counterPanel.style.display = isGallery ? DisplayStyle.None : DisplayStyle.Flex;

                    if (self._galleryPanel != null)
                        self._galleryPanel.style.display = isGallery ? DisplayStyle.Flex : DisplayStyle.None;

                    if (isGallery && !self._galleryPopulated)
                    {
                        self._galleryPopulated = true;
                        self.PopulateGallery();
                    }
                })
                .AddTo(ref disposables);
        }

        private void BindResetConfirm(ref DisposableBag disposables)
        {
            if (_resetButton == null || _confirmOverlay == null) return;

            void OnResetClicked()
            {
                var vm = ViewModel;
                _confirmOverlay.Show(
                    "Reset Counter?",
                    $"Current value: {vm.CounterValue.Value}. This will reset to zero.",
                    "Reset",
                    "Keep",
                    () => vm.ResetCommand.Execute(Unit.Default));
            }

            _resetButton.clicked += OnResetClicked;
            disposables.Add(Disposable.Create(() => _resetButton.clicked -= OnResetClicked));
        }

        private void PopulateGallery()
        {
            var textCards = new List<TextCardData>
            {
                new() { Id = "card-1", Title = "Welcome Card", Description = "This is a TextCardToolkitElement — displays a title, description, and optional icon." },
                new() { Id = "card-2", Title = "Architecture", Description = "Built with ToolkitBindableElement<TData>. Data flows via ReconcileToolkitElementsCommand." },
                new() { Id = "card-3", Title = "Popup Support", Description = "Each element sets VisualElement.tooltip for hover hints." }
            };

            _elementService.ReconcileElementsAsync<TextCardToolkitElement, TextCardData>(
                _textCardsContainer.contentContainer,
                textCards).Forget();

            var statBars = new List<StatBarData>
            {
                new() { Id = "stat-hp", Label = "Health", CurrentValue = 75f, MaxValue = 100f },
                new() { Id = "stat-mp", Label = "Mana", CurrentValue = 30f, MaxValue = 80f },
                new() { Id = "stat-xp", Label = "Experience", CurrentValue = 450f, MaxValue = 1000f, FormattedValue = "450/1000 XP" }
            };

            _elementService.ReconcileElementsAsync<StatBarToolkitElement, StatBarData>(
                _statBarsContainer.contentContainer,
                statBars).Forget();

            var actionItems = new List<ActionListItemData>
            {
                new() { Id = "action-1", Title = "Equip Sword", Subtitle = "+12 Attack Power", ActionLabel = "Equip", IsActionEnabled = true },
                new() { Id = "action-2", Title = "Use Potion", Subtitle = "Restore 50 HP", ActionLabel = "Use", IsActionEnabled = true },
                new() { Id = "action-3", Title = "Locked Skill", Subtitle = "Requires Level 10", ActionLabel = "Locked", IsActionEnabled = false }
            };

            _elementService.ReconcileElementsAsync<ActionListItemToolkitElement, ActionListItemData>(
                _actionItemsContainer.contentContainer,
                actionItems,
                OnActionItemClicked).Forget();
        }

        private static void OnActionItemClicked(ActionListItemData data)
        {
            Debug.Log($"[CounterToolkitView] Action clicked: {data.Title} ({data.ActionLabel})");
        }
    }
}
