#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using Core.Editor.PrefabBuilderSystem.Theming;
using UnityEngine;

namespace Core.Editor.PrefabBuilderSystem.Templates
{
    public enum UIElementType
    {
        Container,
        VerticalLayout,
        HorizontalLayout,
        Text,
        Image,
        Button,
        InputField,
        Toggle,
        Slider,
        Dropdown,
        ScrollView,
        Grid,
        Spacer
    }

    [Serializable]
    public class UITemplateNode
    {
        [Tooltip("Unique identifier for this node")]
        public string Id;

        [Tooltip("Display name in hierarchy")]
        public string Name;

        [Tooltip("Type of UI element")]
        public UIElementType ElementType = UIElementType.Container;

        [Tooltip("Child nodes")]
        public List<UITemplateNode> Children = new();

        [Header("DesignSystem Integration")]
        [Tooltip("Use DesignSystem styles")]
        public bool UseDesignSystem = true;

        [Tooltip("Typography style name (heading1, body, caption, etc.)")]
        public string TextStyleName;

        [Tooltip("Color name from ColorPalette (textPrimary, indigo, etc.)")]
        public string ColorName;

        [Tooltip("Spacing size (XS, SM, MD, LG, XL, XXL, XXXL)")]
        public SpacingSize SpacingSize = SpacingSize.MD;

        [Tooltip("Background color name")]
        public string BackgroundColorName;

        [Tooltip("Button variant (Primary, Secondary, Ghost, Danger)")]
        public string ButtonVariant;

        [Header("Layout Settings")]
        public bool Stretch = true;
        public Vector2 Size = Vector2.zero;
        public Vector2 MinSize = Vector2.zero;
        public float PreferredWidth = -1f;
        public float PreferredHeight = -1f;
        public float FlexibleWidth = -1f;
        public float FlexibleHeight = -1f;

        [Header("Content")]
        public string Text;
        public string Placeholder;
        public Sprite Sprite;
        public bool Bold;
        public bool Centered;

        [Header("Override Values (when UseDesignSystem = false)")]
        public float FontSize = 14f;
        public Color Color = Color.white;
        public Color BackgroundColor = Color.clear;
        public float Spacing = 10f;
        public int Padding;

        public UITemplateNode() { }

        public UITemplateNode(string id, string name, UIElementType type)
        {
            Id = id;
            Name = name;
            ElementType = type;
        }

        public UITemplateNode AddChild(UITemplateNode child)
        {
            Children.Add(child);
            return this;
        }

        public UITemplateNode AddChild(string id, string name, UIElementType type)
        {
            var child = new UITemplateNode(id, name, type);
            Children.Add(child);
            return child;
        }

        public static UITemplateNode Container(string id, string name = null)
            => new(id, name ?? id, UIElementType.Container);

        public static UITemplateNode VLayout(string id, string name = null)
            => new(id, name ?? id, UIElementType.VerticalLayout);

        public static UITemplateNode HLayout(string id, string name = null)
            => new(id, name ?? id, UIElementType.HorizontalLayout);

        public static UITemplateNode TextNode(string id, string text, string styleName = "body")
            => new(id, id, UIElementType.Text) { Text = text, TextStyleName = styleName };

        public static UITemplateNode ImageNode(string id, string bgColor = null)
            => new(id, id, UIElementType.Image) { BackgroundColorName = bgColor };

        public static UITemplateNode ButtonNode(string id, string label, string variant = "Primary")
            => new(id, id, UIElementType.Button) { Text = label, ButtonVariant = variant };

        public static UITemplateNode InputNode(string id, string placeholder = "Enter text...")
            => new(id, id, UIElementType.InputField) { Placeholder = placeholder };

        public static UITemplateNode SpacerNode(string id = "Spacer", float height = -1f)
            => new(id, id, UIElementType.Spacer) { PreferredHeight = height, FlexibleHeight = height < 0 ? 1f : -1f };
    }
}
#endif
