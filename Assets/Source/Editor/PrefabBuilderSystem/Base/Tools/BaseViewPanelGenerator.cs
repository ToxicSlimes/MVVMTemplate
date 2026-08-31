#if UNITY_EDITOR
using Core.Editor.PrefabBuilderSystem.Elements;
using Core.Editor.PrefabBuilderSystem.Theming;
using Core.Infrastructure.MVVM.Animation;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace Core.Editor.PrefabBuilderSystem.Tools
{
    /// <summary>
    /// Builds the prefab every view prefab derives from. Rebuilding it propagates canvas,
    /// scaler and background changes to all view variants at once.
    /// </summary>
    public static class BaseViewPanelGenerator
    {
        public const string BaseViewPanelPath = "Assets/Prefabs/Base/BaseViewPanel.prefab";

        private const string BackgroundName = "Background";
        private const string RootName = "BaseViewPanel";

        private static readonly Vector2 ReferenceResolution = new(1920, 1080);
        private static readonly Color DeepSpaceFallback = new(0.024f, 0.043f, 0.102f, 1f);

        [MenuItem("Tools/UI Prefab Builder/Build Base View Panel", false, 1)]
        public static void BuildBaseViewPanelMenu()
        {
            var path = BuildBaseViewPanel();

            EditorUtility.DisplayDialog(
                "Base View Panel",
                $"Created:\n{path}\n\nRebuild the views so they pick up the change.",
                "OK");
        }

        public static string BuildBaseViewPanel()
        {
            EnsureDirectory();

            var root = new GameObject(RootName, typeof(RectTransform));
            StretchToParent(root);

            var canvas = root.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;

            var scaler = root.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = ReferenceResolution;
            scaler.matchWidthOrHeight = 0.5f;

            root.AddComponent<GraphicRaycaster>();
            root.AddComponent<CanvasGroup>();
            root.AddComponent<ViewAnimationOrchestrator>();

            new ImageBuilder(BackgroundName)
                .WithColor(DesignSystemProvider.Colors?.deepSpace ?? DeepSpaceFallback)
                .NoRaycast()
                .WithStretch()
                .Build(root.transform);

            PrefabUtility.SaveAsPrefabAsset(root, BaseViewPanelPath);
            Object.DestroyImmediate(root);

            AssetDatabase.SaveAssets();
            Debug.Log($"<color=green>✓ Base view panel created: {BaseViewPanelPath}</color>");

            return BaseViewPanelPath;
        }

        /// <summary>Returns the base prefab, building it on first use.</summary>
        public static GameObject LoadOrBuild()
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(BaseViewPanelPath);

            if (prefab != null)
                return prefab;

            BuildBaseViewPanel();
            return AssetDatabase.LoadAssetAtPath<GameObject>(BaseViewPanelPath);
        }

        private static void EnsureDirectory()
        {
            if (!AssetDatabase.IsValidFolder("Assets/Prefabs/Base"))
            {
                AssetDatabase.CreateFolder("Assets/Prefabs", "Base");
            }
        }

        private static void StretchToParent(GameObject go)
        {
            var rect = go.GetComponent<RectTransform>();
            rect.localScale = Vector3.one;
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            rect.pivot = new Vector2(0.5f, 0.5f);
        }
    }
}
#endif
