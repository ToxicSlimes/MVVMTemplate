#if UNITY_EDITOR
using Core.Presentation.MVVM.Views.ConnectingOverlay;
using Core.Presentation.MVVM.Views.Counter;
using Core.Presentation.MVVM.Views.Inventory;
using Core.Presentation.MVVM.Views.LobbyRoom;
using Core.Presentation.MVVM.Views.MainMenu;
using Core.Presentation.MVVM.Views.Matchmaking;
using Core.Presentation.MVVM.Views.Pause;
using Core.Presentation.MVVM.Views.Settings;
using UnityEditor;
using UnityEngine;

namespace Core.Editor.UIToolkit
{
    public static class ToolkitViewsSetup
    {
        [MenuItem("Tools/UI Toolkit/Setup All Toolkit Views")]
        public static void EnsureAll()
        {
            ToolkitViewSetupUtility.EnsureDirectory("Assets/Source/Assets/Scenes/UI");
            ToolkitViewSetupUtility.EnsurePanelSettings();

            EnsureView<CounterToolkitView>(
                "Assets/Prefabs/Views/CounterToolkitHost.prefab",
                "Assets/Source/UI/Toolkit/Counter/CounterScreen.uxml",
                "Assets/Source/UI/Toolkit/Counter/CounterScreen.uss",
                "Assets/Source/00. Configs/ViewsMapping/CounterViewMapping.asset");

            EnsureView<MainMenuToolkitView>(
                "Assets/Prefabs/Views/MainMenuToolkitHost.prefab",
                "Assets/Source/UI/Toolkit/MainMenu/MainMenuScreen.uxml",
                "Assets/Source/UI/Toolkit/MainMenu/MainMenuScreen.uss",
                "Assets/Source/00. Configs/ViewsMapping/MainMenuViewMapping.asset");

            EnsureView<SettingsToolkitView>(
                "Assets/Prefabs/Views/SettingsToolkitHost.prefab",
                "Assets/Source/UI/Toolkit/Settings/SettingsScreen.uxml",
                "Assets/Source/UI/Toolkit/Settings/SettingsScreen.uss",
                "Assets/Source/00. Configs/ViewsMapping/SettingsViewMapping.asset",
                isPersistent: true);

            EnsureView<InventoryToolkitView>(
                "Assets/Prefabs/Views/InventoryToolkitHost.prefab",
                "Assets/Source/UI/Toolkit/Inventory/InventoryScreen.uxml",
                "Assets/Source/UI/Toolkit/Inventory/InventoryScreen.uss",
                "Assets/Source/00. Configs/ViewsMapping/InventoryViewMapping.asset");

            EnsureView<MatchmakingToolkitView>(
                "Assets/Prefabs/Views/MatchmakingToolkitHost.prefab",
                "Assets/Source/UI/Toolkit/Matchmaking/MatchmakingScreen.uxml",
                "Assets/Source/UI/Toolkit/Matchmaking/MatchmakingScreen.uss",
                "Assets/Source/00. Configs/ViewsMapping/MatchmakingViewMapping.asset");

            EnsureView<LobbyRoomToolkitView>(
                "Assets/Prefabs/Views/LobbyRoomToolkitHost.prefab",
                "Assets/Source/UI/Toolkit/LobbyRoom/LobbyRoomScreen.uxml",
                "Assets/Source/UI/Toolkit/LobbyRoom/LobbyRoomScreen.uss",
                "Assets/Source/00. Configs/ViewsMapping/LobbyRoomViewMapping.asset");

            EnsureView<PauseToolkitView>(
                "Assets/Prefabs/Views/PauseToolkitHost.prefab",
                "Assets/Source/UI/Toolkit/Pause/PauseScreen.uxml",
                "Assets/Source/UI/Toolkit/Pause/PauseScreen.uss",
                "Assets/Source/00. Configs/ViewsMapping/PauseViewMapping.asset");

            EnsureView<ConnectingOverlayToolkitView>(
                "Assets/Prefabs/Views/ConnectingOverlayToolkitHost.prefab",
                "Assets/Source/UI/Toolkit/ConnectingOverlay/ConnectingOverlayScreen.uxml",
                "Assets/Source/UI/Toolkit/ConnectingOverlay/ConnectingOverlayScreen.uss",
                "Assets/Source/00. Configs/ViewsMapping/ConnectingOverlayViewMapping.asset");

            ToolkitCounterSetup.EnsureToolkitElementConfig();

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            var report = ToolkitBindingValidator.ValidateAll();
            report.LogToConsole();

            if (report.IsValid)
                Debug.Log("[Toolkit] All toolkit views set up and bindings validated.");
            else
                Debug.LogWarning("[Toolkit] Setup finished with binding issues.");
        }

        private static void EnsureView<TView>(
            string prefabPath,
            string uxmlPath,
            string stylesPath,
            string mappingPath,
            bool isPersistent = false)
            where TView : MonoBehaviour
        {
            ToolkitViewSetupUtility.EnsureToolkitHostPrefab<TView>(
                prefabPath,
                uxmlPath,
                stylesPath,
                isPersistent);
            ToolkitViewSetupUtility.UpdateViewMappingPrefab(mappingPath, prefabPath);
        }
    }
}
#endif
