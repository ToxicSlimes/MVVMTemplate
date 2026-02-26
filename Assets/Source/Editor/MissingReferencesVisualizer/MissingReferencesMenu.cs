#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace MissingReferencesVisualizer
{
    public static class MissingReferencesMenu
    {
        private const string MENU_BASE = "Tools/Missing References/";
        private const string ENABLE_KEY = "MissingRefsVisualizer_Enabled";

        [MenuItem(MENU_BASE + "Scan All Scenes")]
        public static void ScanAllScenes()
        {
            MissingReferencesScanner.ClearCache();
            var missingRefs = MissingReferencesScanner.ScanAllScenes();
            
            if (missingRefs.Count == 0)
            {
                Debug.Log("<color=green>[Missing References]</color> No missing references found in all open scenes.");
            }
            else
            {
                Debug.LogWarning($"<color=red>[Missing References]</color> Found {missingRefs.Count} missing reference(s) in all open scenes:");
                foreach (var refInfo in missingRefs)
                {
                    Debug.LogError($"[Missing Ref] {refInfo}", refInfo.gameObject);
                }
            }

            EditorApplication.RepaintHierarchyWindow();
            SceneView.RepaintAll();
        }

        [MenuItem(MENU_BASE + "Toggle Visualization", false, 1)]
        public static void ToggleVisualization()
        {
            bool currentState = EditorPrefs.GetBool(ENABLE_KEY, true);
            bool newState = !currentState;
            
            EditorPrefs.SetBool(ENABLE_KEY, newState);
            HierarchyMissingRefsIndicator.SetEnabled(newState);
            SceneMissingRefsVisualizer.SetEnabled(newState);
            
            Debug.Log($"<color=cyan>[Missing References]</color> Visualization {(newState ? "enabled" : "disabled")}.");
        }

        [MenuItem(MENU_BASE + "Toggle Visualization", true)]
        public static bool ToggleVisualizationValidate()
        {
            bool isEnabled = EditorPrefs.GetBool(ENABLE_KEY, true);
            Menu.SetChecked(MENU_BASE + "Toggle Visualization", isEnabled);
            return true;
        }

        [MenuItem(MENU_BASE + "Clear Cache")]
        public static void ClearCache()
        {
            MissingReferencesScanner.ClearCache();
            Debug.Log("<color=cyan>[Missing References]</color> Cache cleared.");
            EditorApplication.RepaintHierarchyWindow();
            SceneView.RepaintAll();
        }

        [MenuItem(MENU_BASE + "Toggle Auto Scan", false, 2)]
        public static void ToggleAutoScan()
        {
            bool currentState = MissingReferencesAutoScanner.IsEnabled();
            bool newState = !currentState;
            
            MissingReferencesAutoScanner.SetEnabled(newState);
            Debug.Log($"<color=cyan>[Missing References]</color> Auto scan {(newState ? "enabled" : "disabled")}.");
        }

        [MenuItem(MENU_BASE + "Toggle Auto Scan", true)]
        public static bool ToggleAutoScanValidate()
        {
            bool isEnabled = MissingReferencesAutoScanner.IsEnabled();
            Menu.SetChecked(MENU_BASE + "Toggle Auto Scan", isEnabled);
            return true;
        }

        [MenuItem(MENU_BASE + "Toggle Inspector Highlight", false, 3)]
        public static void ToggleInspectorHighlight()
        {
            bool currentState = InspectorMissingRefsHighlighter.IsEnabled();
            bool newState = !currentState;
            
            InspectorMissingRefsHighlighter.SetEnabled(newState);
            Debug.Log($"<color=cyan>[Missing References]</color> Inspector highlight {(newState ? "enabled" : "disabled")}.");
        }

        [MenuItem(MENU_BASE + "Toggle Inspector Highlight", true)]
        public static bool ToggleInspectorHighlightValidate()
        {
            bool isEnabled = InspectorMissingRefsHighlighter.IsEnabled();
            Menu.SetChecked(MENU_BASE + "Toggle Inspector Highlight", isEnabled);
            return true;
        }
    }
}
#endif

