using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Core.Abstractions.MVVM.ViewModels;
using Core.Abstractions.MVVM.Views;
using Core.Presentation.MVVM.Config;
using UnityEditor;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.UIElements;

namespace Core.Editor.ViewMappingGenerator
{
    public class ViewMappingGeneratorWindow : EditorWindow
    {
        private TextField _searchField;
        private ListView _viewModelListView;
        private ListView _viewListView;
        private Button _generateButton;
        private Label _statusLabel;
        private Label _pathsLabel;
        private IMGUIContainer _codePreview;

        private readonly List<Type> _viewModelTypes = new List<Type>();
        private readonly List<Type> _viewTypes = new List<Type>();
        private readonly List<Type> _filteredViewModelTypes = new List<Type>();
        private readonly List<Type> _filteredViewTypes = new List<Type>();

        private string _currentBaseName;
        private Type _resolvedViewType;
        private Type _resolvedViewModelType;
        private Vector2 _codeScroll;
        private string _previewCode;

        [MenuItem("Tools/MVVM/View Mapping Generator")]
        public static void ShowWindow()
        {
            var window = GetWindow<ViewMappingGeneratorWindow>();
            window.titleContent = new GUIContent("View Mapping Generator");
            window.minSize = new Vector2(700, 420);
        }

        private void OnEnable()
        {
            ScanTypes();
            ResetFilters();
        }

