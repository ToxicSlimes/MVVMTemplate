#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Core.Infrastructure.MVVM.Config;
using Core.Abstractions.MVVM.Elements;
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEngine;
using UnityEngine.UIElements;

namespace Core.Editor.ElementMappingGenerator
{
    public class ElementMappingGeneratorWindow : EditorWindow
    {
        private TextField _searchField;
        private ListView _elementListView;
        private Button _generateButton;
        private Button _refreshButton;
        private Label _statusLabel;
        private Label _pathsLabel;
        private IMGUIContainer _codePreview;

        private readonly List<ElementTypeInfo> _allElements = new();
        private readonly List<ElementTypeInfo> _filteredElements = new();

        private ElementTypeInfo _selectedElement;
        private ElementConfig _elementConfig;
        private Vector2 _codeScroll;
        private string _previewCode;

        private const string ElementConfigPath = "Assets/Source/00. Configs/ElementConfig.asset";

        [MenuItem("Tools/MVVM/Element Mapping Generator")]
        public static void ShowWindow()
        {
            var window = GetWindow<ElementMappingGeneratorWindow>();
            window.titleContent = new GUIContent("Element Mapping Generator");
            window.minSize = new Vector2(700, 500);
        }

        private void OnEnable()
        {
            LoadElementConfig();
            ScanForElements();
            ResetFilters();
        }

        private void OnDisable()
        {
            if (_searchField != null)
            {
                _searchField.UnregisterValueChangedCallback(OnSearchChanged);
                _searchField = null;
            }

            if (_elementListView != null)
            {
                _elementListView.selectionChanged -= OnElementSelectionChanged;
                _elementListView = null;
            }

            _generateButton = null;
            _refreshButton = null;
            _statusLabel = null;
            _pathsLabel = null;
            _codePreview = null;
        }

