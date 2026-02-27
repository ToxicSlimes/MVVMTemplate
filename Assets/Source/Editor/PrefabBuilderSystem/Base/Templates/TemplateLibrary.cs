#if UNITY_EDITOR
using System.Collections.Generic;
using Core.Editor.PrefabBuilderSystem.Theming;
using UnityEditor;
using UnityEngine;

namespace Core.Editor.PrefabBuilderSystem.Templates
{
    public static class TemplateLibrary
    {
        private const string TemplatesFolder = "Assets/Source/Editor/PrefabBuilderSystem/Templates";

        public static UITemplate CreateModalTemplate()
        {
            var template = ScriptableObject.CreateInstance<UITemplate>();
            template.Initialize("Modal Dialog", "Standard modal with header, content, and footer", "Dialogs");
            template.Tags = new List<string> { "modal", "dialog", "popup" };

            var root = template.Root;
            root.BackgroundColorName = "darkSurface";
            root.Stretch = true;

            var overlay = UITemplateNode.Container("Overlay", "Overlay");
            overlay.BackgroundColor = new Color(0, 0, 0, 0.5f);
            overlay.Stretch = true;
            root.AddChild(overlay);

            var modal = UITemplateNode.VLayout("ModalContainer", "Modal");
            modal.BackgroundColorName = "cardBackground";
            modal.Stretch = false;
            modal.Size = new Vector2(500, 400);
            modal.Padding = 24;
            modal.SpacingSize = SpacingSize.MD;
            overlay.AddChild(modal);

            var header = UITemplateNode.HLayout("Header", "Header");
            header.SpacingSize = SpacingSize.SM;
            header.PreferredHeight = 48;
            modal.AddChild(header);

            var title = UITemplateNode.TextNode("Title", "Modal Title", "heading3");
            title.ColorName = "textPrimary";
            title.FlexibleWidth = 1;
            header.AddChild(title);

            var closeBtn = UITemplateNode.ButtonNode("CloseButton", "×", "IconButton");
            closeBtn.Size = new Vector2(32, 32);
            header.AddChild(closeBtn);

            var content = UITemplateNode.VLayout("Content", "Content");
            content.FlexibleHeight = 1;
            content.SpacingSize = SpacingSize.SM;
            modal.AddChild(content);

            var desc = UITemplateNode.TextNode("Description", "Modal content goes here...", "body");
            desc.ColorName = "textSecondary";
            content.AddChild(desc);

            var footer = UITemplateNode.HLayout("Footer", "Footer");
            footer.SpacingSize = SpacingSize.SM;
            footer.PreferredHeight = 48;
            modal.AddChild(footer);

            var spacer = UITemplateNode.SpacerNode("FooterSpacer");
            footer.AddChild(spacer);

            var cancelBtn = UITemplateNode.ButtonNode("CancelButton", "Cancel", "Secondary");
            cancelBtn.Size = new Vector2(100, 40);
            footer.AddChild(cancelBtn);

            var confirmBtn = UITemplateNode.ButtonNode("ConfirmButton", "Confirm", "Primary");
            confirmBtn.Size = new Vector2(100, 40);
            footer.AddChild(confirmBtn);

            return template;
        }

        public static UITemplate CreateMessageBubbleTemplate()
        {
            var template = ScriptableObject.CreateInstance<UITemplate>();
            template.Initialize("Message Bubble", "Chat message bubble with avatar and timestamp", "Chat");
            template.Tags = new List<string> { "message", "bubble", "chat" };

            var root = template.Root;
            root.ElementType = UIElementType.HorizontalLayout;
            root.SpacingSize = SpacingSize.SM;
            root.Padding = 8;
            root.Stretch = false;
            root.PreferredHeight = -1;

            var avatar = UITemplateNode.ImageNode("Avatar", "elevatedSurface");
            avatar.Size = new Vector2(40, 40);
            root.AddChild(avatar);

            var contentCol = UITemplateNode.VLayout("ContentColumn", "Content");
            contentCol.SpacingSize = SpacingSize.XS;
            contentCol.FlexibleWidth = 1;
            root.AddChild(contentCol);

            var bubble = UITemplateNode.VLayout("Bubble", "Bubble");
            bubble.BackgroundColorName = "aiBubble";
            bubble.Padding = 12;
            bubble.SpacingSize = SpacingSize.XS;
            contentCol.AddChild(bubble);

            var messageText = UITemplateNode.TextNode("MessageText", "Hello! This is a message.", "body");
            messageText.ColorName = "textPrimary";
            bubble.AddChild(messageText);

            var timestamp = UITemplateNode.TextNode("Timestamp", "12:34 PM", "caption");
            timestamp.ColorName = "textTertiary";
            contentCol.AddChild(timestamp);

            return template;
        }

