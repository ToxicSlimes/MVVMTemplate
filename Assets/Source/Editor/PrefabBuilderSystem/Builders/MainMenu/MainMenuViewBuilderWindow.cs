#if UNITY_EDITOR
using Core.Editor.PrefabBuilderSystem.Core;
using Core.Editor.PrefabBuilderSystem.Settings;
using UnityEditor;
using UnityEngine;

namespace Core.Editor.PrefabBuilderSystem.Builders.MainMenu
{
    public class MainMenuViewBuilderWindow : BasePrefabBuilderWindow<MainMenuViewBuilder>
    {
        private const string SettingsPath = "Assets/Source/Editor/PrefabBuilderSystem/Builders/MainMenu/MainMenuViewBuilderSettings.asset";

        private MainMenuViewBuilderSettings _settings;
        private UnityEditor.Editor _settingsEditor;
        private Vector2 _scrollPosition;

        protected override string WindowTitle => "Main Menu View Builder";

        [MenuItem("Tools/UI Prefab Builder/Build Main Menu View")]
        public static void ShowWindow()
        {
            GetWindow<MainMenuViewBuilderWindow>("Main Menu View Builder");
        }

        [MenuItem("Tools/MVVM/Rebuild Main Menu Prefab")]
        public static void RebuildPrefab()
        {
            var settings = LoadOrCreateSettingsStatic<MainMenuViewBuilderSettings>(SettingsPath);
            var builder = new MainMenuViewBuilder { Settings = settings };
            builder.Build();
            Debug.Log("[MainMenuViewBuilder] MainMenuView.prefab rebuilt.");
        }

        private static TSettings LoadOrCreateSettingsStatic<TSettings>(string path)
            where TSettings : BuilderSettingsBase
        {
            var settings = AssetDatabase.LoadAssetAtPath<TSettings>(path);
            if (settings != null)
            {
                return settings;
            }

            settings = ScriptableObject.CreateInstance<TSettings>();
            settings.ResetToDefaults();
            var directory = System.IO.Path.GetDirectoryName(path);
            if (!string.IsNullOrEmpty(directory) && !AssetDatabase.IsValidFolder(directory))
            {
                System.IO.Directory.CreateDirectory(directory);
                AssetDatabase.Refresh();
            }

            AssetDatabase.CreateAsset(settings, path);
            AssetDatabase.SaveAssets();
            return settings;
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            _settings = LoadOrCreateSettings<MainMenuViewBuilderSettings>(SettingsPath);
            Builder.Settings = _settings;
        }

        protected override void OnGUI()
        {
            EditorGUILayout.LabelField("Main Menu View Builder", EditorStyles.boldLabel);
            EditorGUILayout.Space(5);

            DrawSettingsControls(_settings, SettingsPath);
            EditorGUILayout.Space(5);

            _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition);

            if (_settingsEditor == null || _settingsEditor.target != _settings)
                _settingsEditor = UnityEditor.Editor.CreateEditor(_settings);

            _settingsEditor.OnInspectorGUI();

            EditorGUILayout.EndScrollView();

            EditorGUILayout.Space(10);
            DrawValidationControls();
        }
    }
}
#endif
