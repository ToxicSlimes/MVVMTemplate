#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using Core.Editor.PrefabBuilderSystem.Settings;
using Core.Editor.PrefabBuilderSystem.Validation;

namespace Core.Editor.PrefabBuilderSystem.Core
{
    public abstract class BasePrefabBuilderWindow<TBuilder> : EditorWindow
        where TBuilder : ViewBuilder, new()
    {
        protected TBuilder Builder { get; private set; }

        protected virtual string WindowTitle => "Prefab Builder";
        protected virtual Vector2 MinWindowSize => new(450, 600);

        protected virtual void OnEnable()
        {
            minSize = MinWindowSize;
            titleContent = new GUIContent(WindowTitle);
            Builder = CreateBuilder();
        }

        protected virtual TBuilder CreateBuilder() => new();

        protected void BuildPrefab()
        {
            Builder.Build();
        }

        protected void ValidateOnly()
        {
            var report = Builder.ValidateOnly();
            ValidationReportWindow.ShowReport(report);
        }

        protected void DrawValidationControls()
        {
            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button("Validate", GUILayout.Height(25)))
            {
                ValidateOnly();
            }

            var oldColor = GUI.backgroundColor;
            GUI.backgroundColor = new Color(0.3f, 0.8f, 0.3f);
            if (GUILayout.Button("BUILD", GUILayout.Height(25)))
            {
                BuildPrefab();
            }
            GUI.backgroundColor = oldColor;

            EditorGUILayout.EndHorizontal();

            if (Builder.LastValidationReport != null && Builder.LastValidationReport.Issues.Count > 0)
            {
                var report = Builder.LastValidationReport;
                var color = report.HasErrors ? Color.red : (report.HasWarnings ? Color.yellow : Color.green);
                var oldGUIColor = GUI.color;
                GUI.color = color;
                EditorGUILayout.LabelField($"Last validation: {report.GetSummary()}", EditorStyles.miniLabel);
                GUI.color = oldGUIColor;
            }
        }

        protected abstract void OnGUI();

        protected void DrawSettingsControls<TSettings>(TSettings settings, string settingsPath = null)
            where TSettings : BuilderSettingsBase
        {
            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button("Reset", GUILayout.Width(60)))
            {
                if (EditorUtility.DisplayDialog("Reset Settings",
                    "Reset all settings to default values?", "Yes", "Cancel"))
                {
                    settings.ResetToDefaults();
                    EditorUtility.SetDirty(settings);
                }
            }

            if (GUILayout.Button("Save Preset", GUILayout.Width(80)))
            {
                var path = EditorUtility.SaveFilePanelInProject(
                    "Save Preset",
                    settings.GetType().Name + "_Preset",
                    "asset",
                    "Save settings preset");

                if (!string.IsNullOrEmpty(path))
                {
                    var preset = settings.Clone<TSettings>();
                    AssetDatabase.CreateAsset(preset, path);
                    AssetDatabase.SaveAssets();
                }
            }

            if (GUILayout.Button("Load Preset", GUILayout.Width(80)))
            {
                var path = EditorUtility.OpenFilePanel("Load Preset", "Assets", "asset");
                if (!string.IsNullOrEmpty(path))
                {
                    path = "Assets" + path.Substring(UnityEngine.Application.dataPath.Length);
                    var preset = AssetDatabase.LoadAssetAtPath<TSettings>(path);
                    if (preset != null)
                    {
                        EditorUtility.CopySerialized(preset, settings);
                        EditorUtility.SetDirty(settings);
                    }
                }
            }