        public void CreateGUI()
        {
            var root = rootVisualElement;
            root.style.paddingLeft = 8;
            root.style.paddingRight = 8;
            root.style.paddingTop = 8;
            root.style.paddingBottom = 8;

            // Header
            var topRow = new VisualElement
            {
                style =
                {
                    flexDirection = FlexDirection.Row,
                    alignItems = Align.Center,
                    marginBottom = 4
                }
            };
            root.Add(topRow);

            var instructions = new Label("Выбери BindableElement и сгенерируй mapping")
            {
                style = { flexGrow = 1 }
            };
            topRow.Add(instructions);

            var faq = new Label("?")
            {
                style =
                {
                    unityFontStyleAndWeight = FontStyle.Bold,
                    unityTextAlign = TextAnchor.MiddleCenter,
                    width = 18,
                    height = 18,
                    marginLeft = 4,
                    borderTopWidth = 1,
                    borderBottomWidth = 1,
                    borderLeftWidth = 1,
                    borderRightWidth = 1,
                    borderTopLeftRadius = 3,
                    borderTopRightRadius = 3,
                    borderBottomLeftRadius = 3,
                    borderBottomRightRadius = 3,
                    backgroundColor = new Color(1f, 0.9f, 0.3f)
                },
                tooltip = "1. Создай BindableElement<TData> в Core.\n" +
                          "2. Открой это окно, найди элемент через поиск.\n" +
                          "3. Нажми Generate Mapping.\n" +
                          "4. Дождись компиляции: скрипт создаст .asset и добавит в ElementConfig.\n" +
                          "5. Назначь префаб элемента в созданном mapping asset."
            };
            topRow.Add(faq);

            // Config status
            DrawConfigStatus(root);

            // Search
            _searchField = new TextField("Search");
            _searchField.RegisterValueChangedCallback(OnSearchChanged);
            root.Add(_searchField);

            // Element list
            var listContainer = new VisualElement
            {
                style =
                {
                    flexGrow = 1,
                    borderTopWidth = 1,
                    borderBottomWidth = 1,
                    borderLeftWidth = 1,
                    borderRightWidth = 1,
                    marginTop = 4,
                    paddingTop = 4,
                    paddingLeft = 6,
                    paddingRight = 4,
                    backgroundColor = new Color(0.16f, 0.16f, 0.16f),
                    minHeight = 200
                }
            };
            root.Add(listContainer);

            var listLabel = new Label("BindableElements")
            {
                style =
                {
                    unityFontStyleAndWeight = FontStyle.Bold,
                    marginBottom = 2
                }
            };
            listContainer.Add(listLabel);

            _elementListView = new ListView
            {
                itemsSource = _filteredElements,
                selectionType = SelectionType.Single,
                showAlternatingRowBackgrounds = AlternatingRowBackground.All,
                style = { flexGrow = 1 }
            };
            _elementListView.makeItem = MakeElementItem;
            _elementListView.bindItem = BindElementItem;
            _elementListView.selectionChanged += OnElementSelectionChanged;
            listContainer.Add(_elementListView);

            // Preview
            var previewContainer = new VisualElement
            {
                style =
                {
                    borderTopWidth = 1,
                    borderBottomWidth = 1,
                    borderLeftWidth = 1,
                    borderRightWidth = 1,
                    marginTop = 6,
                    paddingTop = 4,
                    paddingLeft = 6,
                    paddingRight = 4,
                    paddingBottom = 4,
                    backgroundColor = new Color(0.12f, 0.12f, 0.12f)
                }
            };
            root.Add(previewContainer);

            var previewLabel = new Label("Preview")
            {
                style =
                {
                    unityFontStyleAndWeight = FontStyle.Bold,
                    marginBottom = 2
                }
            };
            previewContainer.Add(previewLabel);

            var pathsTitle = new Label("Paths")
            {
                style =
                {
                    unityFontStyleAndWeight = FontStyle.Bold,
                    marginTop = 2
                }
            };
            previewContainer.Add(pathsTitle);

            _pathsLabel = new Label
            {
                style =
                {
                    whiteSpace = WhiteSpace.Normal,
                    marginBottom = 4
                }
            };
            previewContainer.Add(_pathsLabel);

            var codeTitle = new Label("Generated mapping class")
            {
                style =
                {
                    unityFontStyleAndWeight = FontStyle.Bold,
                    marginBottom = 2
                }
            };
            previewContainer.Add(codeTitle);

            _codePreview = new IMGUIContainer(DrawCodePreview)
            {
                style = { height = 120 }
            };
            previewContainer.Add(_codePreview);

            // Bottom row
            var bottomRow = new VisualElement
            {
                style =
                {
                    flexDirection = FlexDirection.Row,
                    alignItems = Align.Center,
                    marginTop = 6
                }
            };
            root.Add(bottomRow);

            _refreshButton = new Button(OnRefreshClicked) { text = "Refresh" };
            bottomRow.Add(_refreshButton);

            _generateButton = new Button(OnGenerateClicked)
            {
                text = "Generate Mapping",
                style = { marginLeft = 8 }
            };
            bottomRow.Add(_generateButton);

            _statusLabel = new Label
            {
                style = { marginLeft = 8 }
            };
            bottomRow.Add(_statusLabel);

            UpdatePreview();
        }

        private void DrawConfigStatus(VisualElement root)
        {
            var configRow = new VisualElement
            {
                style =
                {
                    flexDirection = FlexDirection.Row,
                    alignItems = Align.Center,
                    marginTop = 4,
                    marginBottom = 4
                }
            };
            root.Add(configRow);

            var configLabel = new Label("ElementConfig: ")
            {
                style = { unityFontStyleAndWeight = FontStyle.Bold }
            };
            configRow.Add(configLabel);

            if (_elementConfig != null)
            {
                var statusLabel = new Label("✓ Loaded")
                {
                    style = { color = new Color(0.4f, 0.8f, 0.4f) }
                };
                configRow.Add(statusLabel);
            }
            else
            {
                var statusLabel = new Label("✗ Not found")
                {
                    style = { color = new Color(1f, 0.4f, 0.4f) }
                };
                configRow.Add(statusLabel);

                var createButton = new Button(CreateElementConfig)
                {
                    text = "Create",
                    style = { marginLeft = 8 }
                };
                configRow.Add(createButton);
            }
        }

        private VisualElement MakeElementItem()
        {
            var container = new VisualElement
            {
                style =
                {
                    flexDirection = FlexDirection.Row,
                    alignItems = Align.Center,
                    paddingLeft = 4,
                    paddingTop = 2,
                    paddingBottom = 2
                }
            };

            var nameLabel = new Label
            {
                name = "name",
                style = { flexGrow = 1 }
            };
            container.Add(nameLabel);

            var dataLabel = new Label
            {
                name = "data",
                style =
                {
                    color = new Color(0.6f, 0.6f, 0.6f),
                    marginRight = 8
                }
            };
            container.Add(dataLabel);

            var statusLabel = new Label
            {
                name = "status",
                style = { width = 70 }
            };
            container.Add(statusLabel);

            return container;
        }

