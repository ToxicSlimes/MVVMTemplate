#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UIElements;
using System.Reflection;
using System.Linq;
using System.IO;
using System.Collections.Generic;
using System;
using UnityEngine.Serialization;

[InitializeOnLoad]
public static class SceneToolbar
{
    private const float SPositionOffset = 180f;
    private const float SDropdownBoxHeight = 20f;
    
    private static string[] s_sceneNames = Array.Empty<string>();
    private static int s_selectedIndex;
    private static int s_addSceneIndex = 0;
    private static string s_lastActiveScene = "";
    private static VisualElement s_toolbarUI;
    private static string s_soloScenePath = "";

    private static string SoloScenePath
    {
        get => EditorPrefs.GetString("SceneSwitcher_SoloScene", "");
        set
        {
            s_soloScenePath = value;
            EditorPrefs.SetString("SceneSwitcher_SoloScene", value);
        }
    }

    private static bool FetchAllScenes
    {
        get => EditorPrefs.GetBool("SceneSwitcher_FetchAllScenes", false);
        set => EditorPrefs.SetBool("SceneSwitcher_FetchAllScenes", value);
    }

    private static List<string> GetAddedScenes()
    {
        var scenesJson = EditorPrefs.GetString("SceneSwitcher_AddedScenes", "[]");
        if (string.IsNullOrEmpty(scenesJson) || scenesJson == "[]")
            return new List<string>();
        return JsonUtility.FromJson<SceneList>(scenesJson).Scenes ?? new List<string>();
    }

    private static void SaveAddedScenes(List<string> scenes)
    {
        var list = new SceneList { Scenes = scenes };
        EditorPrefs.SetString("SceneSwitcher_AddedScenes", JsonUtility.ToJson(list));
    }

    [Serializable]
    private class SceneList
    {
        [FormerlySerializedAs("scenes")] public List<string> Scenes;
    }

    static SceneToolbar()
    {
        s_soloScenePath = SoloScenePath;
        RefreshSceneList();
        SelectCurrentScene();
        EditorBuildSettings.sceneListChanged += RefreshSceneList;
        EditorApplication.projectChanged += RefreshSceneList;
        EditorSceneManager.activeSceneChangedInEditMode += (prev, current) => UpdateSceneSelection();
        EditorApplication.playModeStateChanged += OnPlayModeChanged;
        EditorApplication.delayCall += AddToolbarUI;
    }

