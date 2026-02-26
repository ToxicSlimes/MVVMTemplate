#if UNITY_EDITOR
using System;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace EndlessAdventure.Editor.PrefabBuilderSystem.Wizard
{
    public class PrefabBuilderWizard : EditorWindow
    {
        private string _viewName = "";
        private string _description = "";
        private string _prefabPath = "Assets/Prefabs/Views/";
        private int _categoryIndex;
        private bool _generateSettings = true;
        private bool _generateBuilder = true;
        private bool _generateWindow = true;
        private bool _useGenericViewBuilder;
        private Vector2 _previewScrollPosition;
        private bool _showPreview = true;

        private static readonly string[] Categories = { "General", "Game UI", "Menu", "HUD", "Popup", "Custom" };

        private const string BasePath = "Assets/Source/Editor/PrefabBuilderSystem";

        [MenuItem("Tools/UI Prefab Builder/Create New Builder... %#n", false, 10)]
        public static void ShowWindow()
        {
            var window = GetWindow<PrefabBuilderWizard>("Create New Builder");
            window.minSize = new Vector2(500, 600);
            window.Show();
        }

        private void OnGUI()
        {
            DrawHeader();
            DrawSettings();
            DrawPreview();
            DrawButtons();
        }

        private void DrawHeader()
        {
            EditorGUILayout.Space(10);

            GUILayout.Label("✨ Create New Prefab Builder", new GUIStyle(EditorStyles.boldLabel)
            {
                fontSize = 16,
                alignment = TextAnchor.MiddleCenter
            });

            EditorGUILayout.Space(5);

            EditorGUILayout.HelpBox(
                "Wizard автоматически сгенерирует Settings, Builder и Window классы.\n" +
                "Builder автоматически появится в Hub благодаря [HubEntry] атрибуту.",
                MessageType.Info);

            EditorGUILayout.Space(10);
        }

        private void DrawSettings()
        {
            GUILayout.Label("📝 Basic Settings", EditorStyles.boldLabel);

            EditorGUILayout.Space(5);

            _viewName = EditorGUILayout.TextField("View Name", _viewName);
            if (!string.IsNullOrEmpty(_viewName) && !char.IsUpper(_viewName[0]))
            {
                EditorGUILayout.HelpBox("View Name должен начинаться с заглавной буквы", MessageType.Warning);
            }

            _categoryIndex = EditorGUILayout.Popup("Category", _categoryIndex, Categories);
            _description = EditorGUILayout.TextField("Description", _description);
            _prefabPath = EditorGUILayout.TextField("Prefab Path", _prefabPath);

            if (!_prefabPath.EndsWith("/") && !_prefabPath.EndsWith(".prefab"))
            {
                _prefabPath += "/";
            }

            EditorGUILayout.Space(10);

            GUILayout.Label("⚙️ Generation Options", EditorStyles.boldLabel);

            EditorGUILayout.Space(5);

            _generateSettings = EditorGUILayout.Toggle("Generate Settings", _generateSettings);
            _generateBuilder = EditorGUILayout.Toggle("Generate Builder", _generateBuilder);
            _generateWindow = EditorGUILayout.Toggle("Generate Window", _generateWindow);

            EditorGUI.BeginDisabledGroup(!_generateBuilder);
            _useGenericViewBuilder = EditorGUILayout.Toggle("Use Generic ViewBuilder<T>", _useGenericViewBuilder);
            EditorGUI.EndDisabledGroup();

            EditorGUILayout.Space(10);
        }

        private void DrawPreview()
        {
            _showPreview = EditorGUILayout.Foldout(_showPreview, "📄 Code Preview", true);

            if (!_showPreview)
                return;

            if (string.IsNullOrEmpty(_viewName))
            {
                EditorGUILayout.HelpBox("Введите View Name для preview", MessageType.Info);
                return;
            }

            _previewScrollPosition = EditorGUILayout.BeginScrollView(_previewScrollPosition, GUILayout.Height(250));

            var preview = new StringBuilder();

            if (_generateSettings)
            {
                preview.AppendLine("// " + GetSettingsFileName());
                preview.AppendLine(GenerateSettingsCode());
                preview.AppendLine();
            }

            if (_generateBuilder)
            {
                preview.AppendLine("// " + GetBuilderFileName());
                preview.AppendLine(GenerateBuilderCode());
            }

            EditorGUILayout.TextArea(preview.ToString(), new GUIStyle(EditorStyles.textArea)
            {
                wordWrap = false,
                richText = false
            });

            EditorGUILayout.EndScrollView();
        }

        private void DrawButtons()
        {
            EditorGUILayout.Space(10);

            var canGenerate = !string.IsNullOrEmpty(_viewName) &&
                              char.IsUpper(_viewName[0]) &&
                              (_generateSettings || _generateBuilder || _generateWindow);

            EditorGUI.BeginDisabledGroup(!canGenerate);

            GUILayout.BeginHorizontal();

            if (GUILayout.Button("Cancel", GUILayout.Height(30)))
            {
                Close();
            }

            GUI.backgroundColor = new Color(0.3f, 0.8f, 0.3f);
            if (GUILayout.Button("Generate", GUILayout.Height(30)))
            {
                Generate();
            }
            GUI.backgroundColor = Color.white;

            GUILayout.EndHorizontal();

            EditorGUI.EndDisabledGroup();

            if (!canGenerate && !string.IsNullOrEmpty(_viewName))
            {
                EditorGUILayout.HelpBox("Выберите хотя бы один файл для генерации", MessageType.Warning);
            }
        }

        private void Generate()
        {
            try
            {
                if (_generateSettings)
                    GenerateSettingsFile();

                if (_generateBuilder)
                    GenerateBuilderFile();

                if (_generateWindow)
                    GenerateWindowFile();

                AssetDatabase.Refresh();

                EditorUtility.DisplayDialog(
                    "Generation Complete",
                    $"Files for {_viewName} have been generated successfully!\n\n" +
                    "Builder will appear in Hub after Unity recompiles.",
                    "OK");

                Close();
            }
            catch (Exception e)
            {
                EditorUtility.DisplayDialog("Generation Error", e.Message, "OK");
                Debug.LogException(e);
            }
        }

        private void GenerateSettingsFile()
        {
            var directory = $"{BasePath}/Base/Settings/{_viewName}";
            EnsureDirectoryExists(directory);

            var path = $"{directory}/{GetSettingsFileName()}";
            File.WriteAllText(path, GenerateSettingsCode());
            Debug.Log($"<color=cyan>Generated:</color> {path}");
        }

        private void GenerateBuilderFile()
        {
            var directory = $"{BasePath}/Builders/{_viewName}";
            EnsureDirectoryExists(directory);

            var path = $"{directory}/{GetBuilderFileName()}";
            File.WriteAllText(path, GenerateBuilderCode());
            Debug.Log($"<color=cyan>Generated:</color> {path}");
        }

        private void GenerateWindowFile()
        {
            var directory = $"{BasePath}/Builders/{_viewName}";
            EnsureDirectoryExists(directory);

            var path = $"{directory}/{GetWindowFileName()}";
            File.WriteAllText(path, GenerateWindowCode());
            Debug.Log($"<color=cyan>Generated:</color> {path}");
        }

        private string GetSettingsFileName() => $"{_viewName}BuilderSettings.cs";
        private string GetBuilderFileName() => $"{_viewName}PrefabBuilder.cs";
        private string GetWindowFileName() => $"{_viewName}PrefabBuilderWindow.cs";

        private string GenerateSettingsCode()
        {
            var sb = new StringBuilder();

            sb.AppendLine("#if UNITY_EDITOR");
            sb.AppendLine("using UnityEngine;");
            sb.AppendLine("using EndlessAdventure.Editor.PrefabBuilderSystem.Settings;");
            sb.AppendLine();
            sb.AppendLine($"namespace EndlessAdventure.Editor.PrefabBuilderSystem.{_viewName}");
            sb.AppendLine("{");
            sb.AppendLine($"    [CreateAssetMenu(fileName = \"{_viewName}BuilderSettings\", menuName = \"PrefabBuilder/Settings/{_viewName}\")]");
            sb.AppendLine($"    public class {_viewName}BuilderSettings : BuilderSettingsBase");
            sb.AppendLine("    {");
            sb.AppendLine("        public float PanelHeight = 100f;");
            sb.AppendLine("        public float Padding = 10f;");
            sb.AppendLine("        public float Spacing = 5f;");
            sb.AppendLine();
            sb.AppendLine("        public Color PanelColor = new(0.15f, 0.15f, 0.15f, 0.9f);");
            sb.AppendLine("        public Color BackgroundColor = new(0.2f, 0.2f, 0.2f, 0.8f);");
            sb.AppendLine("        public Color TextColor = Color.white;");
            sb.AppendLine();
            sb.AppendLine("        public override void ResetToDefaults()");
            sb.AppendLine("        {");
            sb.AppendLine("            PanelHeight = 100f;");
            sb.AppendLine("            Padding = 10f;");
            sb.AppendLine("            Spacing = 5f;");
            sb.AppendLine("            PanelColor = new Color(0.15f, 0.15f, 0.15f, 0.9f);");
            sb.AppendLine("            BackgroundColor = new Color(0.2f, 0.2f, 0.2f, 0.8f);");
            sb.AppendLine("            TextColor = Color.white;");
            sb.AppendLine("        }");
            sb.AppendLine("    }");
            sb.AppendLine("}");
            sb.AppendLine("#endif");

            return sb.ToString();
        }

        private string GenerateBuilderCode()
        {
            var sb = new StringBuilder();
            var category = Categories[_categoryIndex];
            var prefabFullPath = _prefabPath.EndsWith(".prefab")
                ? _prefabPath
                : $"{_prefabPath}{_viewName}.prefab";

            sb.AppendLine("#if UNITY_EDITOR");
            sb.AppendLine("using UnityEngine;");
            sb.AppendLine("using UnityEngine.UI;");
            sb.AppendLine("using EndlessAdventure.Editor.PrefabBuilderSystem.Core;");
            sb.AppendLine("using EndlessAdventure.Editor.PrefabBuilderSystem.Attributes;");
            sb.AppendLine();
            sb.AppendLine($"namespace EndlessAdventure.Editor.PrefabBuilderSystem.{_viewName}");
            sb.AppendLine("{");
            sb.AppendLine($"    [HubEntry(\"{_viewName}\", \"{EscapeString(_description)}\", \"{category}\", Order = 100)]");
            sb.AppendLine($"    [BuilderWindow(typeof({_viewName}PrefabBuilderWindow))]");

            if (_useGenericViewBuilder)
            {
                sb.AppendLine($"    public class {_viewName}StructureBuilder : ViewBuilder<{_viewName}View>");
            }
            else
            {
                sb.AppendLine($"    public class {_viewName}StructureBuilder : ViewBuilder");
            }

            sb.AppendLine("    {");
            sb.AppendLine($"        protected override string PrefabPath => \"{prefabFullPath}\";");
            sb.AppendLine();
            sb.AppendLine($"        public {_viewName}BuilderSettings Settings {{ get; }} = new();");
            sb.AppendLine();

            if (_useGenericViewBuilder)
            {
                sb.AppendLine("        protected override void BuildViewStructure()");
            }
            else
            {
                sb.AppendLine("        protected override void BuildStructure()");
            }

            sb.AppendLine("        {");
            sb.AppendLine("            var mainLayout = CreateChild(Context.Root.transform, \"Main_VerticalLayout\", stretch: true);");
            sb.AppendLine("            var vlg = mainLayout.gameObject.AddComponent<VerticalLayoutGroup>();");
            sb.AppendLine("            vlg.childControlWidth = true;");
            sb.AppendLine("            vlg.childControlHeight = true;");
            sb.AppendLine("            vlg.childForceExpandWidth = true;");
            sb.AppendLine("            vlg.childForceExpandHeight = false;");
            sb.AppendLine("            vlg.spacing = Settings.Spacing;");
            sb.AppendLine("            vlg.padding = new RectOffset((int)Settings.Padding, (int)Settings.Padding, (int)Settings.Padding, (int)Settings.Padding);");
            sb.AppendLine("        }");
            sb.AppendLine();
            sb.AppendLine("        private RectTransform CreateChild(Transform parent, string name, bool stretch = false)");
            sb.AppendLine("        {");
            sb.AppendLine("            var go = new GameObject(name);");
            sb.AppendLine("            go.transform.SetParent(parent, false);");
            sb.AppendLine("            var rt = go.AddComponent<RectTransform>();");
            sb.AppendLine();
            sb.AppendLine("            if (stretch)");
            sb.AppendLine("            {");
            sb.AppendLine("                rt.anchorMin = Vector2.zero;");
            sb.AppendLine("                rt.anchorMax = Vector2.one;");
            sb.AppendLine("                rt.offsetMin = Vector2.zero;");
            sb.AppendLine("                rt.offsetMax = Vector2.zero;");
            sb.AppendLine("            }");
            sb.AppendLine();
            sb.AppendLine("            return rt;");
            sb.AppendLine("        }");
            sb.AppendLine("    }");
            sb.AppendLine("}");
            sb.AppendLine("#endif");

            return sb.ToString();
        }

        private string GenerateWindowCode()
        {
            var sb = new StringBuilder();

            sb.AppendLine("#if UNITY_EDITOR");
            sb.AppendLine("using UnityEditor;");
            sb.AppendLine("using UnityEngine;");
            sb.AppendLine("using EndlessAdventure.Editor.PrefabBuilderSystem.Core;");
            sb.AppendLine();
            sb.AppendLine($"namespace EndlessAdventure.Editor.PrefabBuilderSystem.{_viewName}");
            sb.AppendLine("{");
            sb.AppendLine($"    public class {_viewName}PrefabBuilderWindow : BasePrefabBuilderWindow<{_viewName}StructureBuilder>");
            sb.AppendLine("    {");
            sb.AppendLine($"        protected override string WindowTitle => \"{_viewName} Builder\";");
            sb.AppendLine("        protected override Vector2 MinWindowSize => new(450, 600);");
            sb.AppendLine();
            sb.AppendLine($"        [MenuItem(\"Tools/UI Prefab Builder/{_viewName} Builder\", false, 101)]");
            sb.AppendLine($"        public static void ShowWindow() => GetWindow<{_viewName}PrefabBuilderWindow>().Show();");
            sb.AppendLine();
            sb.AppendLine("        protected override void OnGUI()");
            sb.AppendLine("        {");
            sb.AppendLine("            DrawHeader();");
            sb.AppendLine("            DrawSettings();");
            sb.AppendLine("            DrawSettingsControls(Builder.Settings);");
            sb.AppendLine("            GUILayout.Space(10);");
            sb.AppendLine("            DrawBuildButton();");
            sb.AppendLine("        }");
            sb.AppendLine();
            sb.AppendLine("        private void DrawHeader()");
            sb.AppendLine("        {");
            sb.AppendLine($"            GUILayout.Label(\"{_viewName} Builder\", EditorStyles.boldLabel);");
            sb.AppendLine("            EditorGUILayout.HelpBox(");
            sb.AppendLine($"                \"{EscapeString(_description)}\",");
            sb.AppendLine("                MessageType.Info);");
            sb.AppendLine("            GUILayout.Space(10);");
            sb.AppendLine("        }");
            sb.AppendLine();
            sb.AppendLine("        private void DrawSettings()");
            sb.AppendLine("        {");
            sb.AppendLine("            GUILayout.Label(\"Settings\", EditorStyles.boldLabel);");
            sb.AppendLine();
            sb.AppendLine("            var s = Builder.Settings;");
            sb.AppendLine("            s.PanelHeight = EditorGUILayout.FloatField(\"Panel Height\", s.PanelHeight);");
            sb.AppendLine("            s.Padding = EditorGUILayout.FloatField(\"Padding\", s.Padding);");
            sb.AppendLine("            s.Spacing = EditorGUILayout.FloatField(\"Spacing\", s.Spacing);");
            sb.AppendLine();
            sb.AppendLine("            GUILayout.Space(10);");
            sb.AppendLine("            GUILayout.Label(\"Colors\", EditorStyles.boldLabel);");
            sb.AppendLine("            s.PanelColor = EditorGUILayout.ColorField(\"Panel Color\", s.PanelColor);");
            sb.AppendLine("            s.BackgroundColor = EditorGUILayout.ColorField(\"Background Color\", s.BackgroundColor);");
            sb.AppendLine("            s.TextColor = EditorGUILayout.ColorField(\"Text Color\", s.TextColor);");
            sb.AppendLine();
            sb.AppendLine("            GUILayout.Space(10);");
            sb.AppendLine("        }");
            sb.AppendLine();
            sb.AppendLine("        private void DrawBuildButton()");
            sb.AppendLine("        {");
            sb.AppendLine("            GUI.backgroundColor = Color.green;");
            sb.AppendLine($"            if (GUILayout.Button(\"BUILD {_viewName.ToUpper()}\", GUILayout.Height(50)))");
            sb.AppendLine("            {");
            sb.AppendLine("                BuildPrefab();");
            sb.AppendLine("            }");
            sb.AppendLine("            GUI.backgroundColor = Color.white;");
            sb.AppendLine("        }");
            sb.AppendLine("    }");
            sb.AppendLine("}");
            sb.AppendLine("#endif");

            return sb.ToString();
        }

        private static string EscapeString(string input)
        {
            if (string.IsNullOrEmpty(input))
                return "";

            return input
                .Replace("\\", "\\\\")
                .Replace("\"", "\\\"")
                .Replace("\n", "\\n")
                .Replace("\r", "\\r");
        }

        private static void EnsureDirectoryExists(string path)
        {
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);
        }
    }
}
#endif
