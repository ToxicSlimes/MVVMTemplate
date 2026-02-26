#if UNITY_EDITOR
using Editor.PrefabBuilderSystem.Base.Validation.Validators;
using UnityEditor;
using UnityEngine;

namespace EndlessAdventure.Editor.PrefabBuilderSystem.Validation
{
    public class ValidationSettingsWindow : EditorWindow
    {
        private Vector2 _scrollPosition;

        [MenuItem("Tools/UI Prefab Builder/Validation Settings")]
        public static void ShowWindow()
        {
            var window = GetWindow<ValidationSettingsWindow>();
            window.titleContent = new GUIContent("Validation Settings");
            window.minSize = new Vector2(350, 400);
            window.Show();
        }

        private void OnGUI()
        {
            EditorGUILayout.LabelField("Validation Settings", EditorStyles.boldLabel);
            EditorGUILayout.Space();

            var system = UIValidationSystem.Instance;

            EditorGUILayout.LabelField("Global Settings", EditorStyles.boldLabel);
            system.ValidateBeforeBuild = EditorGUILayout.Toggle("Validate Before Build", system.ValidateBeforeBuild);
            system.BlockBuildOnError = EditorGUILayout.Toggle("Block Build on Error", system.BlockBuildOnError);
            system.BlockBuildOnCritical = EditorGUILayout.Toggle("Block Build on Critical", system.BlockBuildOnCritical);

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Validators", EditorStyles.boldLabel);

            _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition);

            foreach (var validator in system.GetAllValidators())
            {
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);

                EditorGUILayout.BeginHorizontal();
                validator.IsEnabled = EditorGUILayout.Toggle(validator.IsEnabled, GUILayout.Width(20));
                EditorGUILayout.LabelField(validator.Name, EditorStyles.boldLabel);
                EditorGUILayout.EndHorizontal();

                EditorGUI.indentLevel++;
                EditorGUILayout.LabelField(validator.Description, EditorStyles.wordWrappedMiniLabel);

                switch (validator)
                {
                    case HierarchyValidator hv:
                        hv.MaxDepth = EditorGUILayout.IntField("Max Depth", hv.MaxDepth);
                        hv.MaxChildren = EditorGUILayout.IntField("Max Children", hv.MaxChildren);
                        break;
                    case NamingValidator nv:
                        nv.RequirePrefix = EditorGUILayout.Toggle("Require Prefix", nv.RequirePrefix);
                        nv.WarnOnMissingPrefix = EditorGUILayout.Toggle("Warn on Missing Prefix", nv.WarnOnMissingPrefix);
                        break;
                }

                EditorGUI.indentLevel--;

                EditorGUILayout.EndVertical();
                EditorGUILayout.Space(2);
            }

            EditorGUILayout.EndScrollView();

            EditorGUILayout.Space();

            if (GUILayout.Button("Reset All to Defaults"))
            {
                system.ValidateBeforeBuild = true;
                system.BlockBuildOnError = true;
                system.BlockBuildOnCritical = true;

                foreach (var validator in system.GetAllValidators())
                {
                    validator.IsEnabled = true;
                }
            }
        }
    }
}
#endif