    private static void AddToolbarUI()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode)
            return;
        var toolbarType = typeof(Editor).Assembly.GetType("UnityEditor.Toolbar");
        if (toolbarType == null) return;
        var toolbars = Resources.FindObjectsOfTypeAll(toolbarType);
        if (toolbars.Length == 0) return;
        var toolbar = toolbars[0];
        var rootField = toolbarType.GetField("m_Root", BindingFlags.NonPublic | BindingFlags.Instance);
        if (rootField == null) return;
        var root = rootField.GetValue(toolbar) as VisualElement;
        var leftContainer = root?.Q("ToolbarZoneLeftAlign");
        if (leftContainer == null) return;
        if (s_toolbarUI != null)
            leftContainer.Remove(s_toolbarUI);
        s_toolbarUI = new IMGUIContainer(OnGUI);
        s_toolbarUI.style.marginLeft = SPositionOffset;
        leftContainer.Add(s_toolbarUI);
    }

    private static void OnGUI()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode)
            return;
        CheckAndRefreshScenes();
        if (s_selectedIndex >= s_sceneNames.Length)
            s_selectedIndex = 0;
        if (s_addSceneIndex >= s_sceneNames.Length)
            s_addSceneIndex = 0;
        var isPlaying = EditorApplication.isPlaying;
        GUILayout.BeginHorizontal();
        EditorGUI.BeginDisabledGroup(isPlaying);
        var newFetchAllScenes = GUILayout.Toggle(FetchAllScenes, "All Scenes", "Button", GUILayout.Height(SDropdownBoxHeight));
        if (newFetchAllScenes != FetchAllScenes)
        {
            FetchAllScenes = newFetchAllScenes;
            RefreshSceneList();
            SelectCurrentScene();
        }
        EditorGUI.EndDisabledGroup();
        EditorGUI.BeginDisabledGroup(isPlaying);
        var popupStyle = new GUIStyle(EditorStyles.popup)
        {
            fixedHeight = SDropdownBoxHeight
        };
        var newIndex = EditorGUILayout.Popup(s_selectedIndex, s_sceneNames, popupStyle, GUILayout.Width(150), GUILayout.Height(SDropdownBoxHeight));
        if (newIndex != s_selectedIndex)
        {
            s_selectedIndex = newIndex;
            LoadScene(s_sceneNames[s_selectedIndex]);
        }
        EditorGUI.EndDisabledGroup();
        EditorGUI.BeginDisabledGroup(isPlaying);
        var newAddIndex = EditorGUILayout.Popup(s_addSceneIndex, s_sceneNames, popupStyle, GUILayout.Width(150), GUILayout.Height(SDropdownBoxHeight));
        if (newAddIndex != s_addSceneIndex)
        {
            s_addSceneIndex = newAddIndex;
        }
        if (GUILayout.Button("+", GUILayout.Width(25), GUILayout.Height(SDropdownBoxHeight)))
        {
            AddSceneAdditive(s_sceneNames[s_addSceneIndex]);
        }
        EditorGUI.EndDisabledGroup();
        var addedScenes = GetAddedScenes();
        var allScenes = GetLoadedScenes();
        var scenesDict = new Dictionary<string, UnityEngine.SceneManagement.Scene>();
        foreach (var scene in allScenes)
        {
            if (scene.IsValid() && !string.IsNullOrEmpty(scene.path))
            {
                scenesDict[scene.path] = scene;
                if (!addedScenes.Contains(scene.path))
                {
                    addedScenes.Add(scene.path);
                }
            }
        }
        SaveAddedScenes(addedScenes);
        addedScenes = addedScenes.Where(File.Exists).ToList();
        if (addedScenes.Count is > 0 and > 0)
        {
            EditorGUI.BeginDisabledGroup(isPlaying);
            var displayText = "Scenes (" + addedScenes.Count + ")";
            var activeCount = addedScenes.Count(p => scenesDict.ContainsKey(p) && scenesDict[p].IsValid() && scenesDict[p].isLoaded);
            if (activeCount > 0)
            {
                displayText = $"Scenes ({activeCount}/{addedScenes.Count})";
            }
            var buttonRect = GUILayoutUtility.GetRect(new GUIContent(displayText), popupStyle, GUILayout.Width(150), GUILayout.Height(SDropdownBoxHeight));
            if (GUI.Button(buttonRect, displayText, popupStyle))
            {
                ShowAddedScenesMenu(addedScenes, scenesDict, buttonRect);
            }
            EditorGUI.EndDisabledGroup();
        }
        GUILayout.EndHorizontal();
    }

    private static void AddSceneAdditive(string sceneName)
    {
        if (string.IsNullOrEmpty(sceneName))
            return;

        var cleanSceneName = sceneName;
        if (cleanSceneName.EndsWith(" (not in build index)"))
        {
            cleanSceneName = cleanSceneName.Replace(" (not in build index)", "");
        }

        string scenePath = null;
        if (FetchAllScenes)
        {
            scenePath = Directory.GetFiles("Assets", "*.unity", SearchOption.AllDirectories)
                .FirstOrDefault(path => Path.GetFileNameWithoutExtension(path) == cleanSceneName);
        }
        else
        {
            var buildScene = EditorBuildSettings.scenes
                .FirstOrDefault(scene => scene.enabled && Path.GetFileNameWithoutExtension(scene.path) == cleanSceneName);
            if (buildScene?.path != null && File.Exists(buildScene.path))
                scenePath = buildScene.path;
        }

        if (string.IsNullOrEmpty(scenePath))
        {
            scenePath = Directory.GetFiles("Assets", "*.unity", SearchOption.AllDirectories)
                .FirstOrDefault(path => Path.GetFileNameWithoutExtension(path) == cleanSceneName);
        }

        if (string.IsNullOrEmpty(scenePath) || !File.Exists(scenePath))
        {
            Debug.LogWarning($"<color=orange>Scene Switcher:</color> Scene \"{cleanSceneName}\" could not be found.");
            return;
        }

        for (var i = 0; i < UnityEngine.SceneManagement.SceneManager.sceneCount; i++)
        {
            var loadedScene = UnityEngine.SceneManagement.SceneManager.GetSceneAt(i);
            if (loadedScene.path != scenePath || !loadedScene.isLoaded)
                continue;
            Debug.LogWarning($"<color=orange>Scene Switcher:</color> Scene \"{cleanSceneName}\" is already loaded.");
            return;
        }

        try
        {
            var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Additive);
            if (!scene.IsValid())
                return;
            var addedScenes = GetAddedScenes();
            if (addedScenes.Contains(scenePath))
                return;
            addedScenes.Add(scenePath);
            SaveAddedScenes(addedScenes);
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to add scene: {e.Message}");
        }
    }

    private static void RefreshSceneList()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode)
            return;
        if (FetchAllScenes)
        {
            s_sceneNames = Directory.GetFiles("Assets", "*.unity", SearchOption.AllDirectories)
                .Select(Path.GetFileNameWithoutExtension)
                .ToArray();
        }
        else
        {
            var validScenes = EditorBuildSettings.scenes
                .Where(scene => scene.enabled && File.Exists(scene.path))
                .Select(scene => Path.GetFileNameWithoutExtension(scene.path))
                .ToArray();
            var missingScenes = EditorBuildSettings.scenes
                .Where(s => s.enabled && !File.Exists(s.path))
                .Select(s => Path.GetFileNameWithoutExtension(s.path))
                .ToArray();
            if (missingScenes.Length > 0)
            {
                Debug.LogWarning(
                    $"<color=orange>Scene Switcher:</color> Ignored {missingScenes.Length} missing scene(s) still listed in Build Settings:\n{string.Join(", ", missingScenes)}");
            }
            s_sceneNames = validScenes;
        }
        SelectCurrentScene();
        s_toolbarUI?.MarkDirtyRepaint();
    }

    private static void CheckAndRefreshScenes()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode)
            return;
        var currentScenes = FetchAllScenes
            ? Directory.GetFiles("Assets", "*.unity", SearchOption.AllDirectories).Select(Path.GetFileNameWithoutExtension).ToArray()
            : EditorBuildSettings.scenes.Where(scene => scene.enabled).Select(scene => Path.GetFileNameWithoutExtension(scene.path)).ToArray();
        var currentHash = string.Join(",", currentScenes);
        var lastHash = string.Join(",", s_sceneNames);
        if (currentHash == lastHash)
            return;
        s_sceneNames = currentScenes;
        SelectCurrentScene();
    }

    private static void SelectCurrentScene()
    {
        var currentScene = Path.GetFileNameWithoutExtension(EditorSceneManager.GetActiveScene().path);
        var index = Array.IndexOf(s_sceneNames, currentScene);
        if (index != -1)
        {
            s_selectedIndex = index;
            s_lastActiveScene = currentScene;
        }
        else
        {
            var notInBuildName = currentScene + " (not in build index)";
            s_sceneNames = new[] { notInBuildName }.Concat(s_sceneNames).ToArray();
            s_selectedIndex = 0;
            s_lastActiveScene = currentScene;
        }
    }

    private static void UpdateSceneSelection()
    {
        var currentScene = Path.GetFileNameWithoutExtension(EditorSceneManager.GetActiveScene().path);
        if (currentScene != s_lastActiveScene)
        {
            s_lastActiveScene = currentScene;
            s_sceneNames = s_sceneNames.Where(name => !name.EndsWith(" (not in build index)")).ToArray();
            SelectCurrentScene();
        }
    }

    private static void LoadScene(string sceneName)
    {
        string scenePath = null;
        if (FetchAllScenes)
        {
            scenePath = Directory.GetFiles("Assets", "*.unity", SearchOption.AllDirectories).FirstOrDefault(path => Path.GetFileNameWithoutExtension(path) == sceneName);
        }
        else
        {
            var buildScene = EditorBuildSettings.scenes.FirstOrDefault(scene => scene.enabled && Path.GetFileNameWithoutExtension(scene.path) == sceneName);
            if (buildScene.path != null && File.Exists(buildScene.path))
                scenePath = buildScene.path;
        }
        if (string.IsNullOrEmpty(scenePath) || !File.Exists(scenePath))
        {
            Debug.LogWarning(
                $"<color=orange>Scene Switcher:</color> Scene \"{sceneName}\" could not be found or has been deleted.\nPlease remove it from Build Settings or re-add the file.");
            return;
        }
        if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
        {
            EditorSceneManager.OpenScene(scenePath);
        }
    }

    private static void OnPlayModeChanged(PlayModeStateChange state)
    {
        if (state is PlayModeStateChange.EnteredPlayMode or PlayModeStateChange.ExitingPlayMode)
        {
            EditorApplication.delayCall += () => AddToolbarUI();
        }
    }

    private static List<UnityEngine.SceneManagement.Scene> GetLoadedScenes()
    {
        var scenes = new List<UnityEngine.SceneManagement.Scene>();
        for (var i = 0; i < UnityEngine.SceneManagement.SceneManager.sceneCount; i++)
        {
            var scene = UnityEngine.SceneManagement.SceneManager.GetSceneAt(i);
            if (scene.IsValid())
            {
                scenes.Add(scene);
            }
        }
        return scenes;
    }

    private static void UnloadScene(string scenePath)
    {
        var allScenes = GetLoadedScenes();
        foreach (var scene in allScenes)
        {
            if (scene.IsValid() && scene.path == scenePath && scene.isLoaded)
            {
                if (EditorSceneManager.SaveModifiedScenesIfUserWantsTo(new[] { scene }))
                {
                    EditorSceneManager.CloseScene(scene, false);
                }
                break;
            }
        }
        s_toolbarUI?.MarkDirtyRepaint();
    }

    private static void LoadSceneAdditive(string scenePath)
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode)
            return;
        if (string.IsNullOrEmpty(scenePath) || !File.Exists(scenePath))
            return;
        for (var i = 0; i < UnityEngine.SceneManagement.SceneManager.sceneCount; i++)
        {
            var loadedScene = UnityEngine.SceneManagement.SceneManager.GetSceneAt(i);
            if (loadedScene.path == scenePath && loadedScene.isLoaded)
            {
                return;
            }
        }
        try
        {
            var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Additive);
            if (scene.IsValid())
            {
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to load scene: {e.Message}");
        }
        s_toolbarUI?.MarkDirtyRepaint();
    }

    private static void SetActiveScene(string scenePath)
    {
        var allScenes = GetLoadedScenes();
        foreach (var scene in allScenes)
        {
            if (scene.IsValid() && scene.path == scenePath && scene.isLoaded)
            {
                EditorSceneManager.SetActiveScene(scene);
                break;
            }
        }
        s_toolbarUI?.MarkDirtyRepaint();
    }

    private static void ToggleSolo(string scenePath)
    {
        if (s_soloScenePath == scenePath)
        {
            SoloScenePath = "";
            s_soloScenePath = "";
        }
        else
        {
            SoloScenePath = scenePath;
            s_soloScenePath = scenePath;
            ApplySoloMode();
        }
        s_toolbarUI?.MarkDirtyRepaint();
    }

    private static void ApplySoloMode()
    {
        if (string.IsNullOrEmpty(s_soloScenePath))
            return;
        if (EditorApplication.isPlayingOrWillChangePlaymode)
            return;
        var allScenes = GetLoadedScenes();
        var scenesToUnload = new List<UnityEngine.SceneManagement.Scene>();
        foreach (var scene in allScenes)
        {
            if (scene.IsValid() && scene.path != s_soloScenePath && scene.isLoaded)
            {
                scenesToUnload.Add(scene);
            }
        }
        if (scenesToUnload.Count > 0)
        {
            if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            {
                foreach (var scene in scenesToUnload)
                {
                    if (EditorSceneManager.SaveModifiedScenesIfUserWantsTo(new[] { scene }))
                    {
                        EditorSceneManager.CloseScene(scene, false);
                    }
                }
                var remainingScenes = GetLoadedScenes();
                foreach (var scene in remainingScenes)
                {
                    if (scene.IsValid() && scene.path == s_soloScenePath && scene.isLoaded)
                    {
                        EditorSceneManager.SetActiveScene(scene);
                        break;
                    }
                }
            }
        }
    }

    private static void RemoveFromList(string scenePath)
    {
        if (s_soloScenePath == scenePath)
        {
            SoloScenePath = "";
            s_soloScenePath = "";
        }
        for (var i = 0; i < UnityEngine.SceneManagement.SceneManager.sceneCount; i++)
        {
            var scene = UnityEngine.SceneManagement.SceneManager.GetSceneAt(i);
            if (scene.IsValid() && scene.path == scenePath)
            {
                if (scene.isLoaded)
                {
                    if (EditorSceneManager.SaveModifiedScenesIfUserWantsTo(new[] { scene }))
                    {
                        EditorSceneManager.CloseScene(scene, true);
                    }
                }
                else
                {
                    EditorSceneManager.CloseScene(scene, true);
                }
                break;
            }
        }
        var addedScenes = GetAddedScenes();
        addedScenes.Remove(scenePath);
        SaveAddedScenes(addedScenes);
        s_toolbarUI?.MarkDirtyRepaint();
    }

    private static void DeleteSceneFile(string scenePath)
    {
        if (!File.Exists(scenePath))
            return;
        if (!EditorUtility.DisplayDialog("Delete Scene", $"Are you sure you want to delete scene file?\n\n{scenePath}\n\nThis action cannot be undone!", "Delete", "Cancel"))
            return;
        var allScenes = GetLoadedScenes();
        foreach (var scene in allScenes)
        {
            if (scene.IsValid() && scene.path == scenePath)
            {
                if (scene.isLoaded)
                {
                    if (!EditorSceneManager.SaveModifiedScenesIfUserWantsTo(new[] { scene }))
                        return;
                    EditorSceneManager.CloseScene(scene, true);
                }
                break;
            }
        }
        try
        {
            File.Delete(scenePath);
            var metaPath = scenePath + ".meta";
            if (File.Exists(metaPath))
            {
                File.Delete(metaPath);
            }
            AssetDatabase.Refresh();
            RemoveFromList(scenePath);
            Debug.Log($"<color=green>Scene Switcher:</color> Scene file deleted: {scenePath}");
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to delete scene file: {e.Message}");
        }
    }

    private static void LoadAllScenes(List<string> addedScenes)
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode)
            return;
        foreach (var scenePath in addedScenes)
        {
            if (string.IsNullOrEmpty(scenePath) || !File.Exists(scenePath))
                continue;
            var alreadyLoaded = false;
            for (var i = 0; i < UnityEngine.SceneManagement.SceneManager.sceneCount; i++)
            {
                var loadedScene = UnityEngine.SceneManagement.SceneManager.GetSceneAt(i);
                if (loadedScene.path == scenePath && loadedScene.isLoaded)
                {
                    alreadyLoaded = true;
                    break;
                }
            }
            if (!alreadyLoaded)
            {
                try
                {
                    var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Additive);
                    if (scene.IsValid())
                    {
                    }
                }
                catch (Exception e)
                {
                    Debug.LogError($"Failed to load scene: {e.Message}");
                }
            }
        }
        s_toolbarUI?.MarkDirtyRepaint();
    }

    private static void UnloadAllScenes(List<string> addedScenes)
    {
        var allScenes = GetLoadedScenes();
        var scenesToUnload = new List<UnityEngine.SceneManagement.Scene>();
        foreach (var scene in allScenes)
        {
            if (scene.IsValid() && scene.isLoaded && scene.path != s_soloScenePath && addedScenes.Contains(scene.path))
            {
                scenesToUnload.Add(scene);
            }
        }
        if (scenesToUnload.Count > 0)
        {
            if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            {
                foreach (var scene in scenesToUnload)
                {
                    if (EditorSceneManager.SaveModifiedScenesIfUserWantsTo(new[] { scene }))
                    {
                        EditorSceneManager.CloseScene(scene, false);
                    }
                }
            }
        }
        s_toolbarUI?.MarkDirtyRepaint();
    }

    private static void ShowAddedScenesMenu(List<string> addedScenes, Dictionary<string, UnityEngine.SceneManagement.Scene> scenesDict, Rect position)
    {
        var menu = new GenericMenu();
        var unloadedCount = 0;
        var loadedCount = 0;
        foreach (var scenePath in addedScenes)
        {
            if (!File.Exists(scenePath))
                continue;
            var sceneExists = scenesDict.ContainsKey(scenePath);
            var isLoaded = sceneExists && scenesDict[scenePath].isLoaded;
            if (!isLoaded)
            {
                unloadedCount++;
            }
            else if (isLoaded && scenePath != s_soloScenePath)
            {
                loadedCount++;
            }
        }
        if (unloadedCount > 0)
        {
            menu.AddItem(new GUIContent($"Load All ({unloadedCount})"), false, () => LoadAllScenes(addedScenes));
            menu.AddSeparator("");
        }
        if (loadedCount > 0)
        {
            menu.AddItem(new GUIContent($"Unload All ({loadedCount})"), false, () => UnloadAllScenes(addedScenes));
            menu.AddSeparator("");
        }
        foreach (var scenePath in addedScenes)
        {
            if (!File.Exists(scenePath))
                continue;
            var sceneName = Path.GetFileNameWithoutExtension(scenePath);
            var sceneExists = scenesDict.ContainsKey(scenePath);
            var isLoaded = sceneExists && scenesDict[scenePath].isLoaded;
            var scene = sceneExists ? scenesDict[scenePath] : default;
            var isActive = scene.IsValid() && scene == EditorSceneManager.GetActiveScene();
            var isSolo = scenePath == s_soloScenePath;
            var displayName = sceneName;
            if (isActive) displayName = "★ " + displayName;
            else if (isSolo) displayName = "● " + displayName;
            else if (isLoaded) displayName = "✓ " + displayName;
            var menuPath = displayName + "/";
            if (isLoaded)
            {
                menu.AddItem(new GUIContent(menuPath + "Unload"), false, () => UnloadScene(scenePath));
                if (!isActive)
                {
                    menu.AddItem(new GUIContent(menuPath + "Set Active"), false, () => SetActiveScene(scenePath));
                }
            }
            else
            {
                menu.AddItem(new GUIContent(menuPath + "Load"), false, () => LoadSceneAdditive(scenePath));
            }
            menu.AddSeparator(menuPath);
            menu.AddItem(new GUIContent(menuPath + "Solo"), isSolo, () => ToggleSolo(scenePath));
            menu.AddSeparator(menuPath);
            menu.AddItem(new GUIContent(menuPath + "Remove from List"), false, () => RemoveFromList(scenePath));
            menu.AddItem(new GUIContent(menuPath + "Delete Scene File"), false, () => DeleteSceneFile(scenePath));
        }
        menu.DropDown(position);
    }
}

#endif

