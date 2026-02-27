#if UNITY_EDITOR
using Core.Editor.PrefabBuilderSystem.Core;
using UnityEditor;
using UnityEngine;

namespace Core.Editor.PrefabBuilderSystem.Builders.Tooltip
{
    public class TooltipViewBuilderWindow : BasePrefabBuilderWindow<TooltipViewBuilder>
    {
        private const string SettingsPath = "Assets/Source/Editor/PrefabBuilderSystem/Builders/Tooltip/TooltipBuilderSettings.asset";

        private TooltipBuilderSettings _settings;
        private UnityEditor.Editor _settingsEditor;
        private Vector2 _scrollPosition;

        protected override string WindowTitle => "Tooltip Builder";

        [MenuItem("Tools/UI Prefab Builder/Build Tooltip Popup")]
        public static void ShowWindow()
        {
            GetWindow<TooltipViewBuilderWindow>("Tooltip Builder");
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            _settings = LoadOrCreateSettings<TooltipBuilderSettings>(SettingsPath);
            Builder.Settings = _settings;
        }

        protected override void OnGUI()
        {
            EditorGUILayout.LabelField("Tooltip Popup Builder", EditorStyles.boldLabel);
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