        private void BindElementItem(VisualElement element, int index)
        {
            if (index < 0 || index >= _filteredElements.Count) return;

            var info = _filteredElements[index];

            var nameLabel = element.Q<Label>("name");
            nameLabel.text = info.ElementType.Name;
            nameLabel.tooltip = info.ElementType.FullName;

            var dataLabel = element.Q<Label>("data");
            dataLabel.text = $"<{info.DataType.Name}>";

            var statusLabel = element.Q<Label>("status");
            if (info.HasMapping)
            {
                statusLabel.text = "[Mapped]";
                statusLabel.style.color = new Color(0.4f, 0.8f, 0.4f);
            }
            else
            {
                statusLabel.text = "[New]";
                statusLabel.style.color = new Color(1f, 0.8f, 0.3f);
            }
        }

        private void OnSearchChanged(ChangeEvent<string> evt)
        {
            ApplyFilter();
        }

        private void OnElementSelectionChanged(IEnumerable<object> selection)
        {
            foreach (var item in selection)
            {
                if (item is ElementTypeInfo info)
                {
                    _selectedElement = info;
                    UpdatePreview();
                    return;
                }
            }

            _selectedElement = null;
            UpdatePreview();
        }

        private void ResetFilters()
        {
            _filteredElements.Clear();
            _filteredElements.AddRange(_allElements);
        }

        private void ApplyFilter()
        {
            var filter = _searchField?.value?.Trim() ?? string.Empty;
            var hasFilter = !string.IsNullOrEmpty(filter);
            var lowerFilter = hasFilter ? filter.ToLowerInvariant() : string.Empty;

            _filteredElements.Clear();

            foreach (var info in _allElements)
            {
                if (!hasFilter)
                {
                    _filteredElements.Add(info);
                    continue;
                }

                var name = info.ElementType.Name?.ToLowerInvariant() ?? "";
                var fullName = info.ElementType.FullName?.ToLowerInvariant() ?? "";
                var dataName = info.DataType.Name?.ToLowerInvariant() ?? "";

                if (name.Contains(lowerFilter) || fullName.Contains(lowerFilter) || dataName.Contains(lowerFilter))
                {
                    _filteredElements.Add(info);
                }
            }

            if (_elementListView != null)
            {
                _elementListView.itemsSource = null;
                _elementListView.itemsSource = _filteredElements;
                _elementListView.Rebuild();
            }
        }

        private void UpdatePreview()
        {
            if (_selectedElement == null)
            {
                if (_pathsLabel != null) _pathsLabel.text = string.Empty;
                _previewCode = string.Empty;
                _codePreview?.MarkDirtyRepaint();
                return;
            }

            var elementName = _selectedElement.ElementType.Name;
            var csPath = ElementMappingCodeGenerator.GetMappingFilePath(elementName);
            var assetPath = ElementMappingCodeGenerator.GetMappingAssetPath(elementName);

            if (_pathsLabel != null)
            {
                _pathsLabel.text = $"CS: {csPath}\nAsset: {assetPath}";
            }

            _previewCode = ElementMappingCodeGenerator.GenerateMappingCode(
                elementName,
                _selectedElement.ElementType.Namespace,
                _selectedElement.DataType.Name,
                _selectedElement.DataType.Namespace);

            _codePreview?.MarkDirtyRepaint();
        }

        private void OnRefreshClicked()
        {
            LoadElementConfig();
            ScanForElements();
            ApplyFilter();
            _statusLabel.text = $"Found {_allElements.Count} elements";
        }

        private void OnGenerateClicked()
        {
            if (_selectedElement == null)
            {
                _statusLabel.text = "Select an element first";
                return;
            }

            if (_elementConfig == null)
            {
                _statusLabel.text = "Create ElementConfig first";
                return;
            }

            if (_selectedElement.HasMapping)
            {
                _statusLabel.text = "Already mapped";
                return;
            }

            GenerateMapping(_selectedElement);
        }

