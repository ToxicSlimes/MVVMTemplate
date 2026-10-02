#if UNITY_EDITOR
using Core.Presentation.MVVM.Views.MainMenu;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Core.Editor.UIToolkit
{
    public static class ToolkitMainMenuSetup
    {
        private const string MainMenuUxmlPath = "Assets/Source/UI/Toolkit/MainMenu/MainMenuScreen.uxml";
        private const string MainMenuStylesPath = "Assets/Source/UI/Toolkit/MainMenu/MainMenuScreen.uss";
        private const string PrefabPath = "Assets/Prefabs/Views/MainMenuToolkitHost.prefab";
        private const string MappingPath = "Assets/Source/00. Configs/ViewsMapping/MainMenuViewMapping.asset";
        private const string EditScenePath = "Assets/Source/Assets/Scenes/UI/ToolkitMainMenuEdit.unity";

        [MenuItem("Tools/UI Toolkit/Open Main Menu Edit Scene")]
        public static void OpenMainMenuEditScene()
        {
            EnsureAssets();
            if (!System.IO.File.Exists(EditScenePath))
                CreateEditScene();

            EditorSceneManager.OpenScene(EditScenePath);
        }

        [MenuItem("Tools/UI Toolkit/Setup Main Menu Toolkit Assets")]
        public static void EnsureAssets()
        {
            ToolkitViewSetupUtility.EnsureDirectory("Assets/Source/UI/Toolkit/MainMenu");
            ToolkitViewSetupUtility.EnsureDirectory("Assets/Source/Assets/Scenes/UI");
            ToolkitViewSetupUtility.EnsurePanelSettings();
            ToolkitViewSetupUtility.EnsureToolkitHostPrefab<MainMenuToolkitView>(
                PrefabPath,
                MainMenuUxmlPath,
                MainMenuStylesPath);
            ToolkitViewSetupUtility.UpdateViewMappingPrefab(MappingPath, PrefabPath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("[Toolkit] Main menu toolkit assets ensured.");
        }
        
        private static void CreateEditScene()
        {
            EnsureAssets();
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            var go = new GameObject("MainMenuToolkitEdit");
            var document = go.AddComponent<UnityEngine.UIElements.UIDocument>();
            document.panelSettings = AssetDatabase.LoadAssetAtPath<UnityEngine.UIElements.PanelSettings>(ToolkitViewSetupUtility.PanelSettingsPath);
            document.visualTreeAsset = AssetDatabase.LoadAssetAtPath<UnityEngine.UIElements.VisualTreeAsset>(MainMenuUxmlPath);
            EditorSceneManager.SaveScene(scene, EditScenePath);
        }
    }
}
#endif
