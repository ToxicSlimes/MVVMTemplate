#if UNITY_EDITOR
using Core.Abstractions.MVVM.Elements;
using Core.Editor.PrefabBuilderSystem.Settings;
using Core.Domain.Models.Elements;
using UnityEditor;
using UnityEngine;

namespace Core.Editor.PrefabBuilderSystem.BindableElements
{
    public abstract class BaseBindableElementBuilderWindow<TBuilder, TElement, TData> : EditorWindow
        where TBuilder : BaseBindableElementBuilder<TElement, TData>, new()
        where TElement : BindableElement<TData>
        where TData : struct, IElementData
    {
        protected Vector2 ScrollPosition;
        protected TBuilder Builder;

        protected abstract string WindowTitle { get; }
        protected abstract string PrefabOutputPath { get; }
        protected virtual Vector2 MinWindowSize => new(400, 500);

        protected virtual void OnEnable()
        {
            minSize = MinWindowSize;
            titleContent = new GUIContent(WindowTitle);
            Builder = CreateBuilder();
        }

        protected virtual TBuilder CreateBuilder() => new();

        protected virtual void OnGUI()
        {
            ScrollPosition = EditorGUILayout.BeginScrollView(ScrollPosition);

            DrawHeader();
            DrawSettings();
            DrawBuildControls();

            EditorGUILayout.EndScrollView();
        }

        protected virtual void DrawHeader()
        {
            EditorGUILayout.Space(10);
            GUILayout.Label(WindowTitle, EditorStyles.boldLabel);
            EditorGUILayout.Space(5);

            EditorGUILayout.HelpBox(
                $"Creates a bindable element prefab.\n\n" +
                $"Element Type: {typeof(TElement).Name}\n" +
                $"Data Type: {typeof(TData).Name}\n\n" +
                $"Output: {PrefabOutputPath}",
                MessageType.Info);

            EditorGUILayout.Space(5);
            EditorGUILayout.LabelField("Output Path", EditorStyles.boldLabel);
            EditorGUI.BeginDisabledGroup(true);
            EditorGUILayout.TextField(PrefabOutputPath);
            EditorGUI.EndDisabledGroup();
            EditorGUILayout.Space(10);
        }

        protected abstract void DrawSettings();

        protected virtual void DrawBuildControls()
        {
            EditorGUILayout.Space(20);

            var oldColor = GUI.backgroundColor;
            GUI.backgroundColor = new Color(0.3f, 0.8f, 0.3f);

            if (GUILayout.Button("BUILD", GUILayout.Height(40)))
            {
                Build();
            }

            GUI.backgroundColor = oldColor;
        }

        protected virtual void Build()
        {
            ApplySettingsToBuilder();
            Builder.Build();
        }

        protected abstract void ApplySettingsToBuilder();

        protected void DrawSettingsControls<TSettings>(TSettings settings)
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
