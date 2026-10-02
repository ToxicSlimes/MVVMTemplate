#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Core.Editor.PrefabBuilderSystem.Conventions;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace Core.Editor.PrefabBuilderSystem.Tools
{
    public class ViewStubGenerator : EditorWindow
    {
        private GameObject _sourcePrefab;
        private string _viewName = "NewView";
        private string _namespace = "Core.Presentation.MVVM.Views";
        private string _outputPath = "Assets/Source/Runtime/Core/Presentation/MVVM/Views";
        private List<ElementInfo> _elements = new();
        private Dictionary<ElementInfo, bool> _selectedElements = new();
        private Vector2 _elementsScrollPos;
        private Vector2 _previewScrollPos;
        private string _generatedCode = "";
        private bool _showPreview;

        [MenuItem("Tools/UI Prefab Builder/Generate View Stub")]
        public static void ShowWindow()
        {
            var window = GetWindow<ViewStubGenerator>("View Stub Generator");
            window.minSize = new Vector2(500, 600);
        }

        private void OnGUI()
        {
            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("View Stub Generator", EditorStyles.boldLabel);
            EditorGUILayout.Space(5);

            DrawSettings();
            EditorGUILayout.Space(10);

            if (_sourcePrefab != null)
            {
                DrawElementsList();
                EditorGUILayout.Space(10);
                DrawPreview();
                EditorGUILayout.Space(10);
                DrawGenerateButton();
            }
        }

        private void DrawSettings()
        {
            EditorGUI.BeginChangeCheck();
            _sourcePrefab = (GameObject)EditorGUILayout.ObjectField("Source Prefab", _sourcePrefab, typeof(GameObject), false);
            if (EditorGUI.EndChangeCheck() && _sourcePrefab != null)
            {
                ScanPrefab();
            }

            _viewName = EditorGUILayout.TextField("View Name", _viewName);
            _namespace = EditorGUILayout.TextField("Namespace", _namespace);

            EditorGUILayout.BeginHorizontal();
            _outputPath = EditorGUILayout.TextField("Output Path", _outputPath);
            if (GUILayout.Button("...", GUILayout.Width(30)))
            {
                var path = EditorUtility.OpenFolderPanel("Select Output Folder", _outputPath, "");
                if (!string.IsNullOrEmpty(path))
                {
                    if (path.StartsWith(UnityEngine.Application.dataPath))
                    {
                        _outputPath = "Assets" + path.Substring(UnityEngine.Application.dataPath.Length);
                    }
                }
            }
            EditorGUILayout.EndHorizontal();
        }

        private void DrawElementsList()
        {
            EditorGUILayout.LabelField($"Found Elements ({_elements.Count})", EditorStyles.boldLabel);

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Select All"))
            {
                foreach (var element in _elements)
                    _selectedElements[element] = true;
                UpdatePreview();
            }
            if (GUILayout.Button("Deselect All"))
            {
                foreach (var element in _elements)
                    _selectedElements[element] = false;
                UpdatePreview();
            }
            if (GUILayout.Button("Refresh"))
            {
                ScanPrefab();
            }
            EditorGUILayout.EndHorizontal();

            _elementsScrollPos = EditorGUILayout.BeginScrollView(_elementsScrollPos, GUILayout.Height(200));
            
            foreach (var element in _elements)
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUI.BeginChangeCheck();
                _selectedElements[element] = EditorGUILayout.Toggle(_selectedElements[element], GUILayout.Width(20));
                if (EditorGUI.EndChangeCheck())
                {
                    UpdatePreview();
                }
                EditorGUILayout.LabelField(element.Name, GUILayout.Width(200));
                EditorGUILayout.LabelField("→", GUILayout.Width(20));
                EditorGUILayout.LabelField(element.FieldName, EditorStyles.miniLabel);
                EditorGUILayout.LabelField($"({element.ComponentType.Name})", EditorStyles.miniLabel, GUILayout.Width(120));
                EditorGUILayout.EndHorizontal();
            }
            
            EditorGUILayout.EndScrollView();
        }

        private void DrawPreview()
        {
            _showPreview = EditorGUILayout.Foldout(_showPreview, "Code Preview");
            if (_showPreview)
            {
                _previewScrollPos = EditorGUILayout.BeginScrollView(_previewScrollPos, GUILayout.Height(250));
                EditorGUILayout.TextArea(_generatedCode, EditorStyles.textArea, GUILayout.ExpandHeight(true));
                EditorGUILayout.EndScrollView();
            }
        }

        private void DrawGenerateButton()
        {
            var selectedCount = _selectedElements.Count(kv => kv.Value);
            EditorGUI.BeginDisabledGroup(selectedCount == 0 || string.IsNullOrEmpty(_viewName));
            if (GUILayout.Button($"Generate View ({selectedCount} fields)", GUILayout.Height(30)))
            {
                GenerateViewFile();
            }
            EditorGUI.EndDisabledGroup();
        }

        private void ScanPrefab()
        {
            _elements.Clear();
            _selectedElements.Clear();

            if (_sourcePrefab == null) return;

            ScanRecursive(_sourcePrefab.transform, _elements);

            foreach (var element in _elements)
            {
                _selectedElements[element] = true;
            }

            if (string.IsNullOrEmpty(_viewName) || _viewName == "NewView")
            {
                _viewName = _sourcePrefab.name.Replace("Prefab", "").Replace("UI", "") + "View";
            }

            UpdatePreview();
        }

        private void ScanRecursive(Transform parent, List<ElementInfo> elements)
        {
            foreach (Transform child in parent)
            {
                if (NamingConvention.TryParse(child.name, out var prefix, out var name))
                {
                    var componentType = GetComponentTypeForPrefix(prefix);
                    if (componentType != null)
                    {
                        var component = child.GetComponent(componentType);
                        if (component != null)
                        {
                            elements.Add(new ElementInfo
                            {
                                Name = child.name,
                                FieldName = NamingConvention.ToFieldName(child.name),
                                ComponentType = componentType,
                                Component = component,
                                Prefix = prefix
                            });
                        }
                    }
                }
                ScanRecursive(child, elements);
            }
        }

        private Type GetComponentTypeForPrefix(string prefix)
        {
            return prefix switch
            {
                NamingConvention.Button => typeof(Button),
                NamingConvention.Text => typeof(TMP_Text),
                NamingConvention.Image => typeof(Image),
                NamingConvention.RawImage => typeof(RawImage),
                NamingConvention.Toggle => typeof(Toggle),
                NamingConvention.Slider => typeof(Slider),
                NamingConvention.Input => typeof(TMP_InputField),
                NamingConvention.Dropdown => typeof(TMP_Dropdown),
                NamingConvention.Scroll => typeof(ScrollRect),
                NamingConvention.Layout => typeof(HorizontalLayoutGroup),
                NamingConvention.Canvas => typeof(Canvas),
                NamingConvention.Panel => typeof(RectTransform),
                _ => null
            };
        }

        private void UpdatePreview()
        {
            _generatedCode = GenerateCode();
        }

        private string GenerateCode()
        {
            var sb = new StringBuilder();
            var selectedElements = _elements.Where(e => _selectedElements.GetValueOrDefault(e, false)).ToList();
            var viewModelName = _viewName.Replace("View", "") + "ViewModel";

            sb.AppendLine("using Core.Abstractions.MVVM.Views;");
            sb.AppendLine("using R3;");
            sb.AppendLine("using UnityEngine;");
            sb.AppendLine("using UnityEngine.UI;");
            sb.AppendLine("using TMPro;");
            sb.AppendLine();
            sb.AppendLine($"namespace {_namespace}");
            sb.AppendLine("{");
            sb.AppendLine($"    public class {_viewName} : BindableView<{viewModelName}>");
            sb.AppendLine("    {");

            var groupedByPrefix = selectedElements
                .GroupBy(e => e.Prefix)
                .OrderBy(g => GetPrefixOrder(g.Key));

            foreach (var group in groupedByPrefix)
            {
                var headerName = group.Key.TrimEnd('_') + "s";
                sb.AppendLine($"        [Header(\"{headerName}\")]");
                foreach (var element in group)
                {
                    var typeName = GetTypeNameForField(element.ComponentType);
                    sb.AppendLine($"        [SerializeField] private {typeName} {element.FieldName};");
                }
                sb.AppendLine();
            }

            sb.AppendLine("        protected override void OnBind(ref DisposableBag disposables)");
            sb.AppendLine("        {");
            sb.AppendLine("            // TODO: Implement bindings");
            sb.AppendLine("        }");
            sb.AppendLine("    }");
            sb.AppendLine("}");

            return sb.ToString();
        }

        private int GetPrefixOrder(string prefix)
        {
            return prefix switch
            {
                NamingConvention.Canvas => 0,
                NamingConvention.Panel => 1,
                NamingConvention.Layout => 2,
                NamingConvention.Button => 3,
                NamingConvention.Toggle => 4,
                NamingConvention.Slider => 5,
                NamingConvention.Input => 6,
                NamingConvention.Dropdown => 7,
                NamingConvention.Text => 8,
                NamingConvention.Image => 9,
                NamingConvention.RawImage => 10,
                NamingConvention.Scroll => 11,
                _ => 99
            };
        }

        private string GetTypeNameForField(Type type)
        {
            if (type == typeof(TMP_Text) || type.Name == "TextMeshProUGUI")
                return "TMP_Text";
            return type.Name;
        }

        private void GenerateViewFile()
        {
            if (!Directory.Exists(_outputPath))
            {
                Directory.CreateDirectory(_outputPath);
            }

            var filePath = Path.Combine(_outputPath, $"{_viewName}.cs");

            if (File.Exists(filePath))
            {
                if (!EditorUtility.DisplayDialog("File Exists",
                    $"File {_viewName}.cs already exists. Overwrite?",
                    "Overwrite", "Cancel"))
                {
                    return;
                }
            }

            File.WriteAllText(filePath, _generatedCode);
            AssetDatabase.Refresh();

            EditorUtility.DisplayDialog("Success", $"Generated {_viewName}.cs at:\n{filePath}", "OK");
            
            var asset = AssetDatabase.LoadAssetAtPath<MonoScript>(filePath);
            if (asset != null)
            {
                EditorGUIUtility.PingObject(asset);
            }
        }

        private class ElementInfo
        {
            public string Name;
            public string FieldName;
            public Type ComponentType;
            public Component Component;
            public string Prefix;
        }
    }
}
#endif
