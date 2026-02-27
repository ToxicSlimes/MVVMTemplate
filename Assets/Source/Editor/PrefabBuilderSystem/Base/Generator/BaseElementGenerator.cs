#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using Core.Editor.PrefabBuilderSystem.Elements;
using Core.Editor.PrefabBuilderSystem.Builders.MainMenu;
using Core.Editor.PrefabBuilderSystem.Builders.Counter;
using Core.Editor.PrefabBuilderSystem.Builders.Settings;
using Core.Editor.PrefabBuilderSystem.Builders.Tooltip;
using Core.Editor.PrefabBuilderSystem.Builders.ConfirmPopup;
using Core.Editor.PrefabBuilderSystem.Builders.Elements;
using System.IO;

namespace Core.Editor.PrefabBuilderSystem.Generator
{
    public class BaseElementGenerator : EditorWindow
    {
        private const string SettingsPath = "Assets/Source/Editor/PrefabBuilderSystem/Settings/BaseElementSettings.asset";
        
        private Vector2 _scrollPosition;
        private bool _atomsFoldout = true;
        private bool _interactiveFoldout = true;
        private bool _containersFoldout = true;
        private bool _gameUIFoldout = true;
        private bool _chatFoldout = true;
        private bool _mainViewFoldout = true;
        private bool _samplesFoldout = true;
        private bool _settingsFoldout = true;
        
        private BaseElementSettings _settings;
        private UnityEditor.Editor _settingsEditor;

        [MenuItem("Tools/UI Prefab Builder/Base Element Generator %#g", false, 10)]
        public static void ShowWindow()
        {
            var window = GetWindow<BaseElementGenerator>("Base Element Generator");
            window.minSize = new Vector2(450, 700);
            window.Show();
        }

        private void OnEnable()
        {
            LoadOrCreateSettings();
        }

        private void OnDisable()
        {
            CleanupSettingsEditor();
        }

        private void CleanupSettingsEditor()
        {
            if (_settingsEditor != null)
            {
                var editor = _settingsEditor;
                _settingsEditor = null;
                DestroyImmediate(editor);
            }
        }

        private void LoadOrCreateSettings()
        {
            _settings = AssetDatabase.LoadAssetAtPath<BaseElementSettings>(SettingsPath);

            if (_settings == null)
            {
                var directory = Path.GetDirectoryName(SettingsPath);
                if (!Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                _settings = CreateInstance<BaseElementSettings>();
                AssetDatabase.CreateAsset(_settings, SettingsPath);
                AssetDatabase.SaveAssets();
                Debug.Log($"[BaseElementGenerator] Created settings at {SettingsPath}");
            }

            CleanupSettingsEditor();

            if (_settings != null)
            {
                _settingsEditor = UnityEditor.Editor.CreateEditor(_settings);
            }
        }

        private void OnGUI()
        {
            EditorGUILayout.Space(10);
            DrawHeader();
            EditorGUILayout.Space(10);

            _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition);

            DrawSettingsSection();
            EditorGUILayout.Space(10);
            DrawAtomsSection();
            EditorGUILayout.Space(10);
            DrawInteractiveSection();
            EditorGUILayout.Space(10);
            DrawContainersSection();
            EditorGUILayout.Space(10);
            DrawGameUISection();
            EditorGUILayout.Space(10);
            DrawSampleElementsSection();
            EditorGUILayout.Space(10);
            DrawChatSection();
            EditorGUILayout.Space(10);
            DrawMainViewSection();
            EditorGUILayout.Space(20);

            DrawBulkActions();

            EditorGUILayout.EndScrollView();
        }
        
        private void DrawSettingsSection()
        {
            _settingsFoldout = EditorGUILayout.BeginFoldoutHeaderGroup(_settingsFoldout, "⚙️ Settings");
            if (_settingsFoldout && _settings != null)
            {
                EditorGUI.indentLevel++;
                
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.ObjectField("Settings Asset", _settings, typeof(BaseElementSettings), false);
                if (GUILayout.Button("Select", GUILayout.Width(60)))
                {
                    Selection.activeObject = _settings;
                    EditorGUIUtility.PingObject(_settings);
                }
                EditorGUILayout.EndHorizontal();
                
                EditorGUILayout.HelpBox(
                    "Настройки размеров и параметров элементов.\n" +
                    "Измени значения и нажми Regenerate для обновления префабов.",
                    MessageType.Info);
                
                EditorGUI.indentLevel--;
            }
            EditorGUILayout.EndFoldoutHeaderGroup();
        }