        private void LoadElementConfig()
        {
            _elementConfig = AssetDatabase.LoadAssetAtPath<ElementConfig>(ElementConfigPath);
        }

        private void CreateElementConfig()
        {
            EnsureDirectoryExists(Path.GetDirectoryName(ElementConfigPath));

            var config = CreateInstance<ElementConfig>();
            AssetDatabase.CreateAsset(config, ElementConfigPath);
            AssetDatabase.SaveAssets();

            _elementConfig = config;
            Debug.Log($"Created ElementConfig at {ElementConfigPath}");

            // Rebuild UI
            rootVisualElement.Clear();
            CreateGUI();
        }

        private void ScanForElements()
        {
            _allElements.Clear();

            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            var bindableElementType = typeof(BindableElement<>);

            foreach (var assembly in assemblies)
            {
                if (assembly.IsDynamic) continue;
                if (IsTestAssembly(assembly)) continue;

                try
                {
                    Type[] types;
                    try
                    {
                        types = assembly.GetTypes();
                    }
                    catch (ReflectionTypeLoadException ex)
                    {
                        types = ex.Types.Where(t => t != null).ToArray();
                    }

                    foreach (var type in types)
                    {
                        if (type == null || type.IsAbstract || type.IsGenericType) continue;

                        var dataType = GetBindableElementDataType(type, bindableElementType);
                        if (dataType == null) continue;

                        var info = new ElementTypeInfo
                        {
                            ElementType = type,
                            DataType = dataType,
                            HasMapping = CheckHasMapping(type),
                            ExistingMapping = GetExistingMapping(type)
                        };

                        _allElements.Add(info);
                    }
                }
                catch (Exception ex)
                {
                    Debug.LogWarning($"[ElementMapping] Failed to load types from {assembly.GetName().Name}: {ex.Message}");
                }
            }

            _allElements.Sort((a, b) => string.Compare(a.ElementType.Name, b.ElementType.Name, StringComparison.Ordinal));

            if (_allElements.Count == 0)
            {
                Debug.LogWarning($"[ElementMapping] No BindableElements found. BindableElement<> type: {bindableElementType.AssemblyQualifiedName}");
            }
            else
            {
                Debug.Log($"[ElementMapping] Found {_allElements.Count} elements: {string.Join(", ", _allElements.Select(e => e.ElementType.Name))}");
            }
        }

        private static bool IsTestAssembly(Assembly assembly)
        {
            var name = assembly.GetName().Name;
            return name.Contains("Test") || name.Contains("NSubstitute") || name.Contains("nunit");
        }

        private Type GetBindableElementDataType(Type type, Type bindableElementType)
        {
            var baseType = type.BaseType;
            while (baseType != null)
            {
                if (baseType.IsGenericType && baseType.GetGenericTypeDefinition() == bindableElementType)
                {
                    return baseType.GetGenericArguments()[0];
                }
                baseType = baseType.BaseType;
            }
            return null;
        }

        private bool CheckHasMapping(Type elementType)
        {
            if (_elementConfig == null) return false;

            var mappings = _elementConfig.GetAllMappings();
            return mappings.Any(m => m != null && m.ElementType == elementType);
        }

        private ElementMappingBase GetExistingMapping(Type elementType)
        {
            if (_elementConfig == null) return null;

            var mappings = _elementConfig.GetAllMappings();
            return mappings.FirstOrDefault(m => m != null && m.ElementType == elementType);
        }