        public static UITemplate CreateUserMessageBubbleTemplate()
        {
            var template = ScriptableObject.CreateInstance<UITemplate>();
            template.Initialize("User Message Bubble", "User chat message with gradient background", "Chat");
            template.Tags = new List<string> { "message", "bubble", "chat", "user" };

            var root = template.Root;
            root.ElementType = UIElementType.HorizontalLayout;
            root.SpacingSize = SpacingSize.SM;
            root.Padding = 8;
            root.Stretch = false;

            var spacer = UITemplateNode.SpacerNode("LeftSpacer");
            spacer.FlexibleWidth = 1;
            root.AddChild(spacer);

            var contentCol = UITemplateNode.VLayout("ContentColumn", "Content");
            contentCol.SpacingSize = SpacingSize.XS;
            contentCol.PreferredWidth = 300;
            root.AddChild(contentCol);

            var bubble = UITemplateNode.VLayout("Bubble", "Bubble");
            bubble.BackgroundColorName = "userBubbleStart";
            bubble.Padding = 12;
            bubble.SpacingSize = SpacingSize.XS;
            contentCol.AddChild(bubble);

            var messageText = UITemplateNode.TextNode("MessageText", "This is a user message.", "body");
            messageText.ColorName = "textPrimary";
            bubble.AddChild(messageText);

            var timestamp = UITemplateNode.TextNode("Timestamp", "12:35 PM", "caption");
            timestamp.ColorName = "textTertiary";
            contentCol.AddChild(timestamp);

            return template;
        }

        public static UITemplate CreateCardTemplate()
        {
            var template = ScriptableObject.CreateInstance<UITemplate>();
            template.Initialize("Card", "Glassmorphism card with header and content", "Components");
            template.Tags = new List<string> { "card", "panel", "glassmorphism" };

            var root = template.Root;
            root.ElementType = UIElementType.VerticalLayout;
            root.BackgroundColorName = "cardBackground";
            root.Padding = 24;
            root.SpacingSize = SpacingSize.MD;
            root.Stretch = false;
            root.Size = new Vector2(300, 200);

            var header = UITemplateNode.VLayout("Header", "Header");
            header.SpacingSize = SpacingSize.XS;
            root.AddChild(header);

            var title = UITemplateNode.TextNode("Title", "Card Title", "heading3");
            title.ColorName = "textPrimary";
            header.AddChild(title);

            var subtitle = UITemplateNode.TextNode("Subtitle", "Card subtitle or description", "bodySmall");
            subtitle.ColorName = "textSecondary";
            header.AddChild(subtitle);

            var content = UITemplateNode.Container("Content", "Content");
            content.FlexibleHeight = 1;
            root.AddChild(content);

            var footer = UITemplateNode.HLayout("Footer", "Footer");
            footer.SpacingSize = SpacingSize.SM;
            root.AddChild(footer);

            var actionBtn = UITemplateNode.ButtonNode("ActionButton", "Action", "Primary");
            actionBtn.Size = new Vector2(100, 36);
            footer.AddChild(actionBtn);

            return template;
        }

        public static UITemplate CreateInputGroupTemplate()
        {
            var template = ScriptableObject.CreateInstance<UITemplate>();
            template.Initialize("Input Group", "Label + Input + Error text", "Forms");
            template.Tags = new List<string> { "input", "form", "field" };

            var root = template.Root;
            root.ElementType = UIElementType.VerticalLayout;
            root.SpacingSize = SpacingSize.XS;
            root.Stretch = false;
            root.PreferredWidth = 300;

            var label = UITemplateNode.TextNode("Label", "Label", "bodySmall");
            label.ColorName = "textSecondary";
            label.Bold = true;
            root.AddChild(label);

            var input = UITemplateNode.InputNode("Input", "Enter value...");
            input.Size = new Vector2(300, 44);
            root.AddChild(input);

            var error = UITemplateNode.TextNode("ErrorText", "", "caption");
            error.ColorName = "errorText";
            root.AddChild(error);

            return template;
        }

