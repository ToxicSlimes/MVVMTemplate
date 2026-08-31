#if UNITY_EDITOR
using Core.Editor.PrefabBuilderSystem.Core;
using UnityEditor;
using UnityEngine;

namespace Core.Editor.PrefabBuilderSystem.Builders.ConnectingOverlay
{
    public class ConnectingOverlayViewBuilderWindow : BasePrefabBuilderWindow<ConnectingOverlayViewBuilder>
    {
        private const string SettingsPath = "Assets/Source/Editor/PrefabBuilderSystem/Builders/ConnectingOverlay/ConnectingOverlayViewBuilderSettings.asset";

        private ConnectingOverlayViewBuilderSettings _settings;
        private UnityEditor.Editor _settingsEditor;
        private Vector2 _scrollPosition;

        protected override string WindowTitle => "Connecting Overlay View Builder";

        [MenuItem("Tools/UI Prefab Builder/Build Connecting Overlay View")]
        public static void ShowWindow()
        {
            GetWindow<ConnectingOverlayViewBuilderWindow>("Connecting Overlay View Builder");
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            _settings = LoadOrCreateSettings<ConnectingOverlayViewBuilderSettings>(SettingsPath);
            Builder.Settings = _settings;
        }

        protected override void OnGUI()
        {
            EditorGUILayout.LabelField("Connecting Overlay View Builder", EditorStyles.boldLabel);
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
