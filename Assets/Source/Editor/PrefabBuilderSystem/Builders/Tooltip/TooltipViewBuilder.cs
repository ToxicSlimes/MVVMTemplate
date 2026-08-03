#if UNITY_EDITOR
using Core.Editor.PrefabBuilderSystem.Core;
using Core.Editor.PrefabBuilderSystem.Elements;
using Core.Infrastructure.Popup.Implementations;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Unity.VisualScripting;

namespace Core.Editor.PrefabBuilderSystem.Builders.Tooltip
{
    public class TooltipViewBuilder : ViewBuilder<TooltipPopup>
    {
        public TooltipBuilderSettings Settings { get; set; }

        protected override string PrefabPath => "Assets/Prefabs/Popups/TooltipPopup.prefab";

        protected override bool ValidateBeforeSave => false;
        protected override bool UseAnimationOrchestrator => false;

        protected override void SetupCanvas(GameObject go)
        {
            // Popups are children of PopupService's overlay canvas — NO own Canvas
            go.AddComponent<RectTransform>();

            var canvasGroup = go.AddComponent<CanvasGroup>();
            Context.CanvasGroup = canvasGroup;

            var rt = go.GetComponent<RectTransform>();
            if (rt != null)
                rt.localScale = Vector3.one;
        }

        private TextMeshProUGUI _titleText;
        private TextMeshProUGUI _descriptionText;
        private GameObject _descriptionContainer;

        protected override void BuildViewStructure()
        {
            var s = Settings;

            // Background panel with outline
            var bg = new PanelBuilder("Background")
                .WithBackgroundColor(s.BackgroundColor)
                .WithAnchors(Vector2.zero, Vector2.one)
                .Build(Root);

            bg.AddComponent<Outline>().effectColor = s.BorderColor;
            bg.GetComponent<Outline>().effectDistance = new Vector2(s.BorderWidth, -s.BorderWidth);

            // Content layout
            var content = new VerticalLayoutBuilder("Content")
                .WithSpacing(s.Spacing)
                .WithPadding((int)s.Padding)
                .WithAlignment(TextAnchor.UpperLeft)
                .WithAnchors(Vector2.zero, Vector2.one)
                .WithContentSizeFitter(ContentSizeFitter.FitMode.PreferredSize, ContentSizeFitter.FitMode.PreferredSize)
                .Build(Root);

            // Title
            _titleText = new TextBuilder("Title")
                .WithText("Tooltip Title")
                .WithFontSize(s.TitleFontSize)
                .WithColor(s.TitleColor)
                .Bold()
                .Build(content.transform);

            // Description container (hides when no description)
            _descriptionContainer = new GameObject("DescriptionContainer");
            _descriptionContainer.transform.SetParent(content.transform, false);
            var descRect = _descriptionContainer.AddComponent<RectTransform>();
            descRect.sizeDelta = Vector2.zero;

            _descriptionText = new TextBuilder("Description")
                .WithText("Tooltip description text goes here.")
                .WithFontSize(s.DescriptionFontSize)
                .WithColor(s.DescriptionColor)
                .Body()
                .Build(_descriptionContainer.transform);

            // ContentSizeFitter on root
            var rootFitter = Root.AddComponent<ContentSizeFitter>();
            rootFitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
            rootFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            // Max width via LayoutElement
            var rootLayout = Root.AddComponent<LayoutElement>();
            rootLayout.preferredWidth = s.MaxWidth;

            // CanvasGroup already added in SetupCanvas
        }

        protected override void WireFields(TooltipPopup view)
        {
            var so = new SerializedObject(view);

            // PopupBase fields
            SerializationHelper.Wire(so, "_canvasGroup", view.GetComponent<CanvasGroup>());
            SerializationHelper.Wire(so, "_contentRoot", view.GetComponent<RectTransform>());

            SerializationHelper.Wire(so, "_titleText", _titleText.GetComponentInChildren<TextMeshProUGUI>());
            SerializationHelper.Wire(so, "_descriptionText", _descriptionText.GetComponentInChildren<TextMeshProUGUI>());
            SerializationHelper.Wire(so, "_descriptionContainer", _descriptionContainer);
            so.ApplyModifiedPropertiesWithoutUndo();
        }
    }
}
#endif