        private void GenerateMapping(ElementTypeInfo info)
        {
            var elementName = info.ElementType.Name;
            var csPath = ElementMappingCodeGenerator.GetMappingFilePath(elementName);
            var assetPath = ElementMappingCodeGenerator.GetMappingAssetPath(elementName);
            var mappingTypeName = ElementMappingCodeGenerator.GetMappingTypeName(elementName);

            // Check if script exists
            if (File.Exists(csPath))
            {
                var mappingType = FindTypeInAssemblies(mappingTypeName);
                if (mappingType != null)
                {
                    EnsureDirectoryExists(ElementMappingCodeGenerator.GetAssetFolder());

                    var existingAsset = AssetDatabase.LoadAssetAtPath(assetPath, mappingType);
                    if (existingAsset == null)
                    {
                        var instance = CreateInstance(mappingType) as ElementMappingBase;
                        if (instance != null)
                        {
                            AssetDatabase.CreateAsset(instance, assetPath);
                            AssetDatabase.SaveAssets();
                            TryAddMappingToConfig(instance);
                            _statusLabel.text = "Asset created and added to config";
                        }
                        return;
                    }

                    TryAddMappingToConfig(existingAsset as ElementMappingBase);
                    _statusLabel.text = "Mapping already exists";
                    return;
                }

                _statusLabel.text = "Script exists, waiting for compilation...";
                ElementMappingGeneratorPostCompile.Schedule(elementName);
                return;
            }

            // Generate script
            EnsureDirectoryExists(Path.GetDirectoryName(csPath));
            EnsureDirectoryExists(ElementMappingCodeGenerator.GetAssetFolder());

            var code = ElementMappingCodeGenerator.GenerateMappingCode(
                elementName,
                info.ElementType.Namespace,
                info.DataType.Name,
                info.DataType.Namespace);

            File.WriteAllText(csPath, code);
            AssetDatabase.ImportAsset(csPath);

            _statusLabel.text = "Script generated. Waiting for compilation...";
            ElementMappingGeneratorPostCompile.Schedule(elementName);
        }

        private void TryAddMappingToConfig(ElementMappingBase mapping)
        {
            if (mapping == null || _elementConfig == null) return;

            var mappings = _elementConfig.GetAllMappings();
            if (mappings.Contains(mapping)) return;

            _elementConfig.AddMapping(mapping);
            EditorUtility.SetDirty(_elementConfig);
            AssetDatabase.SaveAssets();
        }

        private static Type FindTypeInAssemblies(string typeName)
        {
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();

            foreach (var asm in assemblies)
            {
                var type = asm.GetType(typeName);
                if (type != null) return type;
            }

            foreach (var asm in assemblies)
            {
                try
                {
                    foreach (var t in asm.GetTypes())
                    {
                        if (t.FullName == typeName || t.Name == typeName)
                            return t;
                    }
                }
                catch (ReflectionTypeLoadException) { }
            }

            return null;
        }

        private void DrawCodePreview()
        {
            var baseStyle = new GUIStyle(EditorStyles.label)
            {
                richText = false,
                wordWrap = false,
                font = EditorStyles.textArea.font
            };
            baseStyle.normal.textColor = Color.white;

            var keywordStyle = new GUIStyle(baseStyle);
            keywordStyle.normal.textColor = new Color(0.5f, 0.8f, 1f);

            var typeStyle = new GUIStyle(baseStyle);
            typeStyle.normal.textColor = new Color(1f, 0.85f, 0.4f);

            _codeScroll = EditorGUILayout.BeginScrollView(_codeScroll);

            if (!string.IsNullOrEmpty(_previewCode))
            {
                var lines = _previewCode.Split('\n');
                foreach (var line in lines)
                {
                    var trimmed = line.TrimStart();
                    var styleToUse = baseStyle;

                    if (trimmed.StartsWith("using ") || trimmed.StartsWith("namespace ") ||
                        trimmed.StartsWith("public class") || trimmed.StartsWith("internal class"))
                    {
                        styleToUse = keywordStyle;
                    }

                    if (line.Contains("ElementMappingBase<"))
                    {
                        styleToUse = typeStyle;
                    }

                    EditorGUILayout.LabelField(line, styleToUse);
                }
            }

            EditorGUILayout.EndScrollView();
        }

        private static void EnsureDirectoryExists(string path)
        {
            if (string.IsNullOrEmpty(path)) return;
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
        }