            EditorGUILayout.EndHorizontal();
        }

        protected TSettings LoadOrCreateSettings<TSettings>(string path) where TSettings : BuilderSettingsBase
        {
            var settings = AssetDatabase.LoadAssetAtPath<TSettings>(path);

            if (settings == null)
            {
                settings = ScriptableObject.CreateInstance<TSettings>();

                var directory = System.IO.Path.GetDirectoryName(path);
                if (!string.IsNullOrEmpty(directory) && !System.IO.Directory.Exists(directory))
                    System.IO.Directory.CreateDirectory(directory);

                AssetDatabase.CreateAsset(settings, path);
                AssetDatabase.SaveAssets();
                Debug.Log($"<color=cyan>Created settings:</color> {path}");
            }

            return settings;
        }
    }

    public abstract class BasePrefabBuilderWindow : EditorWindow
    {
        protected PrefabBuilderContext Context { get; private set; }

        protected abstract string PrefabPath { get; }
        protected virtual string WindowTitle => "Prefab Builder";
        protected virtual Vector2 MinWindowSize => new(450, 600);
        protected virtual Vector2 ReferenceResolution => new(1920, 1080);
        protected virtual float MatchWidthOrHeight => 0.5f;

        protected virtual void OnEnable()
        {
            minSize = MinWindowSize;
            titleContent = new GUIContent(WindowTitle);
        }

        protected void BuildPrefab()
        {
            EnsureDirectoryExists(PrefabPath);

            Context = new PrefabBuilderContext();
            Context.Root = new GameObject(System.IO.Path.GetFileNameWithoutExtension(PrefabPath));

            SetupCanvas(Context.Root);
            BuildStructure();

            PrefabUtility.SaveAsPrefabAsset(Context.Root, PrefabPath);
            DestroyImmediate(Context.Root);

            Debug.Log($"<color=green>✓ Prefab created: {PrefabPath}</color>");

            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath);
            if (prefab != null)
            {
                Selection.activeObject = prefab;
                EditorGUIUtility.PingObject(prefab);
            }
        }

        protected abstract void BuildStructure();

        protected virtual void SetupCanvas(GameObject go)
        {
            var canvas = go.AddComponent<UnityEngine.Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            Context.Canvas = canvas;

            var scaler = go.AddComponent<UnityEngine.UI.CanvasScaler>();
            scaler.uiScaleMode = UnityEngine.UI.CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = ReferenceResolution;
            scaler.matchWidthOrHeight = MatchWidthOrHeight;
            Context.CanvasScaler = scaler;

            var raycaster = go.AddComponent<UnityEngine.UI.GraphicRaycaster>();
            Context.Raycaster = raycaster;
        }

        protected void EnsureDirectoryExists(string path)
        {
            var directory = System.IO.Path.GetDirectoryName(path);
            if (!string.IsNullOrEmpty(directory) && !System.IO.Directory.Exists(directory))
            {
                System.IO.Directory.CreateDirectory(directory);
            }
        }

        protected bool TryLoadPrefab(string path, out GameObject prefab, string displayName = null)
        {
            prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);

            if (prefab == null)
            {
                Debug.LogError($"Prefab '{displayName ?? System.IO.Path.GetFileNameWithoutExtension(path)}' not found at: {path}");
                return false;
            }

            return true;
        }

        protected GameObject SpawnPrefab(GameObject prefab, Transform parent, string name)
        {
            var instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab, parent);
            instance.name = name;
            return instance;
        }

        protected void SaveElementPrefab(GameObject root, string path)
        {
            EnsureDirectoryExists(path);
            PrefabUtility.SaveAsPrefabAsset(root, path);
            DestroyImmediate(root);
            Debug.Log($"<color=cyan>Element Prefab Created:</color> {path}");
        }

        protected void DrawSettingsControls<TSettings>(TSettings settings, string settingsPath = null)
            where TSettings : BuilderSettingsBase
        {
            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button("Reset", GUILayout.Width(60)))
            {
                if (EditorUtility.DisplayDialog("Reset Settings",
                    "Reset all settings to default values?", "Yes", "Cancel"))
                {
                    settings.ResetToDefaults();
                    EditorUtility.SetDirty(settings);
                }
            }

            if (GUILayout.Button("Save Preset", GUILayout.Width(80)))
            {
                var path = EditorUtility.SaveFilePanelInProject(
                    "Save Preset",
                    settings.GetType().Name + "_Preset",
                    "asset",
                    "Save settings preset");

                if (!string.IsNullOrEmpty(path))
                {
                    var preset = settings.Clone<TSettings>();
                    AssetDatabase.CreateAsset(preset, path);
                    AssetDatabase.SaveAssets();
                }
            }

            if (GUILayout.Button("Load Preset", GUILayout.Width(80)))
            {
                var path = EditorUtility.OpenFilePanel("Load Preset", "Assets", "asset");
                if (!string.IsNullOrEmpty(path))
                {
                    path = "Assets" + path.Substring(UnityEngine.Application.dataPath.Length);
                    var preset = AssetDatabase.LoadAssetAtPath<TSettings>(path);
                    if (preset != null)
                    {
                        EditorUtility.CopySerialized(preset, settings);
                        EditorUtility.SetDirty(settings);
                    }
                }
            }

            EditorGUILayout.EndHorizontal();
        }

        protected TSettings LoadOrCreateSettings<TSettings>(string path) where TSettings : BuilderSettingsBase
        {
            var settings = AssetDatabase.LoadAssetAtPath<TSettings>(path);

            if (settings == null)
            {
                settings = ScriptableObject.CreateInstance<TSettings>();

                var directory = System.IO.Path.GetDirectoryName(path);
                if (!string.IsNullOrEmpty(directory) && !System.IO.Directory.Exists(directory))
                    System.IO.Directory.CreateDirectory(directory);

                AssetDatabase.CreateAsset(settings, path);
                AssetDatabase.SaveAssets();
                Debug.Log($"<color=cyan>Created settings:</color> {path}");
            }

            return settings;
        }
    }
}
#endif