        private void OnDisable()
        {
            if (_searchField != null)
            {
                _searchField.UnregisterValueChangedCallback(_ => ApplyFilter());
                _searchField = null;
            }

            if (_viewModelListView != null)
            {
                _viewModelListView.selectionChanged -= OnViewModelSelectionChanged;
                _viewModelListView = null;
            }

            if (_viewListView != null)
            {
                _viewListView.selectionChanged -= OnViewSelectionChanged;
                _viewListView = null;
            }

            _generateButton = null;
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

            var topRow = new VisualElement
            {
                style =
                {
                    flexDirection = FlexDirection.Row,
                    alignItems = Align.Center
                }
            };
            root.Add(topRow);

            var instructions = new Label("Выбери ViewModel или View и сгенерируй mapping")
            {
                style =
                {
                    flexGrow = 1
                }
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
                tooltip = "1. Создай View и ViewModel в Core.\n2. Открой это окно, найди ViewModel или View через поиск.\n3. Нажми Generate Mapping.\n4. Дождись компиляции: скрипт создаст экземпляр SO в виде .asset и добавит его в ViewConfig.\n5. При необходимости переоткрой ViewMapping и проверь ссылку на префаб View в поле ViewPrefab."
            };
            topRow.Add(faq);

            _searchField = new TextField("Search");
            _searchField.RegisterValueChangedCallback(_ => ApplyFilter());
            root.Add(_searchField);

            var listsRow = new VisualElement();
            listsRow.style.flexDirection = FlexDirection.Row;
            listsRow.style.flexGrow = 1;
            listsRow.style.height = 220;
            root.Add(listsRow);

            var viewModelColumn = new VisualElement();
            viewModelColumn.style.flexGrow = 1;
            viewModelColumn.style.borderTopWidth = 1;
            viewModelColumn.style.borderBottomWidth = 1;
            viewModelColumn.style.borderLeftWidth = 1;
            viewModelColumn.style.borderRightWidth = 1;
            viewModelColumn.style.marginRight = 4;
            viewModelColumn.style.paddingTop = 4;
            viewModelColumn.style.paddingLeft = 6;
            viewModelColumn.style.paddingRight = 4;
            viewModelColumn.style.backgroundColor = new Color(0.16f, 0.16f, 0.16f);
            listsRow.Add(viewModelColumn);

            var viewModelLabel = new Label("ViewModels");
            viewModelLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
            viewModelLabel.style.marginBottom = 2;
            viewModelColumn.Add(viewModelLabel);

            _viewModelListView = new ListView();
            _viewModelListView.itemsSource = _filteredViewModelTypes;
            _viewModelListView.selectionType = SelectionType.Single;
            _viewModelListView.showAlternatingRowBackgrounds = AlternatingRowBackground.All;
            _viewModelListView.makeItem = () =>
            {
                var label = new Label();
                label.style.paddingLeft = 4;
                label.style.paddingTop = 2;
                label.style.paddingBottom = 2;
                label.style.unityTextAlign = TextAnchor.MiddleLeft;
                return label;
            };
            _viewModelListView.bindItem = (element, index) =>
            {
                var label = (Label)element;
                if (index >= 0 && index < _filteredViewModelTypes.Count)
                {
                    var type = _filteredViewModelTypes[index];
                    label.text = type.Name;
                    label.tooltip = type.FullName;
                }
                else
                {
                    label.text = string.Empty;
                    label.tooltip = string.Empty;
                }
            };
            _viewModelListView.selectionChanged += OnViewModelSelectionChanged;
            viewModelColumn.Add(_viewModelListView);

            var viewColumn = new VisualElement();
            viewColumn.style.flexGrow = 1;
            viewColumn.style.borderTopWidth = 1;
            viewColumn.style.borderBottomWidth = 1;
            viewColumn.style.borderLeftWidth = 1;
            viewColumn.style.borderRightWidth = 1;
            viewColumn.style.marginLeft = 4;
            viewColumn.style.paddingTop = 4;
            viewColumn.style.paddingLeft = 6;
            viewColumn.style.paddingRight = 4;
            viewColumn.style.backgroundColor = new Color(0.14f, 0.14f, 0.14f);
            listsRow.Add(viewColumn);

            var viewLabel = new Label("Views");
            viewLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
            viewLabel.style.marginBottom = 2;
            viewColumn.Add(viewLabel);

            _viewListView = new ListView();
            _viewListView.itemsSource = _filteredViewTypes;
            _viewListView.selectionType = SelectionType.Single;
            _viewListView.showAlternatingRowBackgrounds = AlternatingRowBackground.All;
            _viewListView.makeItem = () =>
            {
                var label = new Label();
                label.style.paddingLeft = 4;
                label.style.paddingTop = 2;
                label.style.paddingBottom = 2;
                label.style.unityTextAlign = TextAnchor.MiddleLeft;
                return label;
            };
            _viewListView.bindItem = (element, index) =>
            {
                var label = (Label)element;
                if (index >= 0 && index < _filteredViewTypes.Count)
                {
                    var type = _filteredViewTypes[index];
                    label.text = type.Name;
                    label.tooltip = type.FullName;
                }
                else
                {
                    label.text = string.Empty;
                    label.tooltip = string.Empty;
                }
            };
            _viewListView.selectionChanged += OnViewSelectionChanged;
            viewColumn.Add(_viewListView);

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

            var previewLabel = new Label("Preview");
            previewLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
            previewLabel.style.marginBottom = 2;
            previewContainer.Add(previewLabel);

            var pathsTitle = new Label("Paths");
            pathsTitle.style.unityFontStyleAndWeight = FontStyle.Bold;
            pathsTitle.style.marginTop = 2;
            previewContainer.Add(pathsTitle);

            _pathsLabel = new Label();
            _pathsLabel.style.whiteSpace = WhiteSpace.Normal;
            _pathsLabel.style.marginBottom = 4;
            previewContainer.Add(_pathsLabel);

            var codeTitle = new Label("Generated mapping class");
            codeTitle.style.unityFontStyleAndWeight = FontStyle.Bold;
            codeTitle.style.marginBottom = 2;
            previewContainer.Add(codeTitle);

            _codePreview = new IMGUIContainer(DrawCodePreview);
            _codePreview.style.height = 140;
            previewContainer.Add(_codePreview);

            var bottomRow = new VisualElement();
            bottomRow.style.flexDirection = FlexDirection.Row;
            bottomRow.style.alignItems = Align.Center;
            root.Add(bottomRow);

            _generateButton = new Button(OnGenerateClicked);
            _generateButton.text = "Generate Mapping";
            bottomRow.Add(_generateButton);

            _statusLabel = new Label();
            _statusLabel.style.marginLeft = 8;
            bottomRow.Add(_statusLabel);

            UpdatePreview();
        }

