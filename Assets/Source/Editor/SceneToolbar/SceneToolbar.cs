#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditor.Toolbars;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;

public static class SceneToolbar
{
    private const string KAllScenesToggle = "SceneToolbar/AllScenesToggle";
    private const string KSceneSwitcher = "SceneToolbar/SceneSwitcher";
    private const string KAdditiveSelector = "SceneToolbar/AdditiveSelector";
    private const string KAdditiveAddBtn = "SceneToolbar/AdditiveAdd";
    private const string KScenesManager = "SceneToolbar/ScenesManager";

    // ─────────────────────────────────────────────────────────
    // 1. "All Scenes" Toggle
    // ─────────────────────────────────────────────────────────

    [MainToolbarElement(KAllScenesToggle, defaultDockPosition = MainToolbarDockPosition.Left)]
    static IEnumerable<MainToolbarElement> CreateAllScenesToggle()
    {
        yield return new MainToolbarToggle(
            new MainToolbarContent("All Scenes", "Toggle between Build Settings scenes and all project scenes"),
            SceneToolbarPrefs.FetchAllScenes,
            isOn =>
            {
                SceneToolbarPrefs.FetchAllScenes = isOn;
                MainToolbar.Refresh(KSceneSwitcher);
                MainToolbar.Refresh(KAdditiveSelector);
            });
    }

    // ─────────────────────────────────────────────────────────
    // 2. Scene Switcher Dropdown (switch active scene)
    // ─────────────────────────────────────────────────────────

    [MainToolbarElement(KSceneSwitcher, defaultDockPosition = MainToolbarDockPosition.Left)]
    static IEnumerable<MainToolbarElement> CreateSceneSwitcher()
    {
        var scenes = GetSceneList();
        var activeScenePath = SceneManager.GetActiveScene().path;

        yield return new MainToolbarDropdown(
            new MainToolbarContent(GetActiveSceneName(), "Switch active scene"),
            rect =>
            {
                var menu = new GenericMenu();
                foreach (var scenePath in scenes)
                {
                    var name = Path.GetFileNameWithoutExtension(scenePath);
                    var isActive = scenePath == activeScenePath;
                    var label = isActive ? $"★ {name}" : name;
                    var path = scenePath;

                    menu.AddItem(new GUIContent(label), isActive, () =>
                    {
                        if (!isActive) LoadScene(path);
                        RefreshAll();
                    });
                }

                if (scenes.Length == 0)
                    menu.AddDisabledItem(new GUIContent("No scenes found"));

                menu.DropDown(rect);
            });
    }

    // ─────────────────────────────────────────────────────────
    // 3. Additive Scene Selector Dropdown (pick scene to add)
    // ─────────────────────────────────────────────────────────

    [MainToolbarElement(KAdditiveSelector, defaultDockPosition = MainToolbarDockPosition.Left)]
    static IEnumerable<MainToolbarElement> CreateAdditiveSelector()
    {
        var scenes = GetSceneList();
        var loadedPaths = GetLoadedScenePaths();

        yield return new MainToolbarDropdown(
            new MainToolbarContent("Add Scene", "Select scene to load additively"),
            rect =>
            {
                var menu = new GenericMenu();
                foreach (var scenePath in scenes)
                {
                    var name = Path.GetFileNameWithoutExtension(scenePath);
                    var isLoaded = loadedPaths.Contains(scenePath);
                    var label = isLoaded ? $"✓ {name}" : name;
                    var path = scenePath;

                    menu.AddItem(new GUIContent(label), isLoaded, () =>
                    {
                        if (!isLoaded)
                        {
                            LoadSceneAdditive(path);
                            TrackScene(path);
                        }
                        RefreshAll();
                    });
                }

                if (scenes.Length == 0)
                    menu.AddDisabledItem(new GUIContent("No scenes found"));

                menu.DropDown(rect);
            });
    }

    // ─────────────────────────────────────────────────────────
    // 4. "+" Button (quick-add selected scene)
    // ─────────────────────────────────────────────────────────

    [MainToolbarElement(KAdditiveAddBtn, defaultDockPosition = MainToolbarDockPosition.Left)]
    static IEnumerable<MainToolbarElement> CreateAdditiveAddButton()
    {
        yield return new MainToolbarButton(
            new MainToolbarContent("+", "Load last selected scene additively"),
            () =>
            {
                var lastPath = SceneToolbarPrefs.LastAdditiveScenePath;
                if (!string.IsNullOrEmpty(lastPath) && File.Exists(lastPath))
                {
                    LoadSceneAdditive(lastPath);
                    TrackScene(lastPath);
                    RefreshAll();
                }
            });
    }

    // ─────────────────────────────────────────────────────────
    // 5. Scenes Manager Dropdown (manage loaded additive scenes)
    // ─────────────────────────────────────────────────────────

    [MainToolbarElement(KScenesManager, defaultDockPosition = MainToolbarDockPosition.Left)]
    static IEnumerable<MainToolbarElement> CreateScenesManager()
    {
        yield return new MainToolbarDropdown(
            new MainToolbarContent(GetScenesLabel(), "Manage loaded scenes"),
            ShowScenesManagerMenu);
    }