        [MenuItem("Tools/MVVM/Create Missing Element Mapping Assets")]
        public static void CreateMissingAssets()
        {
            var config = AssetDatabase.LoadAssetAtPath<ElementConfig>(ElementConfigPath);
            if (config == null)
            {
                Debug.LogError($"ElementConfig not found at {ElementConfigPath}");
                return;
            }

            var generatedFolder = ElementMappingCodeGenerator.GetGeneratedFolder();
            var assetFolder = ElementMappingCodeGenerator.GetAssetFolder();

            EnsureDirectoryExists(assetFolder);

            var scripts = AssetDatabase.FindAssets("t:MonoScript", new[] { generatedFolder });
            var createdCount = 0;

            foreach (var guid in scripts)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var script = AssetDatabase.LoadAssetAtPath<MonoScript>(path);
                if (script == null) continue;

                var scriptClass = script.GetClass();
                if (scriptClass == null) continue;
                if (!typeof(ElementMappingBase).IsAssignableFrom(scriptClass)) continue;

                var assetPath = ElementMappingCodeGenerator.GetMappingAssetPath(
                    scriptClass.Name.Replace("Mapping", ""));

                if (AssetDatabase.LoadAssetAtPath<ElementMappingBase>(assetPath) != null) continue;

                var instance = CreateInstance(scriptClass) as ElementMappingBase;
                if (instance == null) continue;

                AssetDatabase.CreateAsset(instance, assetPath);
                config.AddMapping(instance);
                createdCount++;

                Debug.Log($"Created mapping asset: {assetPath}");
            }

            AssetDatabase.SaveAssets();
            Debug.Log($"Created {createdCount} element mapping assets");
        }

        [MenuItem("Tools/MVVM/Link Prefabs to Element Mappings")]
        public static void LinkPrefabsToMappings()
        {
            var config = AssetDatabase.LoadAssetAtPath<ElementConfig>(ElementConfigPath);
            if (config == null)
            {
                Debug.LogError($"ElementConfig not found at {ElementConfigPath}");
                return;
            }

            var mappings = config.GetAllMappings();
            var linkedCount = 0;
            var searchFolders = new[] { "Assets/Prefabs" };

            foreach (var mapping in mappings)
            {
                if (mapping == null) continue;
                if (mapping.ElementPrefab != null && mapping.ElementPrefab.RuntimeKeyIsValid()) continue;

                var elementName = mapping.ElementType.Name.Replace("Element", "");
                var prefab = FindPrefabByName(elementName, searchFolders);

                if (prefab == null)
                {
                    Debug.LogWarning($"[LinkPrefabs] No prefab found for {mapping.ElementType.Name} (searched: {elementName})");
                    continue;
                }

                var component = prefab.GetComponent(mapping.ElementType);
                if (component == null)
                {
                    Debug.LogWarning($"[LinkPrefabs] Prefab {prefab.name} does not have {mapping.ElementType.Name} component");
                    continue;
                }

                var prefabPath = AssetDatabase.GetAssetPath(prefab);
                var prefabGuid = AssetDatabase.AssetPathToGUID(prefabPath);

                EnsureAddressable(prefabPath, prefabGuid);

                var so = new SerializedObject(mapping);
                var guidProp = so.FindProperty("ElementPrefab.m_AssetGUID");
                if (guidProp != null)
                {
                    guidProp.stringValue = prefabGuid;
                    so.ApplyModifiedProperties();
                }

                EditorUtility.SetDirty(mapping);
                linkedCount++;

                Debug.Log($"<color=green>✓ Linked {mapping.name} → {prefab.name} ({prefabPath})</color>");
            }

            AssetDatabase.SaveAssets();
            Debug.Log($"<color=cyan>Linked {linkedCount} prefabs to element mappings</color>");
        }

        private static void EnsureAddressable(string assetPath, string guid)
        {
            var settings = UnityEditor.AddressableAssets.AddressableAssetSettingsDefaultObject.Settings;
            if (settings == null) return;

            var entry = settings.FindAssetEntry(guid);
            if (entry != null) return;

            var group = settings.DefaultGroup;
            entry = settings.CreateOrMoveEntry(guid, group);
            entry.address = assetPath;
            EditorUtility.SetDirty(settings);
        }

        private static GameObject FindPrefabByName(string name, string[] searchFolders)
        {
            var guids = AssetDatabase.FindAssets($"t:Prefab {name}", searchFolders);

            foreach (var guid in guids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);

                if (prefab != null && prefab.name.Equals(name, StringComparison.OrdinalIgnoreCase))
                {
                    return prefab;
                }
            }

            foreach (var guid in guids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);

                if (prefab != null && prefab.name.Contains(name, StringComparison.OrdinalIgnoreCase))
                {
                    return prefab;
                }
            }

            return null;
        }

        private class ElementTypeInfo
        {
            public Type ElementType;
            public Type DataType;
            public bool HasMapping;
            public ElementMappingBase ExistingMapping;
        }
    }
}
#endif