        private void DrawHeader()
        {
            GUILayout.Label("🧱 Base Element Generator", new GUIStyle(EditorStyles.boldLabel)
            {
                fontSize = 18,
                alignment = TextAnchor.MiddleCenter
            });

            EditorGUILayout.HelpBox(
                "Генерация универсальных UI префабов.\n" +
                "Создай базовые элементы один раз — используй везде через UI.Element().FromPrefab()",
                MessageType.Info);
        }

        private void DrawAtomsSection()
        {
            _atomsFoldout = EditorGUILayout.BeginFoldoutHeaderGroup(_atomsFoldout, "🧱 Atoms (базовые)");
            if (_atomsFoldout)
            {
                EditorGUI.indentLevel++;
                DrawElementRow("Text", ElementPaths.Atoms.Text, GenerateText);
                DrawElementRow("Image", ElementPaths.Atoms.Image, GenerateImage);
                DrawElementRow("RawImage", ElementPaths.Atoms.RawImage, GenerateRawImage);
                DrawElementRow("Icon", ElementPaths.Atoms.Icon, GenerateIcon);
                DrawElementRow("Spacer", ElementPaths.Atoms.Spacer, GenerateSpacer);
                DrawElementRow("Divider", ElementPaths.Atoms.Divider, GenerateDivider);
                EditorGUI.indentLevel--;
            }
            EditorGUILayout.EndFoldoutHeaderGroup();
        }

        private void DrawInteractiveSection()
        {
            _interactiveFoldout = EditorGUILayout.BeginFoldoutHeaderGroup(_interactiveFoldout, "🎛️ Interactive");
            if (_interactiveFoldout)
            {
                EditorGUI.indentLevel++;
                DrawElementRow("Button", ElementPaths.Interactive.Button, GenerateButton);
                DrawElementRow("Toggle", ElementPaths.Interactive.Toggle, GenerateToggle);
                DrawElementRow("Slider", ElementPaths.Interactive.Slider, GenerateSlider);
                DrawElementRow("InputField", ElementPaths.Interactive.InputField, GenerateInputField);
                DrawElementRow("Dropdown", ElementPaths.Interactive.Dropdown, GenerateDropdown);
                DrawElementRow("Scrollbar", ElementPaths.Interactive.Scrollbar, GenerateScrollbar);
                DrawElementRow("ProgressBar", ElementPaths.Interactive.ProgressBar, GenerateProgressBar);
                DrawElementRow("TabButton", ElementPaths.Interactive.TabButton, GenerateTabButton);
                DrawElementRow("RadioButton", ElementPaths.Interactive.RadioButton, GenerateRadioButton);
                DrawElementRow("Stepper", ElementPaths.Interactive.Stepper, GenerateStepper);
                EditorGUI.indentLevel--;
            }
            EditorGUILayout.EndFoldoutHeaderGroup();
        }

        private void DrawContainersSection()
        {
            _containersFoldout = EditorGUILayout.BeginFoldoutHeaderGroup(_containersFoldout, "📦 Containers");
            if (_containersFoldout)
            {
                EditorGUI.indentLevel++;
                DrawElementRow("HorizontalLayout", ElementPaths.Containers.HorizontalLayout, GenerateHorizontalLayout);
                DrawElementRow("VerticalLayout", ElementPaths.Containers.VerticalLayout, GenerateVerticalLayout);
                DrawElementRow("GridLayout", ElementPaths.Containers.GridLayout, GenerateGridLayout);
                DrawElementRow("ScrollView", ElementPaths.Containers.ScrollView, GenerateScrollView);
                DrawElementRow("Card", ElementPaths.Containers.Card, GenerateCard);
                DrawElementRow("Panel", ElementPaths.Containers.Panel, GeneratePanel);
                DrawElementRow("Modal", ElementPaths.Containers.Modal, GenerateModal);
                DrawElementRow("TabBar", ElementPaths.Containers.TabBar, GenerateTabBar);
                DrawElementRow("Accordion", ElementPaths.Containers.Accordion, GenerateAccordion);
                EditorGUI.indentLevel--;
            }
            EditorGUILayout.EndFoldoutHeaderGroup();
        }

