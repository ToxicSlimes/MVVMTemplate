#if UNITY_EDITOR
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace EndlessAdventure.Editor.PrefabBuilderSystem.Templates
{
    public class TemplateBuilderWindow : EditorWindow
    {
        private List<UITemplate> _templates = new();
        private UITemplate _selectedTemplate;
        private string _outputPath = "Assets/Prefabs/UI/Generated/";
        private string _prefabName = "NewUI";
        private Vector2 _listScrollPos;
        private Vector2 _previewScrollPos;
        private string _searchQuery = "";
        private string _selectedCategory = "All";
        private List<string> _categories = new() { "All" };

        [MenuItem("Tools/UI Prefab Builder/Template Builder")]
        public static void ShowWindow()
        {
            var window = GetWindow<TemplateBuilderWindow>("Template Builder");
            window.minSize = new Vector2(700, 500);
            window.Show();
        }

        private void OnEnable()
        {
            RefreshTemplates();
        }

        private void RefreshTemplates()
        {
            _templates.Clear();
            _categories = new List<string> { "All" };

            var guids = AssetDatabase.FindAssets("t:UITemplate");
            foreach (var guid in guids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var template = AssetDatabase.LoadAssetAtPath<UITemplate>(path);
                if (template != null)
                {
                    _templates.Add(template);
                    if (!string.IsNullOrEmpty(template.Category) && !_categories.Contains(template.Category))
                    {
                        _categories.Add(template.Category);
                    }
                }
            }

            _templates = _templates.OrderBy(t => t.Category).ThenBy(t => t.DisplayName).ToList();
        }

        private void OnGUI()
        {
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
            {
                if (GUILayout.Button("🔄 Refresh", EditorStyles.toolbarButton, GUILayout.Width(70)))
                    RefreshTemplates();

                GUILayout.Space(10);

                EditorGUILayout.LabelField("Category:", GUILayout.Width(60));
                var categoryIndex = _categories.IndexOf(_selectedCategory);
                var newCategoryIndex = EditorGUILayout.Popup(categoryIndex, _categories.ToArray(), EditorStyles.toolbarDropDown, GUILayout.Width(100));
                if (newCategoryIndex != categoryIndex)
                    _selectedCategory = _categories[newCategoryIndex];

                GUILayout.Space(10);

                EditorGUILayout.LabelField("Search:", GUILayout.Width(45));
                _searchQuery = EditorGUILayout.TextField(_searchQuery, EditorStyles.toolbarSearchField, GUILayout.Width(150));

                GUILayout.FlexibleSpace();

                if (GUILayout.Button("📁 Generate Built-in", EditorStyles.toolbarButton, GUILayout.Width(110)))
                    TemplateLibrary.GenerateBuiltInTemplates();
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            {
                DrawTemplateList();
                DrawTemplatePreview();
            }
            EditorGUILayout.EndHorizontal();
        }

        private void DrawTemplateList()
        {
            EditorGUILayout.BeginVertical(GUILayout.Width(250));
            {
                EditorGUILayout.LabelField("Templates", EditorStyles.boldLabel);
                EditorGUILayout.Space(4);

                _listScrollPos = EditorGUILayout.BeginScrollView(_listScrollPos, GUI.skin.box);
                {
                    var filteredTemplates = GetFilteredTemplates();

                    if (filteredTemplates.Count == 0)
                    {
                        EditorGUILayout.HelpBox("No templates found.\nClick 'Generate Built-in' to create templates.", MessageType.Info);
                    }

                    string currentCategory = null;

                    foreach (var template in filteredTemplates)
                    {
                        if (_selectedCategory == "All" && template.Category != currentCategory)
                        {
                            currentCategory = template.Category;
                            EditorGUILayout.LabelField(currentCategory ?? "Uncategorized", EditorStyles.miniLabel);
                        }

                        var isSelected = _selectedTemplate == template;
                        var style = isSelected ? "selectionRect" : "box";

                        EditorGUILayout.BeginHorizontal(style);
                        {
                            if (template.Icon != null)
                            {
                                GUILayout.Label(template.Icon.texture, GUILayout.Width(32), GUILayout.Height(32));
                            }
                            else
                            {
                                GUILayout.Label(GetIconForCategory(template.Category), GUILayout.Width(32), GUILayout.Height(32));
                            }

                            EditorGUILayout.BeginVertical();
                            {
                                EditorGUILayout.LabelField(template.DisplayName ?? template.name, EditorStyles.boldLabel);
                                EditorGUILayout.LabelField($"{template.NodeCount} nodes", EditorStyles.miniLabel);
                            }
                            EditorGUILayout.EndVertical();
                        }
                        EditorGUILayout.EndHorizontal();

                        if (Event.current.type == EventType.MouseDown &&
                            GUILayoutUtility.GetLastRect().Contains(Event.current.mousePosition))
                        {
                            _selectedTemplate = template;
                            _prefabName = template.DisplayName?.Replace(" ", "") ?? "NewUI";
                            Repaint();
                        }
                    }
                }
                EditorGUILayout.EndScrollView();
            }
            EditorGUILayout.EndVertical();
        }

        private void DrawTemplatePreview()
        {
            EditorGUILayout.BeginVertical();
            {
                if (_selectedTemplate == null)
                {
                    EditorGUILayout.HelpBox("Select a template from the list", MessageType.Info);
                }
                else
                {
                    EditorGUILayout.LabelField(_selectedTemplate.DisplayName ?? _selectedTemplate.name, EditorStyles.boldLabel);
                    EditorGUILayout.LabelField(_selectedTemplate.Description ?? "No description", EditorStyles.wordWrappedLabel);

                    EditorGUILayout.Space(8);

                    EditorGUILayout.BeginHorizontal();
                    {
                        EditorGUILayout.LabelField("Category:", GUILayout.Width(60));
                        EditorGUILayout.LabelField(_selectedTemplate.Category ?? "None");
                    }
                    EditorGUILayout.EndHorizontal();

                    if (_selectedTemplate.Tags.Count > 0)
                    {
                        EditorGUILayout.BeginHorizontal();
                        {
                            EditorGUILayout.LabelField("Tags:", GUILayout.Width(60));
                            EditorGUILayout.LabelField(string.Join(", ", _selectedTemplate.Tags));
                        }
                        EditorGUILayout.EndHorizontal();
                    }

                    EditorGUILayout.Space(8);
                    EditorGUILayout.LabelField("Node Tree", EditorStyles.boldLabel);

                    _previewScrollPos = EditorGUILayout.BeginScrollView(_previewScrollPos, GUI.skin.box, GUILayout.Height(200));
                    {
                        DrawNodeTree(_selectedTemplate.Root, 0);
                    }
                    EditorGUILayout.EndScrollView();

                    EditorGUILayout.Space(16);
                    EditorGUILayout.LabelField("Generate Prefab", EditorStyles.boldLabel);

                    EditorGUILayout.BeginHorizontal();
                    {
                        EditorGUILayout.LabelField("Output Path:", GUILayout.Width(80));
                        _outputPath = EditorGUILayout.TextField(_outputPath);
                        if (GUILayout.Button("...", GUILayout.Width(30)))
                        {
                            var folder = EditorUtility.SaveFolderPanel("Select Output Folder", _outputPath, "");
                            if (!string.IsNullOrEmpty(folder))
                            {
                                if (folder.StartsWith(Application.dataPath))
                                {
                                    _outputPath = "Assets" + folder.Substring(Application.dataPath.Length) + "/";
                                }
                            }
                        }
                    }
                    EditorGUILayout.EndHorizontal();

                    EditorGUILayout.BeginHorizontal();
                    {
                        EditorGUILayout.LabelField("Prefab Name:", GUILayout.Width(80));
                        _prefabName = EditorGUILayout.TextField(_prefabName);
                    }
                    EditorGUILayout.EndHorizontal();

                    EditorGUILayout.Space(8);

                    EditorGUILayout.BeginHorizontal();
                    {
                        GUI.backgroundColor = Color.cyan;
                        if (GUILayout.Button("👁 Preview in Scene", GUILayout.Height(30)))
                        {
                            PreviewInScene();
                        }

                        GUI.backgroundColor = Color.green;
                        if (GUILayout.Button("💾 Generate Prefab", GUILayout.Height(30)))
                        {
                            GeneratePrefab();
                        }
                        GUI.backgroundColor = Color.white;
                    }
                    EditorGUILayout.EndHorizontal();
                }
            }
            EditorGUILayout.EndVertical();
        }

        private void DrawNodeTree(UITemplateNode node, int depth)
        {
            if (node == null) return;

            var indent = new string(' ', depth * 4);
            var icon = GetIconForElementType(node.ElementType);
            var style = node.UseDesignSystem ? "" : " (custom)";

            EditorGUILayout.LabelField($"{indent}{icon} {node.Name} [{node.ElementType}]{style}");

            foreach (var child in node.Children)
            {
                DrawNodeTree(child, depth + 1);
            }
        }

        private List<UITemplate> GetFilteredTemplates()
        {
            var filtered = _templates.AsEnumerable();

            if (_selectedCategory != "All")
            {
                filtered = filtered.Where(t => t.Category == _selectedCategory);
            }

            if (!string.IsNullOrEmpty(_searchQuery))
            {
                var query = _searchQuery.ToLower();
                filtered = filtered.Where(t =>
                    (t.DisplayName?.ToLower().Contains(query) ?? false) ||
                    (t.Description?.ToLower().Contains(query) ?? false) ||
                    t.Tags.Any(tag => tag.ToLower().Contains(query)));
            }

            return filtered.ToList();
        }

        private void PreviewInScene()
        {
            if (_selectedTemplate == null) return;

            var existing = GameObject.Find("[Template Preview]");
            if (existing != null)
                DestroyImmediate(existing);

            var builder = new TemplateBuilder(_selectedTemplate);
            var go = builder.Build();
            go.name = "[Template Preview]";

            Selection.activeGameObject = go;
            Debug.Log($"[TemplateBuilder] Preview created: {_selectedTemplate.DisplayName}");
        }

        private void GeneratePrefab()
        {
            if (_selectedTemplate == null) return;

            var path = $"{_outputPath}{_prefabName}.prefab";
            var builder = new TemplateBuilder(_selectedTemplate);
            var prefab = builder.SaveAsPrefab(path);

            if (prefab != null)
            {
                Selection.activeObject = prefab;
                EditorGUIUtility.PingObject(prefab);
            }
        }

        private string GetIconForElementType(UIElementType type)
        {
            return type switch
            {
                UIElementType.Container => "📦",
                UIElementType.VerticalLayout => "⬇️",
                UIElementType.HorizontalLayout => "➡️",
                UIElementType.Text => "📝",
                UIElementType.Image => "🖼️",
                UIElementType.Button => "🔘",
                UIElementType.InputField => "📥",
                UIElementType.Toggle => "☑️",
                UIElementType.Slider => "🎚️",
                UIElementType.Dropdown => "📋",
                UIElementType.ScrollView => "📜",
                UIElementType.Grid => "🔲",
                UIElementType.Spacer => "⬜",
                _ => "❓"
            };
        }

        private string GetIconForCategory(string category)
        {
            return category?.ToLower() switch
            {
                "dialogs" => "💬",
                "chat" => "💬",
                "components" => "🧩",
                "forms" => "📝",
                "navigation" => "🧭",
                "lists" => "📋",
                "feedback" => "⚠️",
                _ => "📁"
            };
        }
    }
}
#endif