        private void ScanTypes()
        {
            _viewModelTypes.Clear();
            _viewTypes.Clear();

            foreach (var type in TypeCache.GetTypesDerivedFrom<IViewModel>())
            {
                if (type.IsAbstract)
                {
                    continue;
                }
                _viewModelTypes.Add(type);
            }

            foreach (var type in TypeCache.GetTypesDerivedFrom(typeof(BindableView<>)))
            {
                if (type.IsAbstract)
                {
                    continue;
                }
                _viewTypes.Add(type);
            }
        }

        private void ResetFilters()
        {
            _filteredViewModelTypes.Clear();
            _filteredViewTypes.Clear();

            for (int i = 0; i < _viewModelTypes.Count; i++)
            {
                _filteredViewModelTypes.Add(_viewModelTypes[i]);
            }

            for (int i = 0; i < _viewTypes.Count; i++)
            {
                _filteredViewTypes.Add(_viewTypes[i]);
            }
        }

        private void ApplyFilter()
        {
            var filter = _searchField != null ? _searchField.value : string.Empty;
            filter = filter != null ? filter.Trim() : string.Empty;
            var hasFilter = !string.IsNullOrEmpty(filter);
            var lowerFilter = hasFilter ? filter.ToLowerInvariant() : string.Empty;

            _filteredViewModelTypes.Clear();
            _filteredViewTypes.Clear();

            for (int i = 0; i < _viewModelTypes.Count; i++)
            {
                var type = _viewModelTypes[i];
                if (!hasFilter)
                {
                    _filteredViewModelTypes.Add(type);
                    continue;
                }

                var name = type.Name;
                var fullName = type.FullName;
                if ((name != null && name.ToLowerInvariant().Contains(lowerFilter)) || (fullName != null && fullName.ToLowerInvariant().Contains(lowerFilter)))
                {
                    _filteredViewModelTypes.Add(type);
                }
            }

            for (int i = 0; i < _viewTypes.Count; i++)
            {
                var type = _viewTypes[i];
                if (!hasFilter)
                {
                    _filteredViewTypes.Add(type);
                    continue;
                }

                var name = type.Name;
                var fullName = type.FullName;
                if ((name != null && name.ToLowerInvariant().Contains(lowerFilter)) || (fullName != null && fullName.ToLowerInvariant().Contains(lowerFilter)))
                {
                    _filteredViewTypes.Add(type);
                }
            }

            if (_viewModelListView != null)
            {
                _viewModelListView.itemsSource = null;
                _viewModelListView.itemsSource = _filteredViewModelTypes;
                _viewModelListView.Rebuild();
            }

            if (_viewListView != null)
            {
                _viewListView.itemsSource = null;
                _viewListView.itemsSource = _filteredViewTypes;
                _viewListView.Rebuild();
            }
        }

        private void OnViewModelSelectionChanged(IEnumerable<object> selection)
        {
            foreach (var item in selection)
            {
                var type = item as Type;
                if (type == null)
                {
                    continue;
                }

                var name = type.Name;
                var suffix = "ViewModel";
                if (name.EndsWith(suffix, StringComparison.Ordinal))
                {
                    name = name.Substring(0, name.Length - suffix.Length);
                }

                _currentBaseName = name;
                ResolveTypePair(name);
                UpdatePreview();
                break;
            }
        }

        private void OnViewSelectionChanged(IEnumerable<object> selection)
        {
            foreach (var item in selection)
            {
                var type = item as Type;
                if (type == null)
                {
                    continue;
                }

                var name = type.Name;
                var suffix = "View";
                if (name.EndsWith(suffix, StringComparison.Ordinal))
                {
                    name = name.Substring(0, name.Length - suffix.Length);
                }

                _currentBaseName = name;
                ResolveTypePair(name);
                UpdatePreview();
                break;
            }
        }

