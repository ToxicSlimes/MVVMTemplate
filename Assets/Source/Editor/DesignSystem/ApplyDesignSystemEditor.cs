#if UNITY_EDITOR
using Core.Presentation.UI.DesignSystem;
using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

namespace Core.Editor.DesignSystem
{
    public class ApplyDesignSystemEditor : EditorWindow
    {
        private DesignSystemSettings designSystem;
        private Vector2 scrollPosition;
        private bool applyColors = true;
        private bool applyTypography = true;
        private bool applySpacing;
        private readonly List<string> logs = new();

        [MenuItem("Tools/Design System/Apply Design System")]
        public static void ShowWindow()
        {
            GetWindow<ApplyDesignSystemEditor>("Apply Design System");
        }

        private void OnGUI()
        {
            EditorGUILayout.LabelField("Design System Applicator", EditorStyles.boldLabel);
            EditorGUILayout.Space(5);

            designSystem = (DesignSystemSettings)EditorGUILayout.ObjectField(
                "Design System", designSystem, typeof(DesignSystemSettings), false);

            if (designSystem == null)
            {
                EditorGUILayout.HelpBox("Assign a DesignSystemSettings asset to begin.", MessageType.Info);
                return;
            }

            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("Apply Settings", EditorStyles.boldLabel);
            applyColors = EditorGUILayout.Toggle("Apply Colors", applyColors);
            applyTypography = EditorGUILayout.Toggle("Apply Typography", applyTypography);
            applySpacing = EditorGUILayout.Toggle("Apply Spacing (Experimental)", applySpacing);

            EditorGUILayout.Space(10);
            if (GUILayout.Button("Apply Design System", GUILayout.Height(40)))
            {
                ApplyDesignSystem();
            }

            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("Logs", EditorStyles.boldLabel);
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition, GUILayout.Height(300));
            foreach (var log in logs)
                EditorGUILayout.LabelField(log, EditorStyles.wordWrappedLabel);
            EditorGUILayout.EndScrollView();

            if (GUILayout.Button("Clear Logs"))
                logs.Clear();
        }

        private void ApplyDesignSystem()
        {
            logs.Clear();
            logs.Add("=== Starting Design System Application ===");

            if (designSystem.colors == null)
            {
                logs.Add("ERROR: ColorPalette is not assigned!");
                return;
            }

            if (designSystem.typography == null)
            {
                logs.Add("ERROR: TypographySettings is not assigned!");
                return;
            }

            // TEMPLATE: Add your view-specific design system application logic here.
            // Example: find prefabs, iterate TMP_Text components, apply fonts/colors.

            logs.Add("=== Design System Application Complete ===");
            logs.Add("TEMPLATE: Add your view prefab paths and styling logic.");
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }
    }
}
#endif
