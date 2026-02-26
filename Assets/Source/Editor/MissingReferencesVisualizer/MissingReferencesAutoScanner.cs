#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace MissingReferencesVisualizer
{
    [InitializeOnLoad]
    public static class MissingReferencesAutoScanner
    {
        private static bool isEnabled = true;
        private const string ENABLE_KEY = "MissingRefsVisualizer_AutoScan";

        static MissingReferencesAutoScanner()
        {
            isEnabled = EditorPrefs.GetBool(ENABLE_KEY, true);

            if (isEnabled)
            {
                EditorApplication.hierarchyChanged += OnHierarchyChanged;
                EditorSceneManager.sceneOpened += OnSceneOpened;
                EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
            }
        }

        private static void OnHierarchyChanged()
        {
            if (!isEnabled) return;
            if (Application.isPlaying) return;

            MissingReferencesScanner.ClearCache();
            EditorApplication.RepaintHierarchyWindow();
            SceneView.RepaintAll();
        }

        private static void OnSceneOpened(Scene scene, OpenSceneMode mode)
        {
            if (!isEnabled) return;

            MissingReferencesScanner.ClearCache();
            var missingRefs = MissingReferencesScanner.ScanAllScenes();

            if (missingRefs.Count > 0)
            {
                Debug.LogWarning($"<color=red>[Missing References]</color> Found {missingRefs.Count} missing reference(s) in all open scenes");
            }

            EditorApplication.RepaintHierarchyWindow();
            SceneView.RepaintAll();
        }

        private static void OnPlayModeStateChanged(PlayModeStateChange state)
        {
            if (!isEnabled) return;

            if (state == PlayModeStateChange.EnteredEditMode)
            {
                MissingReferencesScanner.ClearCache();
                EditorApplication.RepaintHierarchyWindow();
                SceneView.RepaintAll();
            }
        }

        public static void SetEnabled(bool enabled)
        {
            isEnabled = enabled;
            EditorPrefs.SetBool(ENABLE_KEY, enabled);

            if (enabled)
            {
                EditorApplication.hierarchyChanged += OnHierarchyChanged;
                EditorSceneManager.sceneOpened += OnSceneOpened;
                EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
            }
            else
            {
                EditorApplication.hierarchyChanged -= OnHierarchyChanged;
                EditorSceneManager.sceneOpened -= OnSceneOpened;
                EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
            }
        }

        public static bool IsEnabled()
        {
            return isEnabled;
        }
    }
}
#endif

