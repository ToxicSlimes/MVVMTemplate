using UnityEngine;
using UnityEditor;
using TMPro;
using System.IO;
using Core.Presentation.UI.DesignSystem;

namespace Core.Editor.DesignSystem
{
    public class CreateDesignSystemAssets : EditorWindow
    {
        [MenuItem("Tools/Design System/Create Design System Assets")]
        public static void CreateAssets()
        {
            const string resourcesPath = "Assets/Resources";

            if (!AssetDatabase.IsValidFolder(resourcesPath))
            {
                AssetDatabase.CreateFolder("Assets", "Resources");
            }

            var colorPalette = CreateColorPalette(resourcesPath);
            var typography = CreateTypographySettings(resourcesPath);
            var spacing = CreateSpacingSettings(resourcesPath);
            var designSystem = CreateDesignSystemSettings(resourcesPath, colorPalette, typography, spacing);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            EditorUtility.DisplayDialog(
                "Design System Created",
                "Design System assets created successfully in Assets/Resources/\n\n" +
                "- ColorPalette.asset\n" +
                "- TypographySettings.asset\n" +
                "- SpacingSettings.asset\n" +
                "- DesignSystemSettings.asset\n\n" +
                "All values are filled according to design-guideline.md",
                "OK"
            );

            Selection.activeObject = designSystem;
            EditorGUIUtility.PingObject(designSystem);
        }

        private static ColorPalette CreateColorPalette(string path)
        {
            var assetPath = $"{path}/ColorPalette.asset";

            var existing = AssetDatabase.LoadAssetAtPath<ColorPalette>(assetPath);
            if (existing != null)
            {
                Debug.Log($"ColorPalette already exists at {assetPath}, updating values...");
            }

            var palette = existing != null ? existing : ScriptableObject.CreateInstance<ColorPalette>();

            palette.deepSpace = new Color(0.024f, 0.043f, 0.102f, 1f);
            palette.darkSurface = new Color(0.051f, 0.090f, 0.188f, 1f);
            palette.elevatedSurface = new Color(0.086f, 0.149f, 0.290f, 0.8f);

            palette.indigo = new Color(0.184f, 0.816f, 0.451f, 1f);
            palette.pink = new Color(1f, 0.541f, 0.239f, 1f);
            palette.purple = new Color(1f, 0.827f, 0.306f, 1f);
            palette.cyan = new Color(0.478f, 0.925f, 0.863f, 1f);

            palette.textPrimary = new Color(0.973f, 0.980f, 0.988f, 1f);
            palette.textSecondary = new Color(0.639f, 0.706f, 0.800f, 1f);
            palette.textTertiary = new Color(0.482f, 0.553f, 0.659f, 1f);
            palette.textDisabled = new Color(0.369f, 0.439f, 0.549f, 1f);

            palette.userBubbleStart = new Color(0.059f, 0.361f, 0.259f, 0.95f);
            palette.userBubbleEnd = new Color(0.055f, 0.353f, 0.400f, 0.95f);
            palette.aiBubble = new Color(0.078f, 0.125f, 0.231f, 0.95f);
            palette.aiBubbleBorder = new Color(0.639f, 0.706f, 0.800f, 0.1f);
            palette.errorBubble = new Color(0.902f, 0.224f, 0.275f, 0.18f);
            palette.errorText = new Color(1f, 0.361f, 0.361f, 1f);
            palette.errorBorder = new Color(0.902f, 0.224f, 0.275f, 0.45f);

            palette.cardBackground = new Color(0.086f, 0.149f, 0.290f, 0.4f);
            palette.cardBorder = new Color(0.639f, 0.706f, 0.800f, 0.12f);
            palette.cardHoverBackground = new Color(0.086f, 0.149f, 0.290f, 0.6f);
            palette.cardHoverBorder = new Color(0.184f, 0.816f, 0.451f, 0.3f);

            palette.primaryButtonGradientStart = new Color(0.047f, 0.478f, 0.294f, 1f);
            palette.primaryButtonGradientEnd = new Color(0.043f, 0.431f, 0.455f, 1f);
            palette.secondaryButtonBackground = new Color(0.086f, 0.149f, 0.290f, 0.5f);
            palette.iconButtonBackground = new Color(0.086f, 0.149f, 0.290f, 0.65f);

            palette.inputBackground = new Color(0.078f, 0.125f, 0.231f, 0.65f);
            palette.inputBorder = new Color(0.639f, 0.706f, 0.800f, 0.12f);
            palette.inputFocusBorder = new Color(0.184f, 0.816f, 0.451f, 1f);
            palette.inputPlaceholder = new Color(0.482f, 0.553f, 0.659f, 1f);

            if (existing == null)
            {
                AssetDatabase.CreateAsset(palette, assetPath);
            }
            else
            {
                EditorUtility.SetDirty(palette);
            }

            Debug.Log($"ColorPalette created/updated at {assetPath}");
            return palette;
        }