        private void DrawGameUISection()
        {
            _gameUIFoldout = EditorGUILayout.BeginFoldoutHeaderGroup(_gameUIFoldout, "🎮 Game UI");
            if (_gameUIFoldout)
            {
                EditorGUI.indentLevel++;
                DrawElementRow("HealthBar", ElementPaths.GameUI.HealthBar, GenerateHealthBar);
                DrawElementRow("Badge", ElementPaths.GameUI.Badge, GenerateBadge);
                DrawElementRow("Avatar", ElementPaths.GameUI.Avatar, GenerateAvatar);
                EditorGUI.indentLevel--;
            }
            EditorGUILayout.EndFoldoutHeaderGroup();
        }

        private void DrawSampleElementsSection()
        {
            _samplesFoldout = EditorGUILayout.BeginFoldoutHeaderGroup(_samplesFoldout, "📋 Sample BindableElements");
            if (_samplesFoldout)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.HelpBox(
                    "BindableElement<TData> примеры для Element Gallery.\n" +
                    "Собираются из базовых префабов (Card, Text, Button, ProgressBar).\n" +
                    "Включают: _clickButton, _selectionIndicator, _popupTrigger + custom fields.",
                    MessageType.Info);

                DrawElementRow("TextCard", ElementPaths.Samples.TextCard, SampleElementBuilders.BuildTextCard);
                DrawElementRow("StatBar", ElementPaths.Samples.StatBar, SampleElementBuilders.BuildStatBar);
                DrawElementRow("ActionListItem", ElementPaths.Samples.ActionListItem, SampleElementBuilders.BuildActionListItem);

                EditorGUILayout.Space(4);
                EditorGUILayout.BeginHorizontal();
                GUI.backgroundColor = new Color(0.3f, 0.7f, 0.9f);
                if (GUILayout.Button("Build All Samples", GUILayout.Height(28)))
                {
                    SampleElementBuilders.BuildAll();
                }
                GUI.backgroundColor = Color.white;
                EditorGUILayout.EndHorizontal();

                EditorGUI.indentLevel--;
            }
            EditorGUILayout.EndFoldoutHeaderGroup();
        }

