using UnityEngine;
using UnityEditor;
using TMPro;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;
using Core.Configs.DesignSystem;

namespace EndlessAdventure.Editor.DesignSystem
{
    public class ApplyDesignSystemEditor : EditorWindow
    {
        private DesignSystemSettings designSystem;
        private Vector2 scrollPosition;
        private bool applyToMenuSelection = true;
        private bool applyToChatView = true;
        private bool applyToImageGenView = true;
        private bool applyColors = true;
        private bool applyTypography = true;
        private bool applySpacing = false;

        private List<string> logs = new List<string>();

        [MenuItem("Tools/Design System/Apply Design to Views")]
        public static void ShowWindow()
        {
            var window = GetWindow<ApplyDesignSystemEditor>("Apply Design System");
            window.minSize = new Vector2(500, 600);
            window.Show();
        }

        private void OnEnable()
        {
            designSystem = Resources.Load<DesignSystemSettings>("DesignSystemSettings");
            if (designSystem == null)
            {
                AddLog("ERROR: DesignSystemSettings not found in Resources folder!", true);
            }
        }

        private void OnGUI()
        {
            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("Apply Design System to Views", EditorStyles.boldLabel);
            EditorGUILayout.Space(5);

            designSystem = (DesignSystemSettings)EditorGUILayout.ObjectField(
                "Design System Settings",
                designSystem,
                typeof(DesignSystemSettings),
                false
            );

            if (designSystem == null)
            {
                EditorGUILayout.HelpBox(
                    "DesignSystemSettings not assigned! Please assign it or create one in Resources folder.",
                    MessageType.Error
                );
                return;
            }

            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("Target Views", EditorStyles.boldLabel);
            applyToMenuSelection = EditorGUILayout.Toggle("MenuSelectionView", applyToMenuSelection);
            applyToChatView = EditorGUILayout.Toggle("ChatView", applyToChatView);
            applyToImageGenView = EditorGUILayout.Toggle("ImageGenView", applyToImageGenView);

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
            {
                EditorGUILayout.LabelField(log, EditorStyles.wordWrappedLabel);
            }
            EditorGUILayout.EndScrollView();

            EditorGUILayout.Space(10);
            if (GUILayout.Button("Clear Logs"))
            {
                logs.Clear();
            }
        }

        private void ApplyDesignSystem()
        {
            logs.Clear();
            AddLog("=== Starting Design System Application ===");

            if (designSystem.colors == null)
            {
                AddLog("ERROR: ColorPalette is not assigned!", true);
                return;
            }

            if (designSystem.typography == null)
            {
                AddLog("ERROR: TypographySettings is not assigned!", true);
                return;
            }

            if (applyToMenuSelection)
            {
                ApplyToMenuSelectionView();
            }

            if (applyToChatView)
            {
                ApplyToChatView();
            }

            if (applyToImageGenView)
            {
                ApplyToImageGenView();
            }

            AddLog("=== Design System Application Complete ===");
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        private void ApplyToMenuSelectionView()
        {
            AddLog("\n--- Applying to MenuSelectionView ---");

            var prefabPath = "Assets/Prefabs/Views/MenuSelectionView.prefab";
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);

            if (prefab == null)
            {
                AddLog($"ERROR: MenuSelectionView prefab not found at {prefabPath}", true);
                return;
            }

            var prefabContents = PrefabUtility.LoadPrefabContents(prefabPath);

            try
            {
                if (applyColors)
                {
                    var rootImage = prefabContents.GetComponent<Image>();
                    if (rootImage != null)
                    {
                        rootImage.color = designSystem.colors.deepSpace;
                        AddLog("✓ Applied Deep Space background");
                    }

                    var titleText = FindComponentByName<TMP_Text>(prefabContents, "Title");
                    if (titleText != null && applyTypography)
                    {
                        designSystem.typography.ApplyTextStyle(
                            titleText,
                            designSystem.typography.heading1,
                            designSystem.colors.textPrimary
                        );
                        AddLog("✓ Applied H1 style to Title");
                    }

                    var statusText = FindComponentByName<TMP_Text>(prefabContents, "Status");
                    if (statusText != null && applyTypography)
                    {
                        designSystem.typography.ApplyTextStyle(
                            statusText,
                            designSystem.typography.body,
                            designSystem.colors.textSecondary
                        );
                        AddLog("✓ Applied Body style to Status");
                    }

                    var chatButton = FindComponentInHierarchy<Button>(prefabContents, "ChatGroup/SelectButton");
                    if (chatButton != null)
                    {
                        var chatButtonImage = chatButton.GetComponent<Image>();
                        if (chatButtonImage != null)
                        {
                            chatButtonImage.color = designSystem.colors.indigo;
                            AddLog("✓ Applied Indigo to Chat Button");
                        }

                        var chatButtonText = chatButton.GetComponentInChildren<TMP_Text>();
                        if (chatButtonText != null)
                        {
                            chatButtonText.color = Color.white;
                        }
                    }

                    var imageGenButton = FindComponentInHierarchy<Button>(prefabContents, "ImageGroup/SelectButton");
                    if (imageGenButton != null)
                    {
                        var imageGenButtonImage = imageGenButton.GetComponent<Image>();
                        if (imageGenButtonImage != null)
                        {
                            imageGenButtonImage.color = designSystem.colors.pink;
                            AddLog("✓ Applied Pink to ImageGen Button");
                        }

                        var imageGenButtonText = imageGenButton.GetComponentInChildren<TMP_Text>();
                        if (imageGenButtonText != null)
                        {
                            imageGenButtonText.color = Color.white;
                        }
                    }
                }

                PrefabUtility.SaveAsPrefabAsset(prefabContents, prefabPath);
                AddLog("✓ MenuSelectionView saved successfully");
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(prefabContents);
            }
        }

        private void ApplyToChatView()
        {
            AddLog("\n--- Applying to ChatView ---");
            AddLog("NOTE: ChatView uses dynamic message rendering, colors should be applied in C# code");
            AddLog("Update ChatView.cs DisplayChatMessages() method with:");
            AddLog($"  User Bubble: DesignSystemSettings.Instance.colors.userBubbleStart");
            AddLog($"  AI Bubble: DesignSystemSettings.Instance.colors.aiBubble");
        }

        private void ApplyToImageGenView()
        {
            AddLog("\n--- Applying to ImageGenView ---");
            AddLog("NOTE: ImageGenView uses dynamic message rendering, colors should be applied in C# code");
            AddLog("Update ImageGenView.cs CreateMessageBubble() method with:");
            AddLog($"  User Bubble: DesignSystemSettings.Instance.colors.userBubbleStart");
            AddLog($"  AI Bubble: DesignSystemSettings.Instance.colors.aiBubble");
            AddLog($"  Error Bubble: DesignSystemSettings.Instance.colors.errorBubble");
        }

        private T FindComponentByName<T>(GameObject root, string name) where T : Component
        {
            var components = root.GetComponentsInChildren<T>(true);
            return components.FirstOrDefault(c => c.gameObject.name == name);
        }

        private T FindComponentInHierarchy<T>(GameObject root, string path) where T : Component
        {
            var parts = path.Split('/');
            Transform current = root.transform;

            foreach (var part in parts)
            {
                current = current.Find(part);
                if (current == null)
                {
                    return null;
                }
            }

            return current.GetComponent<T>();
        }

        private void AddLog(string message, bool isError = false)
        {
            var prefix = isError ? "[ERROR] " : "";
            logs.Add($"{prefix}{message}");
            Debug.Log($"{prefix}{message}");
        }
    }
}