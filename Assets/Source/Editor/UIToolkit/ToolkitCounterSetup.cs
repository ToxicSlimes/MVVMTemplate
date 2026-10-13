#if UNITY_EDITOR && !UNITY_6000_3_OR_NEWER

using System.IO;

using Core.Presentation.MVVM.Config;
using Core.Presentation.MVVM.Config.Generated;
using Core.Presentation.MVVM.Views.Counter;
using UnityEditor;

using UnityEditor.SceneManagement;

using UnityEngine;

using UnityEngine.UIElements;



namespace Core.Editor.UIToolkit

{

    public static class ToolkitCounterSetup

    {

        private const string CounterUxmlPath = "Assets/Source/UI/Toolkit/Counter/CounterScreen.uxml";

        private const string CounterStylesPath = "Assets/Source/UI/Toolkit/Counter/CounterScreen.uss";

        private const string PrefabPath = "Assets/Prefabs/Views/CounterToolkitHost.prefab";

        private const string EditScenePath = "Assets/Source/Assets/Scenes/UI/ToolkitCounterEdit.unity";

        private const string ToolkitElementConfigPath = "Assets/Source/00. Configs/ToolkitElementConfig.asset";

        private const string CounterMappingPath = "Assets/Source/00. Configs/ViewsMapping/CounterViewMapping.asset";



        [MenuItem("Tools/UI Toolkit/Open Counter Edit Scene")]

        public static void OpenCounterEditScene()

        {

            EnsureAssets();

            if (!File.Exists(EditScenePath))

                CreateEditScene();



            EditorSceneManager.OpenScene(EditScenePath);

        }



        [MenuItem("Tools/UI Toolkit/Setup Counter Toolkit Assets")]

        public static void EnsureAssets()

        {

            ToolkitViewSetupUtility.EnsureDirectory("Assets/Source/Assets/Scenes/UI");

            ToolkitViewSetupUtility.EnsurePanelSettings();

            ToolkitViewSetupUtility.EnsureToolkitHostPrefab<CounterToolkitView>(

                PrefabPath,

                CounterUxmlPath,

                CounterStylesPath);

            EnsureToolkitElementConfig();

            ToolkitViewSetupUtility.UpdateViewMappingPrefab(CounterMappingPath, PrefabPath);

            AssetDatabase.SaveAssets();

            AssetDatabase.Refresh();

            Debug.Log("[Toolkit] Counter toolkit assets ensured.");

        }



        public static void EnsureAllToolkitViews() => ToolkitViewsSetup.EnsureAll();

        public static void EnsureToolkitElementConfig()

        {

            var config = AssetDatabase.LoadAssetAtPath<ToolkitElementConfig>(ToolkitElementConfigPath);

            if (config == null)

            {

                config = ScriptableObject.CreateInstance<ToolkitElementConfig>();

                AssetDatabase.CreateAsset(config, ToolkitElementConfigPath);

            }



            AddToolkitMappingIfMissing<TextCardToolkitElementMapping>(config, "Assets/Source/UI/Toolkit/Elements/TextCard.uxml");

            AddToolkitMappingIfMissing<StatBarToolkitElementMapping>(config, "Assets/Source/UI/Toolkit/Elements/StatBar.uxml");

            AddToolkitMappingIfMissing<ActionListItemToolkitElementMapping>(config, "Assets/Source/UI/Toolkit/Elements/ActionListItem.uxml");

            EnsureAddressable("Assets/Source/UI/Toolkit/Elements/TextCard.uxml");

            EnsureAddressable("Assets/Source/UI/Toolkit/Elements/StatBar.uxml");

            EnsureAddressable("Assets/Source/UI/Toolkit/Elements/ActionListItem.uxml");

            EditorUtility.SetDirty(config);

        }



        private static void EnsureAddressable(string uxmlPath)

        {

            var settings = UnityEditor.AddressableAssets.AddressableAssetSettingsDefaultObject.Settings;

            if (settings == null) return;



            var guid = AssetDatabase.AssetPathToGUID(uxmlPath);

            if (string.IsNullOrEmpty(guid)) return;



            var group = settings.FindGroup("Elements") ?? settings.DefaultGroup;

            if (settings.FindAssetEntry(guid) == null)

                settings.CreateOrMoveEntry(guid, group);

        }



        private static void AddToolkitMappingIfMissing<TMapping>(ToolkitElementConfig config, string uxmlPath)

            where TMapping : ToolkitElementMappingBase

        {

            var serializedConfig = new SerializedObject(config);

            var mappings = serializedConfig.FindProperty("_mappings");



            for (int i = 0; i < mappings.arraySize; i++)

            {

                if (mappings.GetArrayElementAtIndex(i).objectReferenceValue is TMapping)

                    return;

            }



            var mappingAsset = ScriptableObject.CreateInstance<TMapping>();

            mappingAsset.ElementTemplate = new UnityEngine.AddressableAssets.AssetReferenceT<VisualTreeAsset>(

                AssetDatabase.AssetPathToGUID(uxmlPath));

            mappingAsset.name = typeof(TMapping).Name;



            var mappingPath = $"Assets/Source/00. Configs/ToolkitElements/{typeof(TMapping).Name}.asset";

            ToolkitViewSetupUtility.EnsureDirectory(Path.GetDirectoryName(mappingPath)?.Replace('\\', '/'));

            AssetDatabase.CreateAsset(mappingAsset, mappingPath);



            mappings.InsertArrayElementAtIndex(mappings.arraySize);

            mappings.GetArrayElementAtIndex(mappings.arraySize - 1).objectReferenceValue = mappingAsset;

            serializedConfig.ApplyModifiedPropertiesWithoutUndo();

        }



        private static void CreateEditScene()

        {

            EnsureAssets();

            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            var go = new GameObject("CounterToolkitEdit");

            var document = go.AddComponent<UIDocument>();

            document.panelSettings = AssetDatabase.LoadAssetAtPath<PanelSettings>(ToolkitViewSetupUtility.PanelSettingsPath);

            document.visualTreeAsset = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(CounterUxmlPath);

            EditorSceneManager.SaveScene(scene, EditScenePath);

        }

    }

}

#endif


