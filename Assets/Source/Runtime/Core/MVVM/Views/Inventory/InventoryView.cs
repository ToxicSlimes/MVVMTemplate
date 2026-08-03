using System;
using Core.Infrastructure.MVVM.Animation;
using Core.Application.Commands.Elements;
using Core.Data.Domain.Enums;
using Core.Data.Domain.Models.Elements;
using Core.Infrastructure.Mediator.Abstractions;
using Core.MVVM.Elements;
using Core.MVVM.ViewModels.Inventory;
using Core.UI.Binding;
using Core.UI.Controls;
using R3;
using Reflex.Attributes;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Core.MVVM.Views.Inventory
{
    public class InventoryView : AnimatedView<InventoryViewModel>
    {
        [Inject] private IMediator _mediator;

        [Header("Header")]
        [SerializeField] private BaseButton _backButton;
        [SerializeField] private TMP_Text _itemCountText;

        [Header("Filter Bar")]
        [SerializeField] private Transform _filterBarContainer;
        [SerializeField] private Button[] _filterButtons;

        [Header("Items")]
        [SerializeField] private Transform _itemsContainer;

        [Header("Detail Panel")]
        [SerializeField] private GameObject _detailPanel;
        [SerializeField] private BaseText _selectedNameText;
        [SerializeField] private BaseText _selectedDescriptionText;
        [SerializeField] private BaseButton _removeButton;

        private static readonly ItemCategory[] FilterCategories = (ItemCategory[])Enum.GetValues(typeof(ItemCategory));

        protected override void OnBind(ref DisposableBag disposables)
        {
            if (_backButton != null)
                _backButton.BindTo(ViewModel.BackCommand, ref disposables);

            if (_itemCountText != null)
            {
                ViewModel.ItemCount
                    .Subscribe(_itemCountText, static (count, tmp) => tmp.text = $"Items: {count}")
                    .AddTo(ref disposables);
            }

            BindFilterBar(ref disposables);

            InventoryItemElement.ItemDroppedOnItem += OnItemReordered;
            Disposable.Create(() => InventoryItemElement.ItemDroppedOnItem -= OnItemReordered)
                .AddTo(ref disposables);

            ViewModel.FilteredItems
                .Subscribe(this, static (items, self) =>
                {
                    self._mediator.Send(new ReconcileElementsCommand<InventoryItemElement, InventoryItemData>(
                        self._itemsContainer,
                        items,
                        self.OnItemClicked));
                })
                .AddTo(ref disposables);

            if (_detailPanel != null)
            {
                ViewModel.HasSelection
                    .Subscribe(_detailPanel, static (has, panel) => panel.SetActive(has))
                    .AddTo(ref disposables);
            }

            if (_selectedNameText != null)
                _selectedNameText.BindTo(ViewModel.SelectedItemName, ref disposables);

            if (_selectedDescriptionText != null)
                _selectedDescriptionText.BindTo(ViewModel.SelectedItemDescription, ref disposables);

            if (_removeButton != null)
                _removeButton.BindTo(ViewModel.RemoveSelectedCommand, ref disposables);

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