        public static UITemplate CreateToolbarTemplate()
        {
            var template = ScriptableObject.CreateInstance<UITemplate>();
            template.Initialize("Toolbar", "Horizontal toolbar with buttons", "Navigation");
            template.Tags = new List<string> { "toolbar", "header", "navigation" };

            var root = template.Root;
            root.ElementType = UIElementType.HorizontalLayout;
            root.BackgroundColorName = "darkSurface";
            root.Padding = 16;
            root.SpacingSize = SpacingSize.MD;
            root.PreferredHeight = 64;
            root.Stretch = false;

            var logo = UITemplateNode.ImageNode("Logo", "indigo");
            logo.Size = new Vector2(40, 40);
            root.AddChild(logo);

            var title = UITemplateNode.TextNode("Title", "App Title", "heading3");
            title.ColorName = "textPrimary";
            root.AddChild(title);

            var spacer = UITemplateNode.SpacerNode("CenterSpacer");
            root.AddChild(spacer);

            var actionBtn = UITemplateNode.ButtonNode("ActionButton", "Action", "Primary");
            actionBtn.Size = new Vector2(100, 40);
            root.AddChild(actionBtn);

            var menuBtn = UITemplateNode.ButtonNode("MenuButton", "☰", "IconButton");
            menuBtn.Size = new Vector2(40, 40);
            root.AddChild(menuBtn);

            return template;
        }

        public static UITemplate CreateListItemTemplate()
        {
            var template = ScriptableObject.CreateInstance<UITemplate>();
            template.Initialize("List Item", "Icon + Title + Subtitle + Chevron", "Lists");
            template.Tags = new List<string> { "list", "item", "row" };

            var root = template.Root;
            root.ElementType = UIElementType.HorizontalLayout;
            root.Padding = 12;
            root.SpacingSize = SpacingSize.MD;
            root.PreferredHeight = 72;
            root.Stretch = false;

            var icon = UITemplateNode.ImageNode("Icon", "elevatedSurface");
            icon.Size = new Vector2(48, 48);
            root.AddChild(icon);

            var textCol = UITemplateNode.VLayout("TextColumn", "Text");
            textCol.SpacingSize = SpacingSize.XS;
            textCol.FlexibleWidth = 1;
            root.AddChild(textCol);

            var title = UITemplateNode.TextNode("Title", "Item Title", "body");
            title.ColorName = "textPrimary";
            title.Bold = true;
            textCol.AddChild(title);

            var subtitle = UITemplateNode.TextNode("Subtitle", "Item description", "bodySmall");
            subtitle.ColorName = "textSecondary";
            textCol.AddChild(subtitle);

            var chevron = UITemplateNode.TextNode("Chevron", "›", "heading3");
            chevron.ColorName = "textTertiary";
            root.AddChild(chevron);

            return template;
        }

        public static UITemplate CreateErrorMessageTemplate()
        {
            var template = ScriptableObject.CreateInstance<UITemplate>();
            template.Initialize("Error Message", "Error bubble with icon and text", "Feedback");
            template.Tags = new List<string> { "error", "message", "alert" };

            var root = template.Root;
            root.ElementType = UIElementType.HorizontalLayout;
            root.BackgroundColorName = "errorBubble";
            root.Padding = 12;
            root.SpacingSize = SpacingSize.SM;
            root.Stretch = false;

            var icon = UITemplateNode.TextNode("Icon", "⚠", "heading3");
            icon.ColorName = "errorText";
            root.AddChild(icon);

            var text = UITemplateNode.TextNode("ErrorText", "An error occurred.", "body");
            text.ColorName = "errorText";
            text.FlexibleWidth = 1;
            root.AddChild(text);

            return template;
        }

        public static List<UITemplate> GetAllBuiltInTemplates()
        {
            return new List<UITemplate>
            {
                CreateModalTemplate(),
                CreateMessageBubbleTemplate(),
                CreateUserMessageBubbleTemplate(),
                CreateCardTemplate(),
                CreateInputGroupTemplate(),
                CreateToolbarTemplate(),
                CreateListItemTemplate(),
                CreateErrorMessageTemplate()
            };
        }

        [MenuItem("Tools/UI Prefab Builder/Generate Built-in Templates")]
        public static void GenerateBuiltInTemplates()
        {
            if (!System.IO.Directory.Exists(TemplatesFolder))
                System.IO.Directory.CreateDirectory(TemplatesFolder);

            var templates = new Dictionary<string, UITemplate>
            {
                { "ModalTemplate", CreateModalTemplate() },
                { "MessageBubbleTemplate", CreateMessageBubbleTemplate() },
                { "UserMessageBubbleTemplate", CreateUserMessageBubbleTemplate() },
                { "CardTemplate", CreateCardTemplate() },
                { "InputGroupTemplate", CreateInputGroupTemplate() },
                { "ToolbarTemplate", CreateToolbarTemplate() },
                { "ListItemTemplate", CreateListItemTemplate() },
                { "ErrorMessageTemplate", CreateErrorMessageTemplate() }
            };

            foreach (var kvp in templates)
            {
                var path = $"{TemplatesFolder}/{kvp.Key}.asset";
                AssetDatabase.CreateAsset(kvp.Value, path);
                Debug.Log($"[TemplateLibrary] Created: {path}");
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log($"[TemplateLibrary] Generated {templates.Count} built-in templates!");
        }
    }
}
#endif