        private void ResolveTypePair(string baseName)
        {
            _resolvedViewType = null;
            _resolvedViewModelType = null;

            var viewTypeName = baseName + "View";
            var viewModelTypeName = baseName + "ViewModel";

            for (int i = 0; i < _viewTypes.Count; i++)
            {
                if (_viewTypes[i].Name == viewTypeName)
                {
                    _resolvedViewType = _viewTypes[i];
                    break;
                }
            }

            for (int i = 0; i < _viewModelTypes.Count; i++)
            {
                if (_viewModelTypes[i].Name == viewModelTypeName)
                {
                    _resolvedViewModelType = _viewModelTypes[i];
                    break;
                }
            }
        }

        private void UpdatePreview()
        {
            var viewName = _currentBaseName;
            if (string.IsNullOrWhiteSpace(viewName))
            {
                _pathsLabel.text = string.Empty;
                _previewCode = string.Empty;
                _codePreview.MarkDirtyRepaint();
                return;
            }

            var csPath = ViewMappingGeneratorConfig.GetMappingScriptPath(viewName);
            var assetPath = ViewMappingGeneratorConfig.GetMappingAssetPath(viewName);

            _pathsLabel.text = "CS: " + csPath + "\nAsset: " + assetPath;
            _previewCode = ViewMappingCodeGenerator.GenerateMappingCode(viewName,
                _resolvedViewType?.Namespace, _resolvedViewModelType?.Namespace);
            _codePreview.MarkDirtyRepaint();
        }

        private void OnGenerateClicked()
        {
            var viewName = _currentBaseName;
            if (string.IsNullOrWhiteSpace(viewName))
            {
                _statusLabel.text = "Select ViewModel or View";
                return;
            }

            var csPath = ViewMappingGeneratorConfig.GetMappingScriptPath(viewName);
            var assetPath = ViewMappingGeneratorConfig.GetMappingAssetPath(viewName);
            var mappingTypeName = ViewMappingGeneratorConfig.GetMappingTypeName(viewName);

            if (File.Exists(csPath))
            {
                var mappingType = FindTypeInAssemblies(mappingTypeName);
                if (mappingType != null)
                {
                    ViewMappingGeneratorConfig.EnsureDirectoriesExist(viewName);

                    var existingAsset = AssetDatabase.LoadAssetAtPath(assetPath, mappingType);
                    if (existingAsset == null)
                    {
                        var instance = ScriptableObject.CreateInstance(mappingType);
                        AssetDatabase.CreateAsset(instance, assetPath);
                        AssetDatabase.SaveAssets();
                        TryAutoAssignViewPrefab(instance, viewName);
                        TryAddMappingToViewConfig(instance);
                        _statusLabel.text = "SO instance created for existing mapping script and added to ViewConfig";
                        return;
                    }

                    TryAutoAssignViewPrefab(existingAsset, viewName);
                    TryAddMappingToViewConfig(existingAsset);
                    _statusLabel.text = "Mapping already exists";
                    return;
                }

                _statusLabel.text = "Mapping script exists, waiting for compilation to create SO instance";
                ViewMappingGeneratorPostCompile.Schedule(viewName);
                return;
            }

            ViewMappingGeneratorConfig.EnsureDirectoriesExist(viewName);

            var code = ViewMappingCodeGenerator.GenerateMappingCode(viewName,
                _resolvedViewType?.Namespace, _resolvedViewModelType?.Namespace);
            File.WriteAllText(csPath, code);
            AssetDatabase.ImportAsset(csPath);

            _statusLabel.text = "Script generated. Waiting for compilation to create SO instance";
            ViewMappingGeneratorPostCompile.Schedule(viewName);
        }

