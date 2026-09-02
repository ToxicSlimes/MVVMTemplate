#if UNITY_EDITOR
using System.IO;
using Core.Infrastructure.MVVM.Config;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Core.Editor.UIToolkit
{
    public static class ToolkitViewSetupUtility
    {
        public const string PanelSettingsPath = "Assets/Source/00. Configs/UI/DefaultPanelSettings.asset";
        public const string DesignTokensPath = "Assets/Source/UI/Toolkit/Theme/design-tokens.uss";

        public static void EnsureDirectory(string path)
        {
            if (string.IsNullOrEmpty(path) || AssetDatabase.IsValidFolder(path))
                return;

            var parts = path.Split('/');
            var current = parts[0];
            for (int i = 1; i < parts.Length; i++)
            {
                var next = $"{current}/{parts[i]}";
                if (!AssetDatabase.IsValidFolder(next))
                    AssetDatabase.CreateFolder(current, parts[i]);
                current = next;
            }
        }

        public static void EnsurePanelSettings()
        {
            EnsureDirectory("Assets/Source/00. Configs/UI");
            var settings = AssetDatabase.LoadAssetAtPath<PanelSettings>(PanelSettingsPath);
            if (settings != null)
                return;

            settings = ScriptableObject.CreateInstance<PanelSettings>();
            settings.scaleMode = PanelScaleMode.ScaleWithScreenSize;
            settings.referenceResolution = new Vector2Int(1920, 1080);
            settings.screenMatchMode = PanelScreenMatchMode.MatchWidthOrHeight;
            settings.match = 0.5f;
            AssetDatabase.CreateAsset(settings, PanelSettingsPath);
        }

        public static void EnsureToolkitHostPrefab<TView>(
            string prefabPath,
            string uxmlPath,
            string screenStylesPath,
            bool isPersistent = false)
            where TView : MonoBehaviour
        {
            var uxml = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(uxmlPath);
            var panelSettings = AssetDatabase.LoadAssetAtPath<PanelSettings>(PanelSettingsPath);
            var tokens = AssetDatabase.LoadAssetAtPath<StyleSheet>(DesignTokensPath);
            var screenStyles = AssetDatabase.LoadAssetAtPath<StyleSheet>(screenStylesPath);

            var existing = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            if (existing != null)
            {
                var view = existing.GetComponent<TView>();
                var document = existing.GetComponent<UIDocument>();
                if (document != null)
                {
                    document.panelSettings = panelSettings;
                    document.visualTreeAsset = uxml;
                }

                if (view != null)
                {
                    var serializedView = new SerializedObject(view);
                    serializedView.FindProperty("_layout").objectReferenceValue = uxml;
                    serializedView.FindProperty("_isPersistent").boolValue = isPersistent;

                    var styleSheets = serializedView.FindProperty("_styleSheets");
                    styleSheets.arraySize = 2;
                    styleSheets.GetArrayElementAtIndex(0).objectReferenceValue = tokens;
                    styleSheets.GetArrayElementAtIndex(1).objectReferenceValue = screenStyles;
                    serializedView.ApplyModifiedPropertiesWithoutUndo();
                    EditorUtility.SetDirty(view);
                }

                EditorUtility.SetDirty(existing);
                return;
            }

            var go = new GameObject(Path.GetFileNameWithoutExtension(prefabPath));
            var newDocument = go.AddComponent<UIDocument>();
            newDocument.panelSettings = panelSettings;
            newDocument.visualTreeAsset = uxml;
            var newView = go.AddComponent<TView>();
            var serializedNewView = new SerializedObject(newView);
            serializedNewView.FindProperty("_layout").objectReferenceValue = uxml;
            serializedNewView.FindProperty("_isPersistent").boolValue = isPersistent;

            var newStyleSheets = serializedNewView.FindProperty("_styleSheets");
            newStyleSheets.arraySize = 2;
            newStyleSheets.GetArrayElementAtIndex(0).objectReferenceValue = tokens;
            newStyleSheets.GetArrayElementAtIndex(1).objectReferenceValue = screenStyles;
            serializedNewView.ApplyModifiedPropertiesWithoutUndo();

            PrefabUtility.SaveAsPrefabAsset(go, prefabPath);
            Object.DestroyImmediate(go);
        }

        public static void UpdateViewMappingPrefab(string mappingPath, string prefabPath)
        {
            var mapping = AssetDatabase.LoadAssetAtPath<ViewMappingBase>(mappingPath);
            if (mapping == null)
                return;

            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            if (prefab == null)
                return;

            mapping.ViewPrefab = new UnityEngine.AddressableAssets.AssetReferenceT<GameObject>(
                AssetDatabase.AssetPathToGUID(prefabPath));
            EditorUtility.SetDirty(mapping);
            EnsureViewPrefabAddressable(prefabPath);
        }

        private static void EnsureViewPrefabAddressable(string prefabPath)
        {
            var settings = UnityEditor.AddressableAssets.AddressableAssetSettingsDefaultObject.Settings;
            if (settings == null)
                return;

            var guid = AssetDatabase.AssetPathToGUID(prefabPath);
            if (string.IsNullOrEmpty(guid))
                return;

            var group = settings.FindGroup("Views") ?? settings.DefaultGroup;
            if (settings.FindAssetEntry(guid) == null)
                settings.CreateOrMoveEntry(guid, group);
        }
    }
}
#endif
