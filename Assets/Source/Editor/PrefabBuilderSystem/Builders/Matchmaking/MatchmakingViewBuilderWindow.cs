#if UNITY_EDITOR
using Core.Editor.PrefabBuilderSystem.Core;
using UnityEditor;
using UnityEngine;

namespace Core.Editor.PrefabBuilderSystem.Builders.Matchmaking
{
    public class MatchmakingViewBuilderWindow : BasePrefabBuilderWindow<MatchmakingViewBuilder>
    {
        private const string SettingsPath = "Assets/Source/Editor/PrefabBuilderSystem/Builders/Matchmaking/MatchmakingViewBuilderSettings.asset";

        private MatchmakingViewBuilderSettings _settings;
        private UnityEditor.Editor _settingsEditor;
        private Vector2 _scrollPosition;

        protected override string WindowTitle => "Matchmaking View Builder";

        [MenuItem("Tools/UI Prefab Builder/Build Matchmaking View")]
        public static void ShowWindow()
        {
            GetWindow<MatchmakingViewBuilderWindow>("Matchmaking View Builder");
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            _settings = LoadOrCreateSettings<MatchmakingViewBuilderSettings>(SettingsPath);
            Builder.Settings = _settings;
        }

        protected override void OnGUI()
        {
            EditorGUILayout.LabelField("Matchmaking View Builder", EditorStyles.boldLabel);
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