        internal static void TryAutoAssignViewPrefab(UnityEngine.Object mapping, string viewName)
        {
            var mappingBase = mapping as ViewMappingBase;
            if (mappingBase == null)
            {
                return;
            }

            var searchName = viewName + "View t:prefab";
            var guids = AssetDatabase.FindAssets(searchName);
            if (guids == null || guids.Length != 1)
            {
                return;
            }

            var guid = guids[0];
            var assetReference = new AssetReferenceGameObject(guid);
            mappingBase.ViewPrefab = assetReference;
            EditorUtility.SetDirty(mappingBase);
            AssetDatabase.SaveAssets();
        }

        internal static void TryAddMappingToViewConfig(UnityEngine.Object mapping)
        {
            var config = AssetDatabase.LoadAssetAtPath<ViewConfig>(ViewMappingGeneratorConfig.ViewConfigPath);
            if (config == null)
            {
                return;
            }

            var serializedObject = new SerializedObject(config);
            var mappingsProperty = serializedObject.FindProperty("_mappings");
            if (mappingsProperty == null)
            {
                return;
            }

            for (int i = 0; i < mappingsProperty.arraySize; i++)
            {
                var element = mappingsProperty.GetArrayElementAtIndex(i);
                if (element.objectReferenceValue == mapping)
                {
                    return;
                }
            }

            var indexToInsert = mappingsProperty.arraySize;
            mappingsProperty.InsertArrayElementAtIndex(indexToInsert);
            var newElement = mappingsProperty.GetArrayElementAtIndex(indexToInsert);
            newElement.objectReferenceValue = mapping;

            serializedObject.ApplyModifiedProperties();
            EditorUtility.SetDirty(config);
            AssetDatabase.SaveAssets();
        }

        internal static Type FindTypeInAssemblies(string typeName)
        {
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            for (int i = 0; i < assemblies.Length; i++)
            {
                var asm = assemblies[i];
                var type = asm.GetType(typeName);
                if (type != null)
                {
                    return type;
                }
            }

            for (int i = 0; i < assemblies.Length; i++)
            {
                var asm = assemblies[i];
                Type[] types;
                try
                {
                    types = asm.GetTypes();
                }
                catch (ReflectionTypeLoadException e)
                {
                    types = e.Types;
                }

                if (types == null)
                {
                    continue;
                }

                for (int j = 0; j < types.Length; j++)
                {
                    var t = types[j];
                    if (t == null)
                    {
                        continue;
                    }

                    if (t.Name == typeName)
                    {
                        return t;
                    }
                }
            }

            return null;
        }

        private void DrawCodePreview()
        {
            var baseStyle = new GUIStyle(EditorStyles.label)
            {
                richText = false,
                wordWrap = false
            };

            baseStyle.font = EditorStyles.textArea.font;
            baseStyle.normal.textColor = Color.white;

            var keywordStyle = new GUIStyle(baseStyle);
            keywordStyle.normal.textColor = new Color(0.5f, 0.8f, 1f);

            var typeStyle = new GUIStyle(baseStyle);
            typeStyle.normal.textColor = new Color(1f, 0.85f, 0.4f);

            _codeScroll = EditorGUILayout.BeginScrollView(_codeScroll);

            if (!string.IsNullOrEmpty(_previewCode))
            {
                var lines = _previewCode.Split('\n');
                for (int i = 0; i < lines.Length; i++)
                {
                    var line = lines[i];
                    var trimmed = line.TrimStart();
                    var styleToUse = baseStyle;

                    if (trimmed.StartsWith("using ") || trimmed.StartsWith("namespace ") ||
                        trimmed.StartsWith("public class") || trimmed.StartsWith("internal class") ||
                        trimmed.StartsWith("class "))
                    {
                        styleToUse = keywordStyle;
                    }

                    if (line.Contains("ViewMappingBase<"))
                    {
                        styleToUse = typeStyle;
                    }

                    EditorGUILayout.LabelField(line, styleToUse);
                }
            }

            EditorGUILayout.EndScrollView();
        }
    }
}
