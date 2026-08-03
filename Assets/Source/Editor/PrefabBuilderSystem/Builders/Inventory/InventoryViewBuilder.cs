#if UNITY_EDITOR
using System;
using Core.Data.Domain.Enums;
using Core.Editor.PrefabBuilderSystem.Core;
using Core.Editor.PrefabBuilderSystem.Elements;
using Core.MVVM.Views.Inventory;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Core.Editor.PrefabBuilderSystem.Builders.Inventory
{
    public class InventoryViewBuilder : ViewBuilder<InventoryView>
    {
        public InventoryViewBuilderSettings Settings { get; set; }

        protected override string PrefabPath => "Assets/Prefabs/Views/InventoryView.prefab";

        private Button _backButton;
        private TextMeshProUGUI _itemCountText;
        private Transform _filterBarContainer;
        private Button[] _filterButtons;
        private Transform _itemsContainer;
        private GameObject _detailPanel;
        private TextMeshProUGUI _selectedNameText;
        private TextMeshProUGUI _selectedDescriptionText;
        private Button _removeButton;

        protected override void BuildViewStructure()
        {
            var s = Settings;

            var modal = new ModalBuilder("InventoryModal")
                .WithTitle("Inventory")
                .WithSize(s.ModalWidth, s.ModalHeight)
                .WithDimColor(s.BgDimColor)
                .WithBackgroundColor(s.ModalBgColor)
                .WithHeaderColor(s.HeaderColor)
                .NoCloseButton()
                .Build(Root);

            var content = modal.transform.Find("Window/Content");
            if (content == null) content = modal.transform;

            var headerRow = new HorizontalLayoutBuilder("HeaderRow")
                .WithSpacing(10f)
                .WithAlignment(TextAnchor.MiddleLeft)
                .WithPreferredSize(null, 35f)
                .Build(content);

            _backButton = new ButtonBuilder("BackButton")
                .WithLabel("\u2190 Back")
                .WithLabelSize(14f)
                .WithBackgroundColor(s.BackButtonColor)
                .WithPreferredSize(80f, 30f)
                .Build(headerRow.transform);

            new SpacerBuilder("HeaderSpacer").Flexible().Build(headerRow.transform);

            _itemCountText = new TextBuilder("ItemCountText")
                .WithText("Items: 0")
                .WithFontSize(s.ItemCountFontSize)
                .WithColor(s.ItemCountColor)
                .WithAlignment(TextAlignmentOptions.MidlineRight)
                .WithPreferredSize(100f, 30f)
                .Build(headerRow.transform);

            BuildFilterBar(content, s);

            var scrollView = new ScrollViewBuilder("ItemsScrollView")
                .Vertical()
                .WithPreferredSize(null, s.ModalHeight - s.DetailPanelHeight - s.FilterBarHeight - 100f)
                .Build(content);

            var viewport = scrollView.ScrollRect.transform.Find("Viewport/Content");
            if (viewport == null) viewport = scrollView.ScrollRect.transform;

            var grid = viewport.gameObject.AddComponent<GridLayoutGroup>();
            grid.cellSize = new Vector2(s.GridCellSize, s.GridCellSize);
            grid.spacing = new Vector2(s.GridSpacing, s.GridSpacing);
            grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            grid.constraintCount = s.GridConstraintCount;
            grid.childAlignment = TextAnchor.UpperLeft;
            grid.padding = new RectOffset(5, 5, 5, 5);

            var contentSizeFitter = viewport.gameObject.GetComponent<ContentSizeFitter>();
            if (contentSizeFitter == null) contentSizeFitter = viewport.gameObject.AddComponent<ContentSizeFitter>();
            contentSizeFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            _itemsContainer = viewport;

            var detailVlg = new VerticalLayoutBuilder("DetailPanel")
                .WithPadding(10)
                .WithSpacing(5f)
                .WithPreferredSize(null, s.DetailPanelHeight)
                .Build(content);
            _detailPanel = detailVlg.gameObject;

            var detailBg = _detailPanel.GetComponent<Image>() ?? _detailPanel.AddComponent<Image>();
            detailBg.color = s.DetailPanelColor;

            _selectedNameText = new TextBuilder("SelectedName")
                .WithText("Item Name")
                .WithFontSize(s.DetailNameFontSize)
                .Bold()
                .WithPreferredSize(null, 25f)
                .Build(_detailPanel.transform);

            _selectedDescriptionText = new TextBuilder("SelectedDescription")
                .WithText("Item description goes here.")
                .WithFontSize(s.DetailDescFontSize)
                .WithColor(new Color(0.7f, 0.7f, 0.7f))
                .WithPreferredSize(null, 20f)
                .Build(_detailPanel.transform);

            var detailButtonRow = new HorizontalLayoutBuilder("DetailButtonRow")
                .WithAlignment(TextAnchor.MiddleRight)
                .WithPreferredSize(null, 35f)
                .Build(_detailPanel.transform);

            new SpacerBuilder("DetailSpacer").Flexible().Build(detailButtonRow.transform);

            _removeButton = new ButtonBuilder("RemoveButton")
                .WithLabel("Remove")
                .WithLabelSize(13f)
                .WithBackgroundColor(s.RemoveButtonColor)
                .WithPreferredSize(100f, 30f)
                .Build(detailButtonRow.transform);

            _detailPanel.SetActive(false);

            AddPanelAnimator(modal.gameObject, 0, "Preset_PopupModal");
        }

        private void BuildFilterBar(Transform parent, InventoryViewBuilderSettings s)
        {
            var filterRow = new HorizontalLayoutBuilder("FilterBar")
                .WithSpacing(s.FilterBarSpacing)
                .WithAlignment(TextAnchor.MiddleLeft)
                .WithPreferredSize(null, s.FilterBarHeight)
                .Build(parent);

            _filterBarContainer = filterRow.transform;

            var categories = (ItemCategory[])Enum.GetValues(typeof(ItemCategory));
            _filterButtons = new Button[categories.Length];

            for (int i = 0; i < categories.Length; i++)
            {
                var btn = new ButtonBuilder($"Filter_{categories[i]}")
                    .WithLabel(categories[i].ToString())
                    .WithLabelSize(s.FilterFontSize)
                    .WithBackgroundColor(i == 0 ? s.FilterActiveColor : s.FilterInactiveColor)
                    .WithPreferredSize(s.FilterButtonWidth, s.FilterButtonHeight)
                    .Build(_filterBarContainer);

                _filterButtons[i] = btn;
            }
        }

        protected override void WireFields(InventoryView view)
        {
            var so = new SerializedObject(view);

            SerializationHelper.Wire(so, "_backButton", _backButton);
            SerializationHelper.Wire(so, "_itemCountText", _itemCountText);
            SerializationHelper.Wire(so, "_filterBarContainer", _filterBarContainer);

            var filterBtnsProp = so.FindProperty("_filterButtons");
            if (filterBtnsProp != null)
            {
                filterBtnsProp.arraySize = _filterButtons.Length;
                for (int i = 0; i < _filterButtons.Length; i++)
                    filterBtnsProp.GetArrayElementAtIndex(i).objectReferenceValue = _filterButtons[i];
            }
            SerializationHelper.Wire(so, "_itemsContainer", _itemsContainer);
            SerializationHelper.Wire(so, "_detailPanel", _detailPanel);
            SerializationHelper.Wire(so, "_selectedNameText", _selectedNameText);
            SerializationHelper.Wire(so, "_selectedDescriptionText", _selectedDescriptionText);
            SerializationHelper.Wire(so, "_removeButton", _removeButton);

            so.ApplyModifiedPropertiesWithoutUndo();
        }
    }
}
#endif
