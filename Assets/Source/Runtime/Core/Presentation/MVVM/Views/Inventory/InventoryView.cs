using System;
using Core.Abstractions.MVVM.Services;
using Core.Domain.Enums;
using Core.Domain.Models.Elements;
using Core.Presentation.MVVM.Views;
using Core.Presentation.MVVM.Elements;
using Core.Presentation.MVVM.ViewModels.Inventory;
using Core.UI.Controls;
using Cysharp.Threading.Tasks;
using R3;
using Reflex.Attributes;
using UnityEngine;
using UnityEngine.UI;

namespace Core.Presentation.MVVM.Views.Inventory
{
    public class InventoryView : PanelView<InventoryViewModel>
    {
        private const string ItemCountFormat = "Items: {0}";

        [Inject] private IUIElementService _elementService;

        [Header("Header")]
        [SerializeField] private BaseButton _backButton;
        [SerializeField] private BaseText _itemCountText;

        [Header("Filter Bar")]
        [SerializeField] private Transform _filterBarContainer;
        [SerializeField] private BaseButton[] _filterButtons;

        [Header("Items")]
        [SerializeField] private Transform _itemsContainer;

        [Header("Detail Panel")]
        [SerializeField] private GameObject _detailPanel;
        [SerializeField] private BaseText _selectedNameText;
        [SerializeField] private BaseText _selectedDescriptionText;
        [SerializeField] private BaseButton _removeButton;

        private static readonly ItemCategory[] FilterCategories = (ItemCategory[])Enum.GetValues(typeof(ItemCategory));

        protected override void BindPanel(ref DisposableBag disposables)
        {
            Bind(_backButton, ViewModel.BackCommand, ref disposables);
            Bind(_itemCountText, ViewModel.ItemCount, ItemCountFormat, ref disposables);

            BindFilterBar(ref disposables);
            BindItems(ref disposables);

            BindActive(_detailPanel, ViewModel.HasSelection, ref disposables);
            Bind(_selectedNameText, ViewModel.SelectedItemName, ref disposables);
            Bind(_selectedDescriptionText, ViewModel.SelectedItemDescription, ref disposables);
            Bind(_removeButton, ViewModel.RemoveSelectedCommand, ref disposables);
        }

        private void BindItems(ref DisposableBag disposables)
        {
            InventoryItemElement.ItemDroppedOnItem += OnItemReordered;
            Disposable.Create(() => InventoryItemElement.ItemDroppedOnItem -= OnItemReordered)
                .AddTo(ref disposables);

            ViewModel.FilteredItems
                .Subscribe(this, static (items, self) =>
                {
                    self._elementService.ReconcileElementsAsync<InventoryItemElement, InventoryItemData>(
                        self._itemsContainer,
                        items,
                        self.OnItemClicked).Forget();
                })
                .AddTo(ref disposables);
        }

        private void BindFilterBar(ref DisposableBag disposables)
        {
            if (_filterButtons == null) return;

            for (int i = 0; i < _filterButtons.Length && i < FilterCategories.Length; i++)
            {
                if (_filterButtons[i] == null) continue;

                var category = FilterCategories[i];
                _filterButtons[i].OnClickAsObservable()
                    .Subscribe((vm: ViewModel, cat: category), static (_, state) => state.vm.SetFilter(state.cat))
                    .AddTo(ref disposables);
            }

            ViewModel.CurrentFilter
                .Subscribe(this, static (filter, self) => self.UpdateFilterButtonVisuals(filter))
                .AddTo(ref disposables);
        }

        private void UpdateFilterButtonVisuals(ItemCategory activeFilter)
        {
            if (_filterButtons == null) return;

            for (int i = 0; i < _filterButtons.Length && i < FilterCategories.Length; i++)
            {
                if (_filterButtons[i] == null) continue;

                var colors = _filterButtons[i].colors;
                colors.normalColor = FilterCategories[i] == activeFilter
                    ? new Color(0.3f, 0.6f, 1f, 1f)
                    : new Color(0.25f, 0.25f, 0.3f, 1f);
                _filterButtons[i].colors = colors;
            }
        }

        private void OnItemClicked(InventoryItemData data)
        {
            ViewModel.SelectItem(data.Id);
        }

        private void OnItemReordered(InventoryItemData source, InventoryItemData target)
        {
            var items = ViewModel.Items.Value;
            var fromIndex = items.FindIndex(i => i.Id == source.Id);
            var toIndex = items.FindIndex(i => i.Id == target.Id);

            if (fromIndex >= 0 && toIndex >= 0)
                ViewModel.MoveItem(fromIndex, toIndex);
        }
    }
}