    static void ShowScenesManagerMenu(Rect buttonRect)
    {
        var menu = new GenericMenu();
        var loadedPaths = GetLoadedScenePaths();
        var addedScenes = SceneToolbarPrefs.GetAddedScenes();

        // Merge loaded scenes into tracking list
        var merged = false;
        foreach (var path in loadedPaths.Where(p => !addedScenes.Contains(p)))
        {
            addedScenes.Add(path);
            merged = true;
        }
        if (merged) SceneToolbarPrefs.SaveAddedScenes(addedScenes);
        addedScenes = addedScenes.Where(File.Exists).ToList();

        if (addedScenes.Count == 0)
        {
            menu.AddDisabledItem(new GUIContent("No scenes tracked"));
            menu.DropDown(buttonRect);
            return;
        }

        // Bulk actions
        var unloadedCount = addedScenes.Count(p => !loadedPaths.Contains(p));
        var loadedCount = addedScenes.Count(p => loadedPaths.Contains(p));

        if (unloadedCount > 0)
            menu.AddItem(new GUIContent($"Load All ({unloadedCount})"), false, () => { LoadAllScenes(addedScenes); RefreshAll(); });
        if (loadedCount > 0)
            menu.AddItem(new GUIContent($"Unload All ({loadedCount})"), false, () => { UnloadAllScenes(addedScenes); RefreshAll(); });
        if (unloadedCount > 0 || loadedCount > 0)
            menu.AddSeparator("");

        // Per-scene items
        var activeScenePath = SceneManager.GetActiveScene().path;
        var soloPath = SceneToolbarPrefs.SoloScenePath;

        foreach (var scenePath in addedScenes)
        {
            var name = Path.GetFileNameWithoutExtension(scenePath);
            var isLoaded = loadedPaths.Contains(scenePath);
            var isActive = scenePath == activeScenePath;
            var isSolo = scenePath == soloPath;

            var prefix = isActive ? "★ " : isSolo ? "● " : isLoaded ? "✓ " : "  ";
            var menuPath = $"{prefix}{name}/";
            var path = scenePath;

            if (isLoaded)
            {
                menu.AddItem(new GUIContent($"{menuPath}Unload"), false, () => { UnloadScene(path); RefreshAll(); });
                if (!isActive)
                    menu.AddItem(new GUIContent($"{menuPath}Set Active"), false, () => { SetActiveScene(path); RefreshAll(); });
            }
            else
            {
                menu.AddItem(new GUIContent($"{menuPath}Load"), false, () => { LoadSceneAdditive(path); RefreshAll(); });
            }

            menu.AddSeparator(menuPath);
            menu.AddItem(new GUIContent($"{menuPath}Solo"), isSolo, () => { ToggleSolo(path); RefreshAll(); });
            menu.AddSeparator(menuPath);
            menu.AddItem(new GUIContent($"{menuPath}Remove from List"), false, () => { RemoveFromList(path); RefreshAll(); });
        }

        menu.DropDown(buttonRect);
    }

    // ─────────────────────────────────────────────────────────
    // Helpers
    // ─────────────────────────────────────────────────────────

    static void RefreshAll()
    {
        MainToolbar.Refresh(KAllScenesToggle);
        MainToolbar.Refresh(KSceneSwitcher);
        MainToolbar.Refresh(KAdditiveSelector);
        MainToolbar.Refresh(KScenesManager);
    }

    static string GetActiveSceneName()
    {
        var name = Path.GetFileNameWithoutExtension(SceneManager.GetActiveScene().path);
        return string.IsNullOrEmpty(name) ? "Untitled" : name;
    }

    static string GetScenesLabel()
    {
        var loaded = GetLoadedScenePaths();
        var added = SceneToolbarPrefs.GetAddedScenes().Where(File.Exists).ToList();
        if (added.Count == 0) return "Scenes (0)";
        var activeCount = added.Count(p => loaded.Contains(p));
        return activeCount > 0 ? $"Scenes ({activeCount}/{added.Count})" : $"Scenes ({added.Count})";
    }

    static string[] GetSceneList()
    {
        return SceneToolbarPrefs.FetchAllScenes
            ? Directory.GetFiles("Assets", "*.unity", SearchOption.AllDirectories)
            : EditorBuildSettings.scenes
                .Where(s => s.enabled && File.Exists(s.path))
                .Select(s => s.path)
                .ToArray();
    }

    static HashSet<string> GetLoadedScenePaths()
    {
        var paths = new HashSet<string>();
        for (var i = 0; i < SceneManager.sceneCount; i++)
        {
            var scene = SceneManager.GetSceneAt(i);
            if (scene.IsValid() && !string.IsNullOrEmpty(scene.path))
                paths.Add(scene.path);
        }
        return paths;
    }

    static void TrackScene(string path)
    {
        SceneToolbarPrefs.LastAdditiveScenePath = path;
        var added = SceneToolbarPrefs.GetAddedScenes();
        if (!added.Contains(path))
        {
            added.Add(path);
            SceneToolbarPrefs.SaveAddedScenes(added);
        }
    }

