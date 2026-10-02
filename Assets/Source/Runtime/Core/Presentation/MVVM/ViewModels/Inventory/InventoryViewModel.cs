using System;
using System.Collections.Generic;
using System.Linq;
using Core.Abstractions.MVVM.Models.Inventory;
using Core.Abstractions.MVVM.ViewModels;
using Core.Application.Commands.Common;
using Core.Application.Commands.Inventory;
using Core.Domain.Enums;
using Core.Domain.Models.Elements;
using Core.Mediator.Abstractions;
using Core.Mediator.Attributes;
using Cysharp.Threading.Tasks;
using R3;

namespace Core.Presentation.MVVM.ViewModels.Inventory
{
    [GenerateViewCommands]
    public class InventoryViewModel : DisposableObject
    {
        private readonly IInventoryModel _model;
        private readonly IMediator _mediator;

        public ReactiveProperty<List<InventoryItemData>> Items { get; }
        public ReactiveProperty<List<InventoryItemData>> FilteredItems { get; }
        public ReactiveProperty<ItemCategory> CurrentFilter { get; }
        public ReactiveProperty<InventoryItemData?> SelectedItem { get; }
        public ReactiveProperty<string> SelectedItemName { get; }
        public ReactiveProperty<string> SelectedItemDescription { get; }
        public ReactiveProperty<bool> HasSelection { get; }
        public ReactiveProperty<int> ItemCount { get; }

        public ReactiveCommand<Unit> RemoveSelectedCommand { get; set; }
        public ReactiveCommand<Unit> ClearSelectionCommand { get; set; }
        public ReactiveCommand<Unit> BackCommand { get; set; }

        public InventoryViewModel(IInventoryModel model, IMediator mediator)
        {
            _model = model ?? throw new ArgumentNullException(nameof(model));
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));

            Items = new ReactiveProperty<List<InventoryItemData>>(_model.Items);
            CurrentFilter = new ReactiveProperty<ItemCategory>(ItemCategory.All);
            FilteredItems = new ReactiveProperty<List<InventoryItemData>>(ApplyFilter(_model.Items, ItemCategory.All));
            SelectedItem = new ReactiveProperty<InventoryItemData?>(null);
            SelectedItemName = new ReactiveProperty<string>(string.Empty);
            SelectedItemDescription = new ReactiveProperty<string>(string.Empty);
            HasSelection = new ReactiveProperty<bool>(false);
            ItemCount = new ReactiveProperty<int>(_model.Items.Count);

            SetupModelSubscriptions();
            SetupCommands();
        }

        private void SetupModelSubscriptions()
        {
            _model.InventoryChanged += OnInventoryChanged;
            _model.SelectionChanged += OnSelectionChanged;
        }

        private void SetupCommands()
        {
            RemoveSelectedCommand = new ReactiveCommand<Unit>(_ => OnRemoveSelected().Forget());
            RemoveSelectedCommand.AddTo(ref Disposables);

            ClearSelectionCommand = new ReactiveCommand<Unit>(_ => OnClearSelection());
            ClearSelectionCommand.AddTo(ref Disposables);

            BackCommand = new ReactiveCommand<Unit>(_ => OnBack().Forget());
            BackCommand.AddTo(ref Disposables);
        }

        private void OnInventoryChanged(List<InventoryItemData> items)
        {
            Items.Value = items;
            ItemCount.Value = items.Count;
            FilteredItems.Value = ApplyFilter(items, CurrentFilter.Value);
        }

        private void OnSelectionChanged(InventoryItemData? item)
        {
            SelectedItem.Value = item;
            HasSelection.Value = item.HasValue;
            SelectedItemName.Value = item?.Name ?? string.Empty;
            SelectedItemDescription.Value = item?.Description ?? string.Empty;
        }

        public void SetFilter(ItemCategory category)
        {
            CurrentFilter.Value = category;
            FilteredItems.Value = ApplyFilter(Items.Value, category);
        }

        public void SelectItem(string id)
        {
            OnSelectItem(id).Forget();
        }

        public void MoveItem(int fromIndex, int toIndex)
        {
            OnMoveItem(fromIndex, toIndex).Forget();
        }

        private async UniTaskVoid OnMoveItem(int fromIndex, int toIndex)
        {
            await _mediator.Send(new MoveItemCommand(fromIndex, toIndex));
        }

        private async UniTaskVoid OnSelectItem(string id)
        {
            await _mediator.Send(new SelectItemCommand(id));
        }

        private async UniTaskVoid OnRemoveSelected()
        {
            if (!SelectedItem.Value.HasValue) return;
            await _mediator.Send(new RemoveItemCommand(SelectedItem.Value.Value.Id));
        }

        private void OnClearSelection()
        {
            _model.ClearSelection();
        }

        private async UniTaskVoid OnBack()
        {
            await _mediator.Send(new NavigateBackCommand());
        }

        private static List<InventoryItemData> ApplyFilter(List<InventoryItemData> items, ItemCategory category)
        {
            if (category == ItemCategory.All)
                return new List<InventoryItemData>(items);

            return items.Where(i => i.Category == category).ToList();
        }

        public override void Dispose()
        {
            _model.InventoryChanged -= OnInventoryChanged;
            _model.SelectionChanged -= OnSelectionChanged;
            base.Dispose();
        }
    }
}