        private static TypographySettings CreateTypographySettings(string path)
        {
            var assetPath = $"{path}/TypographySettings.asset";

            var existing = AssetDatabase.LoadAssetAtPath<TypographySettings>(assetPath);
            if (existing != null)
            {
                Debug.Log($"TypographySettings already exists at {assetPath}, updating values...");
            }

            var typography = existing != null ? existing : ScriptableObject.CreateInstance<TypographySettings>();

            typography.heading1 = new TypographySettings.TextStyle
            {
                fontSize = 48f,
                fontWeight = 700,
                lineHeight = 1.2f,
                letterSpacing = -0.02f,
                alignment = TextAlignmentOptions.Center
            };

            typography.heading2 = new TypographySettings.TextStyle
            {
                fontSize = 32f,
                fontWeight = 600,
                lineHeight = 1.3f,
                letterSpacing = -0.01f,
                alignment = TextAlignmentOptions.Left
            };

            typography.heading3 = new TypographySettings.TextStyle
            {
                fontSize = 24f,
                fontWeight = 600,
                lineHeight = 1.4f,
                letterSpacing = 0f,
                alignment = TextAlignmentOptions.Left
            };

            typography.bodyLarge = new TypographySettings.TextStyle
            {
                fontSize = 18f,
                fontWeight = 400,
                lineHeight = 1.6f,
                letterSpacing = 0f,
                alignment = TextAlignmentOptions.Left
            };

            typography.body = new TypographySettings.TextStyle
            {
                fontSize = 14f,
                fontWeight = 400,
                lineHeight = 1.6f,
                letterSpacing = 0f,
                alignment = TextAlignmentOptions.Left
            };

            typography.bodySmall = new TypographySettings.TextStyle
            {
                fontSize = 12f,
                fontWeight = 400,
                lineHeight = 1.5f,
                letterSpacing = 0f,
                alignment = TextAlignmentOptions.Left
            };

            typography.caption = new TypographySettings.TextStyle
            {
                fontSize = 12f,
                fontWeight = 500,
                lineHeight = 1.5f,
                letterSpacing = 0f,
                alignment = TextAlignmentOptions.Left
            };

            typography.button = new TypographySettings.TextStyle
            {
                fontSize = 14f,
                fontWeight = 600,
                lineHeight = 1.4f,
                letterSpacing = 0.01f,
                alignment = TextAlignmentOptions.Center
            };

            if (existing == null)
            {
                AssetDatabase.CreateAsset(typography, assetPath);
            }
            else
            {
                EditorUtility.SetDirty(typography);
            }

            Debug.Log($"TypographySettings created/updated at {assetPath}");
            return typography;
        }

        private static SpacingSettings CreateSpacingSettings(string path)
        {
            var assetPath = $"{path}/SpacingSettings.asset";

            var existing = AssetDatabase.LoadAssetAtPath<SpacingSettings>(assetPath);
            if (existing != null)
            {
                Debug.Log($"SpacingSettings already exists at {assetPath}, updating values...");
            }

            var spacing = existing != null ? existing : ScriptableObject.CreateInstance<SpacingSettings>();

            spacing.xs = 4f;
            spacing.sm = 8f;
            spacing.md = 16f;
            spacing.lg = 24f;
            spacing.xl = 32f;
            spacing.xxl = 48f;
            spacing.xxxl = 64f;

            spacing.cardPaddingDesktop = 32f;
            spacing.cardPaddingMobile = 20f;
            spacing.buttonPaddingHorizontal = 24f;
            spacing.buttonPaddingVertical = 12f;
            spacing.inputPaddingHorizontal = 16f;
            spacing.inputPaddingVertical = 12f;
            spacing.messagePaddingHorizontal = 16f;
            spacing.messagePaddingVertical = 12f;
            spacing.messageSpacing = 12f;

            spacing.borderRadiusSmall = 8f;
            spacing.borderRadiusMedium = 12f;
            spacing.borderRadiusLarge = 16f;
            spacing.borderRadiusXLarge = 24f;

            spacing.containerMaxWidth = 1920f;
            spacing.containerPaddingDesktop = 32f;
            spacing.containerPaddingMobile = 16f;
            spacing.gridGutter = 24f;

            if (existing == null)
            {
                AssetDatabase.CreateAsset(spacing, assetPath);
            }
            else
            {
                EditorUtility.SetDirty(spacing);
            }

            Debug.Log($"SpacingSettings created/updated at {assetPath}");
            return spacing;
        }

        private static DesignSystemSettings CreateDesignSystemSettings(
            string path,
            ColorPalette colors,
            TypographySettings typography,
            SpacingSettings spacing)
        {
            var assetPath = $"{path}/DesignSystemSettings.asset";

            var existing = AssetDatabase.LoadAssetAtPath<DesignSystemSettings>(assetPath);
            if (existing != null)
            {
                Debug.Log($"DesignSystemSettings already exists at {assetPath}, updating references...");
            }

            var settings = existing != null ? existing : ScriptableObject.CreateInstance<DesignSystemSettings>();

            settings.colors = colors;
            settings.typography = typography;
            settings.spacing = spacing;

            settings.durationFast = 0.15f;
            settings.durationMedium = 0.3f;
            settings.durationSlow = 0.5f;


            if (existing == null)
            {
                AssetDatabase.CreateAsset(settings, assetPath);
            }
            else
            {
                EditorUtility.SetDirty(settings);
            }

            Debug.Log($"DesignSystemSettings created/updated at {assetPath}");
            return settings;
        }
    }
}