    static void LoadScene(string scenePath)
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode) return;
        if (string.IsNullOrEmpty(scenePath) || !File.Exists(scenePath)) return;
        if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            EditorSceneManager.OpenScene(scenePath);
    }

    static void LoadSceneAdditive(string path)
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode) return;
        if (string.IsNullOrEmpty(path) || !File.Exists(path)) return;

        for (var i = 0; i < SceneManager.sceneCount; i++)
        {
            var s = SceneManager.GetSceneAt(i);
            if (s.path == path && s.isLoaded) return;
        }

        try { EditorSceneManager.OpenScene(path, OpenSceneMode.Additive); }
        catch (Exception e) { Debug.LogError($"[SceneToolbar] Failed to load scene: {e.Message}"); }
    }

    static void UnloadScene(string scenePath)
    {
        for (var i = 0; i < SceneManager.sceneCount; i++)
        {
            var scene = SceneManager.GetSceneAt(i);
            if (scene.path == scenePath && scene.isLoaded)
            {
                if (EditorSceneManager.SaveModifiedScenesIfUserWantsTo(new[] { scene }))
                    EditorSceneManager.CloseScene(scene, false);
                break;
            }
        }
    }

    static void SetActiveScene(string scenePath)
    {
        for (var i = 0; i < SceneManager.sceneCount; i++)
        {
            var scene = SceneManager.GetSceneAt(i);
            if (scene.path == scenePath && scene.isLoaded)
            {
                EditorSceneManager.SetActiveScene(scene);
                break;
            }
        }
    }

    static void ToggleSolo(string scenePath)
    {
        if (SceneToolbarPrefs.SoloScenePath == scenePath)
        {
            SceneToolbarPrefs.SoloScenePath = "";
            return;
        }

        SceneToolbarPrefs.SoloScenePath = scenePath;
        if (EditorApplication.isPlayingOrWillChangePlaymode) return;
        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;

        for (var i = SceneManager.sceneCount - 1; i >= 0; i--)
        {
            var scene = SceneManager.GetSceneAt(i);
            if (scene.IsValid() && scene.path != scenePath && scene.isLoaded)
                EditorSceneManager.CloseScene(scene, false);
        }

        for (var i = 0; i < SceneManager.sceneCount; i++)
        {
            var scene = SceneManager.GetSceneAt(i);
            if (scene.path == scenePath && scene.isLoaded)
            {
                EditorSceneManager.SetActiveScene(scene);
                break;
            }
        }
    }

    static void RemoveFromList(string scenePath)
    {
        if (SceneToolbarPrefs.SoloScenePath == scenePath)
            SceneToolbarPrefs.SoloScenePath = "";

        UnloadScene(scenePath);

        var added = SceneToolbarPrefs.GetAddedScenes();
        added.Remove(scenePath);
        SceneToolbarPrefs.SaveAddedScenes(added);
    }

    static void LoadAllScenes(List<string> scenes)
    {
        foreach (var path in scenes) LoadSceneAdditive(path);
    }

    static void UnloadAllScenes(List<string> scenes)
    {
        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
        var soloPath = SceneToolbarPrefs.SoloScenePath;
        for (var i = SceneManager.sceneCount - 1; i >= 0; i--)
        {
            var scene = SceneManager.GetSceneAt(i);
            if (scene.IsValid() && scene.isLoaded && scenes.Contains(scene.path) && scene.path != soloPath)
                EditorSceneManager.CloseScene(scene, false);
        }
    }

    // ─────────────────────────────────────────────────────────
    // Prefs
    // ─────────────────────────────────────────────────────────

    static class SceneToolbarPrefs
    {
        public static bool FetchAllScenes
        {
            get => EditorPrefs.GetBool("SceneSwitcher_FetchAllScenes", false);
            set => EditorPrefs.SetBool("SceneSwitcher_FetchAllScenes", value);
        }

        public static string SoloScenePath
        {
            get => EditorPrefs.GetString("SceneSwitcher_SoloScene", "");
            set => EditorPrefs.SetString("SceneSwitcher_SoloScene", value);
        }

        public static string LastAdditiveScenePath
        {
            get => EditorPrefs.GetString("SceneSwitcher_LastAdditive", "");
            set => EditorPrefs.SetString("SceneSwitcher_LastAdditive", value);
        }

        public static List<string> GetAddedScenes()
        {
            var json = EditorPrefs.GetString("SceneSwitcher_AddedScenes", "[]");
            if (string.IsNullOrEmpty(json) || json == "[]")
                return new List<string>();
            return JsonUtility.FromJson<SceneList>(json).Scenes ?? new List<string>();
        }

        public static void SaveAddedScenes(List<string> scenes)
        {
            EditorPrefs.SetString("SceneSwitcher_AddedScenes",
                JsonUtility.ToJson(new SceneList { Scenes = scenes }));
        }

        [Serializable]
        class SceneList
        {
            [FormerlySerializedAs("scenes")] public List<string> Scenes;
        }
    }
}
#endif
