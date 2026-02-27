#if UNITY_EDITOR
using Core.Configs.DesignSystem;
using UnityEditor;
using UnityEngine;

namespace Core.Editor.PrefabBuilderSystem.Theming
{
    public static class DesignSystemProvider
    {
        private static DesignSystemSettings _settings;
        private static ColorPalette _colors;
        private static TypographySettings _typography;
        private static SpacingSettings _spacing;

        private static readonly string[] SearchPaths =
        {
            "Assets/Source/00. Configs/DesignSystemSettings.asset",
            "Assets/Resources/Configs/DesignSystemSettings.asset",
            "Assets/Configs/DesignSystemSettings.asset"
        };

        public static DesignSystemSettings Settings
        {
            get
            {
                if (_settings == null)
                    LoadSettings();
                return _settings;
            }
        }

        public static ColorPalette Colors
        {
            get
            {
                if (_colors == null && Settings != null)
                    _colors = Settings.colors;
                return _colors;
            }
        }

        public static TypographySettings Typography
        {
            get
            {
                if (_typography == null && Settings != null)
                    _typography = Settings.typography;
                return _typography;
            }
        }

        public static SpacingSettings Spacing
        {
            get
            {
                if (_spacing == null && Settings != null)
                    _spacing = Settings.spacing;
                return _spacing;
            }
        }

        public static bool AutoApply { get; set; } = true;

        public static bool IsLoaded => _settings != null;

        public static void Reload()
        {
            _settings = null;
            _colors = null;
            _typography = null;
            _spacing = null;
            LoadSettings();
        }

        private static void LoadSettings()
        {
            foreach (var path in SearchPaths)
            {
                _settings = AssetDatabase.LoadAssetAtPath<DesignSystemSettings>(path);
                if (_settings != null)
                    return;
            }

            var guids = AssetDatabase.FindAssets("t:DesignSystemSettings");
            if (guids.Length > 0)
            {
                var path = AssetDatabase.GUIDToAssetPath(guids[0]);
                _settings = AssetDatabase.LoadAssetAtPath<DesignSystemSettings>(path);
            }

            if (_settings == null)
            {
                Debug.LogWarning("[DesignSystemProvider] DesignSystemSettings not found! Using fallback values.");
            }
        }

        public static float GetSpacing(SpacingSize size)
        {
            if (Spacing == null)
                return GetDefaultSpacing(size);

            return size switch
            {
                SpacingSize.XS => Spacing.xs,
                SpacingSize.SM => Spacing.sm,
                SpacingSize.MD => Spacing.md,
                SpacingSize.LG => Spacing.lg,
                SpacingSize.XL => Spacing.xl,
                SpacingSize.XXL => Spacing.xxl,
                SpacingSize.XXXL => Spacing.xxxl,
                _ => Spacing.md
            };
        }

        private static float GetDefaultSpacing(SpacingSize size)
        {
            return size switch
            {
                SpacingSize.XS => 4f,
                SpacingSize.SM => 8f,
                SpacingSize.MD => 16f,
                SpacingSize.LG => 24f,
                SpacingSize.XL => 32f,
                SpacingSize.XXL => 48f,
                SpacingSize.XXXL => 64f,
                _ => 16f
            };
        }

        public static Color GetColor(string colorName)
        {
            if (Colors == null)
                return Color.white;

            return colorName switch
            {
                "deepSpace" => Colors.deepSpace,
                "darkSurface" => Colors.darkSurface,
                "elevatedSurface" => Colors.elevatedSurface,
                "indigo" => Colors.indigo,
                "pink" => Colors.pink,
                "purple" => Colors.purple,
                "cyan" => Colors.cyan,
                "textPrimary" => Colors.textPrimary,
                "textSecondary" => Colors.textSecondary,
                "textTertiary" => Colors.textTertiary,
                "textDisabled" => Colors.textDisabled,
                "cardBackground" => Colors.cardBackground,
                "cardBorder" => Colors.cardBorder,
                "inputBackground" => Colors.inputBackground,
                "inputBorder" => Colors.inputBorder,
                "inputFocusBorder" => Colors.inputFocusBorder,
                "inputPlaceholder" => Colors.inputPlaceholder,
                "primaryButtonStart" => Colors.primaryButtonGradientStart,
                "primaryButtonEnd" => Colors.primaryButtonGradientEnd,
                "secondaryButton" => Colors.secondaryButtonBackground,
                "iconButton" => Colors.iconButtonBackground,
                "userBubbleStart" => Colors.userBubbleStart,
                "userBubbleEnd" => Colors.userBubbleEnd,
                "aiBubble" => Colors.aiBubble,
                "errorBubble" => Colors.errorBubble,
                "errorText" => Colors.errorText,
                _ => Color.white
            };
        }

        public static TypographySettings.TextStyle GetTextStyle(string styleName)
        {
            if (Typography == null)
                return null;

            return styleName switch
            {
                "heading1" => Typography.heading1,
                "heading2" => Typography.heading2,
                "heading3" => Typography.heading3,
                "bodyLarge" => Typography.bodyLarge,
                "body" => Typography.body,
                "bodySmall" => Typography.bodySmall,
                "caption" => Typography.caption,
                "button" => Typography.button,
                _ => Typography.body
            };
        }
    }
}
#endif
