#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using EndlessAdventure.Editor.PrefabBuilderSystem.Elements;
using EndlessAdventure.Editor.PrefabBuilderSystem.Theming;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace EndlessAdventure.Editor.PrefabBuilderSystem.Templates
{
    public class TemplateBuilder
    {
        private readonly UITemplate _template;
        private readonly Dictionary<string, GameObject> _builtNodes = new();
        private readonly Dictionary<string, Action<GameObject>> _customizers = new();

        public TemplateBuilder(UITemplate template)
        {
            _template = template;
        }

        public TemplateBuilder Customize(string nodeId, Action<GameObject> customizer)
        {
            _customizers[nodeId] = customizer;
            return this;
        }

        public GameObject Build(Transform parent = null)
        {
            if (_template == null || _template.Root == null)
            {
                Debug.LogError("[TemplateBuilder] Template or Root is null!");
                return null;
            }

            _builtNodes.Clear();

            var rootGo = BuildNode(_template.Root, parent);

            if (_template.AddCanvas && rootGo.GetComponent<Canvas>() == null)
            {
                var canvas = rootGo.AddComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;

                if (_template.AddCanvasScaler)
                {
                    var scaler = rootGo.AddComponent<CanvasScaler>();
                    scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                    scaler.referenceResolution = new Vector2(1920, 1080);
                }

                rootGo.AddComponent<GraphicRaycaster>();
            }

            return rootGo;
        }

        public GameObject GetBuiltNode(string id)
        {
            return _builtNodes.TryGetValue(id, out var go) ? go : null;
        }

        public T GetComponent<T>(string nodeId) where T : Component
        {
            var go = GetBuiltNode(nodeId);
            return go != null ? go.GetComponent<T>() : null;
        }

        public GameObject SaveAsPrefab(string path)
        {
            var root = Build();
            if (root == null) return null;

            var dir = System.IO.Path.GetDirectoryName(path);
            if (!System.IO.Directory.Exists(dir))
                System.IO.Directory.CreateDirectory(dir);

            var prefab = PrefabUtility.SaveAsPrefabAsset(root, path);
            UnityEngine.Object.DestroyImmediate(root);

            Debug.Log($"[TemplateBuilder] Saved prefab: {path}");
            AssetDatabase.Refresh();

            return prefab;
        }

        private GameObject BuildNode(UITemplateNode node, Transform parent)
        {
            if (node == null) return null;

            var go = CreateElement(node, parent);
            if (go == null) return null;

            _builtNodes[node.Id] = go;

            if (_customizers.TryGetValue(node.Id, out var customizer))
            {
                customizer?.Invoke(go);
            }

            foreach (var child in node.Children)
            {
                BuildNode(child, go.transform);
            }

            return go;
        }

        private GameObject CreateElement(UITemplateNode node, Transform parent)
        {
            return node.ElementType switch
            {
                UIElementType.Container => CreateContainer(node, parent),
                UIElementType.VerticalLayout => CreateVerticalLayout(node, parent),
                UIElementType.HorizontalLayout => CreateHorizontalLayout(node, parent),
                UIElementType.Text => CreateText(node, parent),
                UIElementType.Image => CreateImage(node, parent),
                UIElementType.Button => CreateButton(node, parent),
                UIElementType.InputField => CreateInputField(node, parent),
                UIElementType.Toggle => CreateToggle(node, parent),
                UIElementType.Slider => CreateSlider(node, parent),
                UIElementType.Dropdown => CreateDropdown(node, parent),
                UIElementType.ScrollView => CreateScrollView(node, parent),
                UIElementType.Grid => CreateGrid(node, parent),
                UIElementType.Spacer => CreateSpacer(node, parent),
                _ => CreateContainer(node, parent)
            };
        }

        private GameObject CreateContainer(UITemplateNode node, Transform parent)
        {
            var go = new GameObject(node.Name);
            go.transform.SetParent(parent, false);
            var rt = go.AddComponent<RectTransform>();

            if (node.Stretch)
            {
                rt.anchorMin = Vector2.zero;
                rt.anchorMax = Vector2.one;
                rt.offsetMin = Vector2.zero;
                rt.offsetMax = Vector2.zero;
            }
            else if (node.Size != Vector2.zero)
            {
                rt.sizeDelta = node.Size;
            }

            if (!string.IsNullOrEmpty(node.BackgroundColorName) || node.BackgroundColor.a > 0)
            {
                var img = go.AddComponent<Image>();
                img.color = GetBackgroundColor(node);
            }

            ApplyLayoutElement(go, node);

            return go;
        }

        private GameObject CreateVerticalLayout(UITemplateNode node, Transform parent)
        {
            var builder = new VerticalLayoutBuilder(node.Name);

            if (node.UseDesignSystem)
            {
                builder.WithSpacing(node.SpacingSize);
                if (node.Padding > 0)
                    builder.WithPadding(node.Padding);
            }
            else
            {
                builder.WithSpacing(node.Spacing);
                if (node.Padding > 0)
                    builder.WithPadding(node.Padding);
            }

            if (!string.IsNullOrEmpty(node.BackgroundColorName))
            {
                builder.WithBackground(GetBackgroundColor(node));
            }
            else if (node.BackgroundColor.a > 0)
            {
                builder.WithBackground(node.BackgroundColor);
            }

            if (node.Stretch)
                builder.WithStretch();
            else if (node.Size != Vector2.zero)
                builder.WithSize(node.Size.x, node.Size.y);

            ApplyLayoutSettings(builder, node);

            var vlg = builder.Build(parent);
            return vlg.gameObject;
        }

        private GameObject CreateHorizontalLayout(UITemplateNode node, Transform parent)
        {
            var builder = new HorizontalLayoutBuilder(node.Name);

            if (node.UseDesignSystem)
            {
                builder.WithSpacing(node.SpacingSize);
                if (node.Padding > 0)
                    builder.WithPadding(node.Padding);
            }
            else
            {
                builder.WithSpacing(node.Spacing);
                if (node.Padding > 0)
                    builder.WithPadding(node.Padding);
            }

            if (!string.IsNullOrEmpty(node.BackgroundColorName))
            {
                builder.WithBackground(GetBackgroundColor(node));
            }
            else if (node.BackgroundColor.a > 0)
            {
                builder.WithBackground(node.BackgroundColor);
            }

            if (node.Stretch)
                builder.WithStretch();
            else if (node.Size != Vector2.zero)
                builder.WithSize(node.Size.x, node.Size.y);

            ApplyLayoutSettings(builder, node);

            var hlg = builder.Build(parent);
            return hlg.gameObject;
        }

        private GameObject CreateText(UITemplateNode node, Transform parent)
        {
            var builder = new TextBuilder(node.Name)
                .WithText(node.Text ?? "");

            if (node.UseDesignSystem && !string.IsNullOrEmpty(node.TextStyleName))
            {
                ApplyTextStyle(builder, node.TextStyleName);

                if (!string.IsNullOrEmpty(node.ColorName))
                {
                    builder.WithColor(DesignSystemProvider.GetColor(node.ColorName));
                }
            }
            else
            {
                builder.WithFontSize(node.FontSize);
                builder.WithColor(node.Color);
            }

            if (node.Bold) builder.Bold();
            if (node.Centered) builder.Centered();

            if (node.Stretch)
                builder.WithStretch();
            else if (node.Size != Vector2.zero)
                builder.WithSize(node.Size.x, node.Size.y);

            ApplyLayoutSettings(builder, node);

            var tmp = builder.Build(parent);
            return tmp.gameObject;
        }

        private GameObject CreateImage(UITemplateNode node, Transform parent)
        {
            var builder = new ImageBuilder(node.Name);

            if (node.Sprite != null)
                builder.WithSprite(node.Sprite);

            if (!string.IsNullOrEmpty(node.BackgroundColorName))
            {
                builder.WithColor(GetBackgroundColor(node));
            }
            else if (node.BackgroundColor.a > 0)
            {
                builder.WithColor(node.BackgroundColor);
            }

            if (node.Stretch)
                builder.WithStretch();
            else if (node.Size != Vector2.zero)
                builder.WithSize(node.Size.x, node.Size.y);

            ApplyLayoutSettings(builder, node);

            var img = builder.Build(parent);
            return img.gameObject;
        }

        private GameObject CreateButton(UITemplateNode node, Transform parent)
        {
            var builder = new ButtonBuilder(node.Name)
                .WithLabel(node.Text ?? "Button");

            if (node.UseDesignSystem && !string.IsNullOrEmpty(node.ButtonVariant))
            {
                ApplyButtonVariant(builder, node.ButtonVariant);
            }
            else
            {
                builder.WithBackgroundColor(node.BackgroundColor);
                builder.WithLabelColor(node.Color);
            }

            if (node.Size != Vector2.zero)
                builder.WithSize(node.Size.x, node.Size.y);

            ApplyLayoutSettings(builder, node);

            var btn = builder.Build(parent);
            return btn.gameObject;
        }

        private GameObject CreateInputField(UITemplateNode node, Transform parent)
        {
            var builder = new InputFieldBuilder(node.Name)
                .WithPlaceholder(node.Placeholder ?? "Enter text...");

            if (!string.IsNullOrEmpty(node.Text))
                builder.WithText(node.Text);

            if (node.UseDesignSystem)
            {
                builder.Themed();
            }
            else
            {
                builder.WithBackgroundColor(node.BackgroundColor);
                builder.WithTextColor(node.Color);
            }

            if (node.Size != Vector2.zero)
                builder.WithSize(node.Size.x, node.Size.y);

            ApplyLayoutSettings(builder, node);

            var input = builder.Build(parent);
            return input.gameObject;
        }

        private GameObject CreateToggle(UITemplateNode node, Transform parent)
        {
            var builder = new ToggleBuilder(node.Name)
                .WithLabel(node.Text ?? "Toggle");

            if (node.Size != Vector2.zero)
                builder.WithSize(node.Size.x, node.Size.y);

            ApplyLayoutSettings(builder, node);

            var toggle = builder.Build(parent);
            return toggle.gameObject;
        }

        private GameObject CreateSlider(UITemplateNode node, Transform parent)
        {
            var builder = new SliderBuilder(node.Name);

            if (node.Size != Vector2.zero)
                builder.WithSize(node.Size.x, node.Size.y);

            ApplyLayoutSettings(builder, node);

            var slider = builder.Build(parent);
            return slider.gameObject;
        }

        private GameObject CreateDropdown(UITemplateNode node, Transform parent)
        {
            var builder = new DropdownBuilder(node.Name);

            if (node.Size != Vector2.zero)
                builder.WithSize(node.Size.x, node.Size.y);

            ApplyLayoutSettings(builder, node);

            var dropdown = builder.Build(parent);
            return dropdown.gameObject;
        }

        private GameObject CreateScrollView(UITemplateNode node, Transform parent)
        {
            var builder = new ScrollViewBuilder(node.Name)
                .Vertical();

            if (!string.IsNullOrEmpty(node.BackgroundColorName))
            {
                builder.WithBackground(GetBackgroundColor(node));
            }

            if (node.PreferredHeight > 0)
                builder.WithPreferredHeight(node.PreferredHeight);

            var result = builder.Build(parent);
            return result.Root.gameObject;
        }

        private GameObject CreateGrid(UITemplateNode node, Transform parent)
        {
            var builder = new GridBuilder(node.Name);

            if (node.Size != Vector2.zero)
                builder.WithCellSize(node.Size.x, node.Size.y);

            if (node.UseDesignSystem)
            {
                var spacing = DesignSystemProvider.GetSpacing(node.SpacingSize);
                builder.WithSpacing(spacing, spacing);
            }
            else
            {
                builder.WithSpacing(node.Spacing, node.Spacing);
            }

            if (node.Padding > 0)
                builder.WithPadding(node.Padding);

            var grid = builder.Build(parent);
            return grid.gameObject;
        }

        private GameObject CreateSpacer(UITemplateNode node, Transform parent)
        {
            var builder = new SpacerBuilder(node.Name);

            if (node.PreferredHeight > 0)
                builder.SetPreferredHeight(node.PreferredHeight);
            if (node.PreferredWidth > 0)
                builder.SetPreferredWidth(node.PreferredWidth);
            if (node.FlexibleHeight > 0)
                builder.SetFlexibleHeight(node.FlexibleHeight);
            if (node.FlexibleWidth > 0)
                builder.SetFlexibleWidth(node.FlexibleWidth);

            var layoutElement = builder.Build(parent);
            return layoutElement.gameObject;
        }

        private void ApplyTextStyle(TextBuilder builder, string styleName)
        {
            switch (styleName.ToLower())
            {
                case "heading1": builder.Heading1(); break;
                case "heading2": builder.Heading2(); break;
                case "heading3": builder.Heading3(); break;
                case "bodylarge": builder.BodyLarge(); break;
                case "body": builder.Body(); break;
                case "bodysmall": builder.BodySmall(); break;
                case "caption": builder.Caption(); break;
                case "button": builder.ButtonText(); break;
            }
        }

        private void ApplyButtonVariant(ButtonBuilder builder, string variant)
        {
            switch (variant.ToLower())
            {
                case "primary": builder.Primary(); break;
                case "secondary": builder.Secondary(); break;
                case "ghost": builder.Ghost(); break;
                case "danger": builder.Danger(); break;
                case "iconbutton": builder.IconButton(); break;
                case "indigo": builder.Indigo(); break;
                case "pink": builder.Pink(); break;
            }
        }

        private Color GetBackgroundColor(UITemplateNode node)
        {
            if (node.UseDesignSystem && !string.IsNullOrEmpty(node.BackgroundColorName))
            {
                return DesignSystemProvider.GetColor(node.BackgroundColorName);
            }
            return node.BackgroundColor;
        }

        private void ApplyLayoutElement(GameObject go, UITemplateNode node)
        {
            if (node.PreferredWidth < 0 && node.PreferredHeight < 0 &&
                node.MinSize == Vector2.zero && node.FlexibleWidth < 0 && node.FlexibleHeight < 0)
                return;

            var le = go.AddComponent<LayoutElement>();
            if (node.PreferredWidth >= 0) le.preferredWidth = node.PreferredWidth;
            if (node.PreferredHeight >= 0) le.preferredHeight = node.PreferredHeight;
            if (node.MinSize.x > 0) le.minWidth = node.MinSize.x;
            if (node.MinSize.y > 0) le.minHeight = node.MinSize.y;
            if (node.FlexibleWidth >= 0) le.flexibleWidth = node.FlexibleWidth;
            if (node.FlexibleHeight >= 0) le.flexibleHeight = node.FlexibleHeight;
        }

        private void ApplyLayoutSettings<TBuilder, TComponent>(BaseElementBuilder<TBuilder, TComponent> builder, UITemplateNode node)
            where TBuilder : BaseElementBuilder<TBuilder, TComponent>
            where TComponent : Component
        {
            if (node.PreferredWidth >= 0 || node.PreferredHeight >= 0)
            {
                if (node.PreferredWidth >= 0 && node.PreferredHeight >= 0)
                    builder.WithPreferredSize(node.PreferredWidth, node.PreferredHeight);
                else if (node.PreferredWidth >= 0)
                    builder.WithPreferredSize(width: node.PreferredWidth);
                else
                    builder.WithPreferredSize(height: node.PreferredHeight);
            }

            if (node.MinSize.x > 0 || node.MinSize.y > 0)
                builder.WithMinSize(node.MinSize.x, node.MinSize.y);

            if (node.FlexibleWidth >= 0 || node.FlexibleHeight >= 0)
            {
                builder.WithFlexible(
                    node.FlexibleWidth >= 0 ? node.FlexibleWidth : null,
                    node.FlexibleHeight >= 0 ? node.FlexibleHeight : null);
            }
        }
    }
}
#endif
