#if UNITY_EDITOR
using Core.Editor.PrefabBuilderSystem.Core;
using UnityEditor;
using UnityEngine;

namespace Core.Editor.PrefabBuilderSystem.Builders.ConfirmPopup
{
    public class ConfirmPopupBuilderWindow : BasePrefabBuilderWindow<ConfirmPopupBuilder>
    {
        private const string SettingsPath = "Assets/Source/Editor/PrefabBuilderSystem/Builders/ConfirmPopup/ConfirmPopupBuilderSettings.asset";

        private ConfirmPopupBuilderSettings _settings;
        private UnityEditor.Editor _settingsEditor;
        private Vector2 _scrollPosition;

        protected override string WindowTitle => "Confirm Popup Builder";

        [MenuItem("Tools/UI Prefab Builder/Build Confirm Popup")]
        public static void ShowWindow()
        {
            GetWindow<ConfirmPopupBuilderWindow>("Confirm Popup Builder");
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            _settings = LoadOrCreateSettings<ConfirmPopupBuilderSettings>(SettingsPath);
            Builder.Settings = _settings;
        }

        protected override void OnGUI()
        {
            EditorGUILayout.LabelField("Confirm Popup Builder", EditorStyles.boldLabel);
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
