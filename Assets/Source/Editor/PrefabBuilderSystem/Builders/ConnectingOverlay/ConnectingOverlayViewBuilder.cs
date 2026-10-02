#if UNITY_EDITOR
using Core.Editor.PrefabBuilderSystem.Core;
using Core.Editor.PrefabBuilderSystem.Elements;
using Core.Editor.PrefabBuilderSystem.Theming;
using Core.Presentation.MVVM.Views.ConnectingOverlay;
using Core.UI.Controls;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace Core.Editor.PrefabBuilderSystem.Builders.ConnectingOverlay
{
    public class ConnectingOverlayViewBuilder : PanelViewBuilder<ConnectingOverlayView>
    {
        private const string FillSpritePath = "UI/Skin/UISprite.psd";

        private static readonly Vector2 Centered = new(0.5f, 0.5f);

        public ConnectingOverlayViewBuilderSettings Settings { get; set; }

        protected override string PrefabPath => "Assets/Prefabs/Views/ConnectingOverlayView.prefab";

        protected override bool ValidateBeforeSave => false;
        protected override bool UseAnimationOrchestrator => false;
        protected override bool UseBasePanelPrefab => false;

        protected override Color BackgroundColor
        {
            get
            {
                var dim = DesignSystemProvider.Colors.deepSpace;
                dim.a = Settings.DimAlpha;
                return dim;
            }
        }

        private BaseText _titleText;
        private BaseText _statusText;
        private Image _progressFill;
        private BaseButton _cancelButton;

        protected override void SetupCanvas(GameObject go)
        {
            var rt = go.AddComponent<RectTransform>();
            rt.localScale = Vector3.one;
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
            rt.pivot = Centered;

            Context.CanvasGroup = go.AddComponent<CanvasGroup>();
        }

        protected override void BuildPanelStructure()
        {
            BuildWindow(Settings);
        }

        private void BuildWindow(ConnectingOverlayViewBuilderSettings s)
        {
            var window = new CardBuilder("Card_Window")
                .WithBackgroundColor(DesignSystemProvider.Colors.darkSurface)
                .WithBorder(DesignSystemProvider.Colors.cardBorder, s.WindowBorderWidth)
                .WithPadding(s.WindowPadding)
                .WithSpacing(0f)
                .WithAnchors(Centered, Centered)
                .WithPivot(Centered)
                .WithSize(s.WindowWidth, s.WindowHeight)
                .Build(Root);

            _titleText = new TextBuilder("Title")
                .WithText("CONNECTING")
                .WithFontSize(s.TitleFontSize)
                .WithColor(DesignSystemProvider.Colors.textPrimary)
                .Bold()
                .Centered()
                .NoAutoSize()
                .WithPreferredSize(null, s.TitleHeight)
                .Build(window.transform);

            new SpacerBuilder("Spacer_StatusGap")
                .Height(s.StatusTopSpacing)
                .Build(window.transform);

            _statusText = new TextBuilder("Status")
                .Body()
                .Secondary()
                .Centered()
                .WithText("Looking for room...")
                .NoAutoSize()
                .WithPreferredSize(null, s.StatusHeight)
                .Build(window.transform);

            new SpacerBuilder("Spacer_ProgressGap")
                .Height(s.ProgressTopSpacing)
                .Build(window.transform);

            _progressFill = BuildProgressFill(s, window.transform);

            new SpacerBuilder("Spacer_Flex")
                .Flexible()
                .Build(window.transform);

            _cancelButton = new ButtonBuilder("Cancel")
                .WithLabel("CANCEL")
                .WithLabelSize(s.CancelButtonFontSize)
                .Ghost()
                .WithPreferredSize(null, s.CancelButtonHeight)
                .Build(window.transform);
        }

        private static Image BuildProgressFill(ConnectingOverlayViewBuilderSettings s, Transform parent)
        {
            var track = new ProgressBarBuilder("Progress_Bar")
                .WithBackgroundColor(DesignSystemProvider.Colors.elevatedSurface)
                .WithFillColor(DesignSystemProvider.Colors.indigo)
                .WithHeight(s.ProgressBarHeight)
                .WithValue(0f)
                .Build(parent);

            var fill = track.transform.Find("Fill").GetComponent<Image>();

            var fillRect = fill.rectTransform;
            fillRect.anchorMin = Vector2.zero;
            fillRect.anchorMax = Vector2.one;
            fillRect.offsetMin = Vector2.zero;
            fillRect.offsetMax = Vector2.zero;

            fill.sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>(FillSpritePath);
            fill.type = Image.Type.Filled;
            fill.fillMethod = Image.FillMethod.Horizontal;
            fill.fillOrigin = (int)Image.OriginHorizontal.Left;
            fill.fillAmount = 0f;

            return fill;
        }

        protected override void WireFields(ConnectingOverlayView view)
        {
            WireAll(view, so =>
            {
                SerializationHelper.Wire(so, "_canvasGroup", view.GetComponent<CanvasGroup>());
                SerializationHelper.WireBool(so, "_isPersistent", false);

                SerializationHelper.Wire(so, "_titleText", _titleText);
                SerializationHelper.Wire(so, "_statusText", _statusText);
                SerializationHelper.Wire(so, "_progressFill", _progressFill);
                SerializationHelper.Wire(so, "_cancelButton", _cancelButton);
            });
        }
    }
}
#endif