        private void DrawChatSection()
        {
            _chatFoldout = EditorGUILayout.BeginFoldoutHeaderGroup(_chatFoldout, "💬 Chat");
            if (_chatFoldout)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.HelpBox(
                    "Элементы чата со своими Builder Windows.\n" +
                    "Используй Open Builder для настройки параметров.",
                    MessageType.Info);
                EditorGUI.indentLevel--;
            }
            EditorGUILayout.EndFoldoutHeaderGroup();
        }

        private void DrawMainViewSection()
        {
            _mainViewFoldout = EditorGUILayout.BeginFoldoutHeaderGroup(_mainViewFoldout, "🖼️ View Prefabs (Builder Windows)");
            if (_mainViewFoldout)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.HelpBox(
                    "View префабы с MVVM wiring.\n" +
                    "Open Builder → настрой параметры → BUILD → префаб со всеми SerializeField привязками.",
                    MessageType.Info);

                DrawBuilderWindowRow("MainMenuView", "Assets/Prefabs/Views/MainMenuView.prefab",
                    typeof(MainMenuViewBuilderWindow));
                DrawBuilderWindowRow("CounterView", "Assets/Prefabs/Views/CounterView.prefab",
                    typeof(CounterViewBuilderWindow));
                DrawBuilderWindowRow("SettingsView", "Assets/Prefabs/Views/SettingsView.prefab",
                    typeof(SettingsViewBuilderWindow));

                EditorGUILayout.Space(10);
                EditorGUILayout.LabelField("Popups", EditorStyles.boldLabel);

                DrawBuilderWindowRow("TooltipPopup", "Assets/Prefabs/Popups/TooltipPopup.prefab",
                    typeof(TooltipViewBuilderWindow));
                DrawBuilderWindowRow("ConfirmPopup", "Assets/Prefabs/Popups/ConfirmPopup.prefab",
                    typeof(ConfirmPopupBuilderWindow));

                EditorGUI.indentLevel--;
            }
            EditorGUILayout.EndFoldoutHeaderGroup();
        }

        private void DrawExistingPrefabRow(string name, string path)
        {
            EditorGUILayout.BeginHorizontal();

            var exists = AssetDatabase.LoadAssetAtPath<GameObject>(path) != null;
            var statusIcon = exists ? "✅" : "❌";

            GUILayout.Label($"{statusIcon} {name}", GUILayout.Width(150));
            GUILayout.Label("(manual prefab)", EditorStyles.miniLabel, GUILayout.Width(100));

            if (exists)
            {
                if (GUILayout.Button("Select", GUILayout.Width(60)))
                {
                    var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                    Selection.activeObject = prefab;
                    EditorGUIUtility.PingObject(prefab);
                }
            }

            EditorGUILayout.EndHorizontal();
        }

        private void DrawBuilderWindowRow(string name, string path, System.Type windowType)
        {
            EditorGUILayout.BeginHorizontal();

            var exists = AssetDatabase.LoadAssetAtPath<GameObject>(path) != null;
            var statusIcon = exists ? "✅" : "❌";

            GUILayout.Label($"{statusIcon} {name}", GUILayout.Width(150));

            if (GUILayout.Button("Open Builder", GUILayout.Width(100)))
            {
                EditorWindow.GetWindow(windowType);
            }

            if (exists)
            {
                if (GUILayout.Button("Select", GUILayout.Width(60)))
                {
                    var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                    Selection.activeObject = prefab;
                    EditorGUIUtility.PingObject(prefab);
                }
            }

            EditorGUILayout.EndHorizontal();
        }

        private void DrawElementRow(string name, string path, System.Action generateAction, System.Action wireAction = null)
        {
            EditorGUILayout.BeginHorizontal();

            var exists = AssetDatabase.LoadAssetAtPath<GameObject>(path) != null;
            var statusIcon = exists ? "✅" : "❌";

            GUILayout.Label($"{statusIcon} {name}", GUILayout.Width(150));

            GUI.enabled = !exists;
            if (GUILayout.Button("Generate", GUILayout.Width(80)))
            {
                generateAction();
            }
            GUI.enabled = true;

            if (exists)
            {
                if (GUILayout.Button("Regenerate", GUILayout.Width(80)))
                {
                    if (EditorUtility.DisplayDialog("Regenerate?",
                        $"Перезаписать {name}?", "Да", "Отмена"))
                    {
                        generateAction();
                    }
                }

                if (wireAction != null)
                {
                    GUI.backgroundColor = new Color(0.4f, 0.8f, 1f);
                    if (GUILayout.Button("Wire", GUILayout.Width(50)))
                    {
                        wireAction();
                    }
                    GUI.backgroundColor = Color.white;
                }

                if (GUILayout.Button("Select", GUILayout.Width(60)))
                {
                    var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                    Selection.activeObject = prefab;
                    EditorGUIUtility.PingObject(prefab);
                }
            }

            EditorGUILayout.EndHorizontal();
        }

        private void DrawBulkActions()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            GUILayout.Label("Bulk Actions", EditorStyles.boldLabel);
            
            EditorGUILayout.HelpBox(
                "Генерирует только базовые элементы (Atoms, Interactive, Containers, GameUI).\n" +
                "Chat и MainView элементы требуют Builder Windows для кастомных настроек.",
                MessageType.None);

            EditorGUILayout.BeginHorizontal();

            GUI.backgroundColor = new Color(0.3f, 0.8f, 0.3f);
            if (GUILayout.Button("🔨 Generate All Missing", GUILayout.Height(35)))
            {
                GenerateAllMissing();
            }

            GUI.backgroundColor = new Color(0.8f, 0.6f, 0.3f);
            if (GUILayout.Button("🔄 Regenerate All", GUILayout.Height(35)))
            {
                if (EditorUtility.DisplayDialog("Regenerate All?",
                    "Это перезапишет ВСЕ базовые префабы. Продолжить?", "Да", "Отмена"))
                {
                    GenerateAll();
                }
            }
            GUI.backgroundColor = Color.white;

            EditorGUILayout.EndHorizontal();

            EditorGUILayout.EndVertical();
        }

        private void GenerateAllMissing()
        {
            var count = 0;

            // Atoms
            if (!Exists(ElementPaths.Atoms.Text)) { GenerateText(); count++; }
            if (!Exists(ElementPaths.Atoms.Image)) { GenerateImage(); count++; }
            if (!Exists(ElementPaths.Atoms.RawImage)) { GenerateRawImage(); count++; }
            if (!Exists(ElementPaths.Atoms.Icon)) { GenerateIcon(); count++; }
            if (!Exists(ElementPaths.Atoms.Spacer)) { GenerateSpacer(); count++; }
            if (!Exists(ElementPaths.Atoms.Divider)) { GenerateDivider(); count++; }

            // Interactive
            if (!Exists(ElementPaths.Interactive.Button)) { GenerateButton(); count++; }
            if (!Exists(ElementPaths.Interactive.Toggle)) { GenerateToggle(); count++; }
            if (!Exists(ElementPaths.Interactive.Slider)) { GenerateSlider(); count++; }
            if (!Exists(ElementPaths.Interactive.InputField)) { GenerateInputField(); count++; }
            if (!Exists(ElementPaths.Interactive.Dropdown)) { GenerateDropdown(); count++; }
            if (!Exists(ElementPaths.Interactive.Scrollbar)) { GenerateScrollbar(); count++; }
            if (!Exists(ElementPaths.Interactive.ProgressBar)) { GenerateProgressBar(); count++; }
            if (!Exists(ElementPaths.Interactive.TabButton)) { GenerateTabButton(); count++; }
            if (!Exists(ElementPaths.Interactive.RadioButton)) { GenerateRadioButton(); count++; }
            if (!Exists(ElementPaths.Interactive.Stepper)) { GenerateStepper(); count++; }

            // Containers
            if (!Exists(ElementPaths.Containers.HorizontalLayout)) { GenerateHorizontalLayout(); count++; }
            if (!Exists(ElementPaths.Containers.VerticalLayout)) { GenerateVerticalLayout(); count++; }
            if (!Exists(ElementPaths.Containers.GridLayout)) { GenerateGridLayout(); count++; }
            if (!Exists(ElementPaths.Containers.ScrollView)) { GenerateScrollView(); count++; }
            if (!Exists(ElementPaths.Containers.Card)) { GenerateCard(); count++; }
            if (!Exists(ElementPaths.Containers.Panel)) { GeneratePanel(); count++; }
            if (!Exists(ElementPaths.Containers.Modal)) { GenerateModal(); count++; }
            if (!Exists(ElementPaths.Containers.TabBar)) { GenerateTabBar(); count++; }
            if (!Exists(ElementPaths.Containers.Accordion)) { GenerateAccordion(); count++; }

            // Game UI
            if (!Exists(ElementPaths.GameUI.HealthBar)) { GenerateHealthBar(); count++; }
            if (!Exists(ElementPaths.GameUI.Badge)) { GenerateBadge(); count++; }
            if (!Exists(ElementPaths.GameUI.Avatar)) { GenerateAvatar(); count++; }

            AssetDatabase.Refresh();
            Debug.Log($"[BaseElementGenerator] Generated {count} prefabs");
        }

        private void GenerateAll()
        {
            GenerateText();
            GenerateImage();
            GenerateRawImage();
            GenerateIcon();
            GenerateSpacer();
            GenerateDivider();

            GenerateButton();
            GenerateToggle();
            GenerateSlider();
            GenerateInputField();
            GenerateDropdown();
            GenerateScrollbar();
            GenerateProgressBar();
            GenerateTabButton();
            GenerateRadioButton();
            GenerateStepper();

            GenerateHorizontalLayout();
            GenerateVerticalLayout();
            GenerateGridLayout();
            GenerateScrollView();
            GenerateCard();
            GeneratePanel();
            GenerateModal();
            GenerateTabBar();
            GenerateAccordion();

            GenerateHealthBar();
            GenerateBadge();
            GenerateAvatar();

            AssetDatabase.Refresh();
            Debug.Log("[BaseElementGenerator] Regenerated ALL prefabs");
        }

        private bool Exists(string path) => AssetDatabase.LoadAssetAtPath<GameObject>(path) != null;

        // === ATOMS ===
        private void GenerateText()
        {
            var s = _settings.text;
            UI.Text("Text").Body().WithFontSize(s.fontSize).WithPreferredSize(null, s.preferredHeight)
                .SaveAsPrefab(ElementPaths.Atoms.Text);
        }

        private void GenerateImage()
        {
            var s = _settings.image;
            UI.Image("Image").WithPreferredSize(s.width, s.height).SaveAsPrefab(ElementPaths.Atoms.Image);
        }

        private void GenerateRawImage()
        {
            var s = _settings.image;
            UI.RawImage("RawImage").WithPreferredSize(s.width, s.height).SaveAsPrefab(ElementPaths.Atoms.RawImage);
        }

        private void GenerateIcon()
        {
            var s = _settings.icon;
            UI.Icon("Icon").WithSize(s.size).SaveAsPrefab(ElementPaths.Atoms.Icon);
        }

        private void GenerateSpacer()
        {
            var s = _settings.spacer;
            UI.Spacer("Spacer").Height(s.height).SaveAsPrefab(ElementPaths.Atoms.Spacer);
        }

        private void GenerateDivider()
        {
            var s = _settings.divider;
            UI.Divider("Divider").Themed().WithThickness(s.thickness).SaveAsPrefab(ElementPaths.Atoms.Divider);
        }

        // === INTERACTIVE ===
        private void GenerateButton()
        {
            var s = _settings.button;
            UI.Button("Button").Primary().WithLabel(s.defaultLabel).WithLabelSize(s.fontSize)
                .WithPreferredSize(s.width, s.height).SaveAsPrefab(ElementPaths.Interactive.Button);
        }

        private void GenerateToggle()
        {
            var s = _settings.toggle;
            UI.Toggle("Toggle").Themed().WithToggleSize(s.size)
                .WithPreferredSize(s.size, s.size).SaveAsPrefab(ElementPaths.Interactive.Toggle);
        }

        private void GenerateSlider()
        {
            var s = _settings.slider;
            UI.Slider("Slider").Themed()
                .WithHeight(s.height).WithHandleSize(s.handleSize).WithValue(s.defaultValue)
                .WithTrackOffset(s.trackOffset).WithHandleAreaOffset(s.handleAreaOffset)
                .WithPreferredSize(s.width, s.height).SaveAsPrefab(ElementPaths.Interactive.Slider);
        }

        private void GenerateInputField()
        {
            var s = _settings.inputField;
            UI.InputField("InputField").Themed()
                .WithPlaceholder(s.placeholder).WithFontSize(s.fontSize)
                .WithPadding(s.padding, s.textAreaVerticalOffset)
                .WithPreferredSize(s.width, s.height).SaveAsPrefab(ElementPaths.Interactive.InputField);
        }

        private void GenerateDropdown()
        {
            var s = _settings.dropdown;
            UI.Dropdown("Dropdown").Themed().WithFontSize(s.fontSize)
                .WithPreferredSize(s.width, s.height).SaveAsPrefab(ElementPaths.Interactive.Dropdown);
        }

        private void GenerateScrollbar()
        {
            var s = _settings.scrollbar;
            UI.Scrollbar("Scrollbar").Vertical().Themed().WithSize(s.width)
                .SaveAsPrefab(ElementPaths.Interactive.Scrollbar);
        }

        private void GenerateProgressBar()
        {
            var s = _settings.progressBar;
            UI.ProgressBar("ProgressBar").Themed().WithValue(s.defaultValue).WithHeight(s.height)
                .SaveAsPrefab(ElementPaths.Interactive.ProgressBar);
        }

        private void GenerateTabButton()
        {
            var s = _settings.tabButton;
            UI.TabButton("TabButton").Themed().WithLabel("Tab")
                .WithMinWidth(s.minWidth).WithHeight(s.height).WithFontSize(s.fontSize)
                .SaveAsPrefab(ElementPaths.Interactive.TabButton);
        }

        private void GenerateRadioButton()
        {
            var s = _settings.radioButton;
            UI.RadioButton("RadioButton").Themed().WithLabel("Option").WithRadioSize(s.radioSize)
                .SaveAsPrefab(ElementPaths.Interactive.RadioButton);
        }

        private void GenerateStepper()
        {
            var s = _settings.stepper;
            UI.Stepper("Stepper").Themed().WithValue(0).WithRange(0, 100)
                .WithButtonSize(s.buttonSize).WithValueWidth(s.valueWidth).WithFontSize(s.fontSize)
                .SaveAsPrefab(ElementPaths.Interactive.Stepper);
        }

        // === CONTAINERS ===
        private void GenerateHorizontalLayout()
        {
            var s = _settings.horizontalLayout;
            UI.HorizontalLayout("HorizontalLayout").WithSpacing(s.spacing)
                .WithPadding(s.paddingLeft, s.paddingRight, s.paddingTop, s.paddingBottom)
                .SaveAsPrefab(ElementPaths.Containers.HorizontalLayout);
        }

        private void GenerateVerticalLayout()
        {
            var s = _settings.verticalLayout;
            UI.VerticalLayout("VerticalLayout").WithSpacing(s.spacing)
                .WithPadding(s.paddingLeft, s.paddingRight, s.paddingTop, s.paddingBottom)
                .SaveAsPrefab(ElementPaths.Containers.VerticalLayout);
        }

        private void GenerateGridLayout()
        {
            var s = _settings.grid;
            UI.Grid("GridLayout").WithCellSize(s.cellWidth, s.cellHeight).WithSpacing(s.spacingX, s.spacingY)
                .SaveAsPrefab(ElementPaths.Containers.GridLayout);
        }

        private void GenerateScrollView()
        {
            var s = _settings.scrollView;
            UI.ScrollView("ScrollView").Themed().WithScrollSensitivity(s.scrollSensitivity)
                .WithPreferredSize(s.width, s.height).SaveAsPrefab(ElementPaths.Containers.ScrollView);
        }

        private void GenerateCard()
        {
            var s = _settings.card;
            UI.Card("Card").Themed().WithPadding(s.padding).SaveAsPrefab(ElementPaths.Containers.Card);
        }

        private void GeneratePanel()
        {
            var s = _settings.panel;
            UI.Panel("Panel").Themed().WithTitle("Panel")
                .WithHeaderHeight(s.headerHeight).WithContentPadding(s.contentPadding).WithContentSpacing(s.contentSpacing)
                .SaveAsPrefab(ElementPaths.Containers.Panel);
        }

        private void GenerateModal()
        {
            var s = _settings.modal;
            UI.Modal("Modal").Themed().WithTitle("Modal").WithSize(s.width, s.height)
                .SaveAsPrefab(ElementPaths.Containers.Modal);
        }

        private void GenerateTabBar()
        {
            var s = _settings.tabBar;
            UI.TabBar("TabBar").Themed().WithHeight(s.height).WithSpacing(s.spacing).WithPadding(s.padding)
                .SaveAsPrefab(ElementPaths.Containers.TabBar);
        }

        private void GenerateAccordion()
        {
            var s = _settings.accordion;
            UI.Accordion("Accordion").Themed().WithTitle("Section")
                .WithHeaderHeight(s.headerHeight).WithContentPadding(s.contentPadding).WithContentSpacing(s.contentSpacing)
                .SaveAsPrefab(ElementPaths.Containers.Accordion);
        }

        // === GAME UI ===
        private void GenerateHealthBar()
        {
            var s = _settings.healthBar;
            UI.HealthBar("HealthBar")
                .Health()
                .WithHeight(s.height)
                .WithFontSize(s.fontSize)
                .WithValue(s.defaultValue)
                .WithFillColor(s.fillColor)
                .WithBackgroundColor(s.backgroundColor)
                .WithTextColor(s.textColor)
                .ShowText()
                .SaveAsPrefab(ElementPaths.GameUI.HealthBar);
        }

        private void GenerateBadge()
        {
            var s = _settings.badge;
            UI.Badge("Badge")
                .Level()
                .WithSize(s.size)
                .WithFontSize(s.fontSize)
                .WithText("1")
                .WithBackgroundColor(s.backgroundColor)
                .WithTextColor(s.textColor)
                .WithOutline(s.outlineColor)
                .SaveAsPrefab(ElementPaths.GameUI.Badge);
        }

        private void GenerateAvatar()
        {
            var s = _settings.avatar;
            UI.Avatar("Avatar")
                .WithSize(s.size)
                .WithBorder(s.borderColor, s.borderWidth)
                .WithBackgroundColor(s.backgroundColor)
                .SaveAsPrefab(ElementPaths.GameUI.Avatar);
        }
    }
}
#endif
