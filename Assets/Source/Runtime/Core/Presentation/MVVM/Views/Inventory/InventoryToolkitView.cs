using System;
using Core.Domain.Enums;
using Core.Domain.Models.Elements;
using Core.Presentation.MVVM.ViewModels.Inventory;
using Core.UI.Toolkit;
using R3;
using UnityEngine.UIElements;

namespace Core.Presentation.MVVM.Views.Inventory
{
    public class InventoryToolkitView : ToolkitPanelView<InventoryViewModel>
    {
        private const string ItemCountFormat = "Items: {0}";

        private static readonly ItemCategory[] FilterCategories = (ItemCategory[])Enum.GetValues(typeof(ItemCategory));

        private Button _backButton;
        private Label _itemCountLabel;
        private VisualElement _filterBar;
        private ScrollView _itemsScroll;
        private VisualElement _detailPanel;
        private Label _selectedNameLabel;
        private Label _selectedDescriptionLabel;
        private Button _removeButton;

        protected override void BindPanel(ref DisposableBag disposables)
        {
            QueryElements();
            Bind(_backButton, ViewModel.BackCommand, ref disposables);
            Bind(_itemCountLabel, ViewModel.ItemCount, ItemCountFormat, ref disposables);
            BindFilterBar(ref disposables);
            BindItems(ref disposables);
            BindDisplay(_detailPanel, ViewModel.HasSelection, ref disposables);
            Bind(_selectedNameLabel, ViewModel.SelectedItemName, ref disposables);
            Bind(_selectedDescriptionLabel, ViewModel.SelectedItemDescription, ref disposables);
            Bind(_removeButton, ViewModel.RemoveSelectedCommand, ref disposables);
        }

        private void QueryElements()
        {
            if (Root == null) return;
            _backButton = Root.Q<Button>("back-button");
            _itemCountLabel = Root.Q<Label>("item-count-label");
            _filterBar = Root.Q<VisualElement>("filter-bar");
            _itemsScroll = Root.Q<ScrollView>("items-scroll");
            _detailPanel = Root.Q<VisualElement>("detail-panel");
            _selectedNameLabel = Root.Q<Label>("selected-name-label");
            _selectedDescriptionLabel = Root.Q<Label>("selected-description-label");
            _removeButton = Root.Q<Button>("remove-button");
        }

        private void BindFilterBar(ref DisposableBag disposables)
        {
            if (_filterBar == null) return;

            _filterBar.Clear();
            for (int i = 0; i < FilterCategories.Length; i++)
            {
                var category = FilterCategories[i];
                var button = new Button { text = category.ToString().ToUpperInvariant(), name = $"filter-{category}" };
                button.AddToClassList("btn");
                button.AddToClassList("filter-button");
                var captured = category;
                button.clicked += () => ViewModel.SetFilter(captured);
                _filterBar.Add(button);
            }

            ViewModel.CurrentFilter
                .Subscribe(this, static (filter, self) => self.UpdateFilterVisuals(filter))
                .AddTo(ref disposables);
        }

        private void UpdateFilterVisuals(ItemCategory activeFilter)
        {
            if (_filterBar == null) return;

            for (int i = 0; i < _filterBar.childCount && i < FilterCategories.Length; i++)
            {
                if (_filterBar[i] is not Button button) continue;
                var isActive = FilterCategories[i] == activeFilter;
                button.EnableInClassList("btn-primary", isActive);
                button.EnableInClassList("btn-outline", !isActive);
            }
        }

        private void BindItems(ref DisposableBag disposables)
        {
            ViewModel.FilteredItems
                .Subscribe(this, static (items, self) => self.RebuildItems(items))
                .AddTo(ref disposables);
        }

        private void RebuildItems(System.Collections.Generic.List<InventoryItemData> items)
        {
            if (_itemsScroll == null) return;

            var container = _itemsScroll.contentContainer;
            container.Clear();

            foreach (var item in items)
            {
                var captured = item;
                var button = new Button(() => ViewModel.SelectItem(captured.Id))
                {
                    text = $"{captured.Name}  ({captured.Category})"
                };
                button.AddToClassList("btn");
                button.AddToClassList("btn-secondary");
                button.AddToClassList("item-button");
                container.Add(button);
            }
        }
    }
}
