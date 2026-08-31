#if UNITY_EDITOR
using Core.Editor.PrefabBuilderSystem.Core;
using Core.Editor.PrefabBuilderSystem.Elements;
using Core.Editor.PrefabBuilderSystem.Theming;
using Core.MVVM.Views.Pause;
using Core.UI.Controls;
using UnityEngine;

namespace Core.Editor.PrefabBuilderSystem.Builders.Pause
{
    /// <summary>
    /// Меню паузы: затемнение на весь экран, легенда управления слева и две кнопки по центру.
    /// </summary>
    /// <remarks>
    /// Фон панели выключен, а затемнение нарисовано отдельной заливкой: экран открывается поверх
    /// боя, и за ним должна оставаться видна комната — игрок решает, выходить ли, глядя на то,
    /// что бросает.
    /// </remarks>
    public class PauseViewBuilder : PanelViewBuilder<PauseView>
    {
        private const float DimAlpha = 0.72f;
        private const float PanelWidth = 420f;
        private const float PanelHeight = 260f;
        private const float ButtonWidth = 320f;
        private const float ButtonHeight = 56f;
        private const float TitleFontSize = 34f;
        private const float ButtonFontSize = 20f;

        private const float LegendWidth = 300f;
        private const float LegendHeight = 96f;
        private const float LegendMargin = 48f;
        private const float LegendKeyWidth = 96f;
        private const float LegendRowHeight = 30f;
        private const float LegendFontSize = 17f;
        private const float LegendTitleFontSize = 20f;

        private static readonly (string Key, string Action)[] Controls =
        {
            ("ESC", "PAUSE"),
        };

        protected override string PrefabPath => "Assets/Prefabs/Views/PauseView.prefab";

        protected override bool HasBackground => false;

        private GameObject _panel;

        private BaseButton _resumeButton;
        private BaseButton _leaveButton;

        protected override void BuildPanelStructure()
        {
            BuildDim();
            BuildLegend();
            BuildPanel();
        }

        protected override void ApplyPanelAnimators()
        {
            AddAnimator(_panel, 0, "Preset_FadeScaleBounce");
        }

        private void BuildDim()
        {
            var dim = DesignSystemProvider.Colors.deepSpace;
            dim.a = DimAlpha;

            new ImageBuilder("Dim")
                .WithColor(dim)
                .WithAnchors(Vector2.zero, Vector2.one)
                .WithPivot(new Vector2(0.5f, 0.5f))
                .WithPosition(Vector2.zero)
                .WithSize(Vector2.zero)
                .Build(Root);
        }

        /// <remarks>
        /// Легенда прижата к левому краю, а не сложена с панелью в один столбец: панель ловит
        /// клики, и разросшийся вверх столбец уводил бы кнопки от центра экрана, куда игрок
        /// возвращает курсор после боя.
        /// </remarks>
        private void BuildLegend()
        {
            var legend = new VerticalLayoutBuilder("Legend")
                .WithSpacing(SpacingSize.XS)
                .WithAlignment(TextAnchor.UpperLeft)
                .ForceExpand(false, false)
                .WithPadding((int)DesignSystemProvider.GetSpacing(SpacingSize.LG))
                .WithAnchors(new Vector2(0f, 0.5f), new Vector2(0f, 0.5f))
                .WithPivot(new Vector2(0f, 0.5f))
                .WithPosition(new Vector2(LegendMargin, 0f))
                .WithSize(LegendWidth, LegendHeight)
                .DarkSurface()
                .Build(Root);

            new TextBuilder("LegendTitle")
                .WithText("CONTROLS")
                .WithFontSize(LegendTitleFontSize)
                .WithColor(DesignSystemProvider.Colors.cyan)
                .Bold()
                .NoAutoSize()
                .WithPreferredSize(LegendWidth, LegendRowHeight)
                .Build(legend.transform);

            foreach (var control in Controls)
            {
                BuildLegendRow(legend.transform, control.Key, control.Action);
            }
        }

        private void BuildLegendRow(Transform parent, string key, string action)
        {
            var row = new HorizontalLayoutBuilder($"Row_{key}")
                .WithSpacing(SpacingSize.SM)
                .WithAlignment(TextAnchor.MiddleLeft)
                .ForceExpand(false, false)
                .ControlChildren(false, false)
                .WithPreferredSize(LegendWidth, LegendRowHeight)
                .Build(parent);

            new TextBuilder("Key")
                .WithText(key)
                .WithFontSize(LegendFontSize)
                .WithColor(DesignSystemProvider.Colors.textPrimary)
                .Bold()
                .NoAutoSize()
                .WithPreferredSize(LegendKeyWidth, LegendRowHeight)
                .Build(row.transform);

            new TextBuilder("Action")
                .WithText(action)
                .WithFontSize(LegendFontSize)
                .WithColor(DesignSystemProvider.Colors.textSecondary)
                .NoAutoSize()
                .WithPreferredSize(LegendWidth - LegendKeyWidth, LegendRowHeight)
                .Build(row.transform);
        }

        private void BuildPanel()
        {
            var layout = new VerticalLayoutBuilder("PausePanel")
                .WithSpacing(SpacingSize.LG)
                .WithAlignment(TextAnchor.MiddleCenter)
                .ForceExpand(false, false)
                .WithPadding((int)DesignSystemProvider.GetSpacing(SpacingSize.XL))
                .WithAnchors(new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f))
                .WithPivot(new Vector2(0.5f, 0.5f))
                .WithPosition(Vector2.zero)
                .WithSize(PanelWidth, PanelHeight)
                .DarkSurface()
                .Build(Root);

            _panel = layout.gameObject;

            new TextBuilder("PauseTitle")
                .WithText("PAUSED")
                .WithFontSize(TitleFontSize)
                .WithColor(DesignSystemProvider.Colors.textPrimary)
                .Bold()
                .Centered()
                .NoAutoSize()
                .WithPreferredSize(ButtonWidth, ButtonHeight)
                .Build(_panel.transform);

            _resumeButton = new ButtonBuilder("ResumeButton")
                .WithLabel("RESUME")
                .WithLabelSize(ButtonFontSize)
                .Primary()
                .WithPreferredSize(ButtonWidth, ButtonHeight)
                .Build(_panel.transform);

            _leaveButton = new ButtonBuilder("LeaveButton")
                .WithLabel("LEAVE MATCH")
                .WithLabelSize(ButtonFontSize)
                .Secondary()
                .WithOutline(DesignSystemProvider.Colors.errorText)
                .WithPreferredSize(ButtonWidth, ButtonHeight)
                .Build(_panel.transform);
        }

        protected override void WireFields(PauseView view)
        {
            WireAll(view,
                ("_resumeButton", _resumeButton),
                ("_leaveButton", _leaveButton));
        }
    }
}
#endif
