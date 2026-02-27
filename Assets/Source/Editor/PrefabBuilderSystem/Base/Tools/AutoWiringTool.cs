#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Core.Editor.PrefabBuilderSystem.Conventions;
using UnityEditor;
using UnityEngine;

namespace Core.Editor.PrefabBuilderSystem.Tools
{
    public class AutoWiringTool : EditorWindow
    {
        private GameObject _targetPrefab;
        private int _selectedViewTypeIndex;
        private string[] _viewTypeNames = Array.Empty<string>();
        private Type[] _viewTypes = Array.Empty<Type>();
        private List<WiringInfo> _wiringInfos = new();
        private Vector2 _scrollPos;
        private bool _addViewComponentIfMissing = true;
        private bool _savePrefabAfterWiring = true;

        [MenuItem("Tools/UI Prefab Builder/Auto Wire View")]
        public static void ShowWindow()
        {
            var window = GetWindow<AutoWiringTool>("Auto Wire View");
            window.minSize = new Vector2(550, 500);
        }

        private void OnEnable()
        {
            RefreshViewTypes();
        }

        private void OnGUI()
        {
            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("Auto Wire View", EditorStyles.boldLabel);
            EditorGUILayout.Space(5);

            DrawSettings();
            EditorGUILayout.Space(10);
            DrawOptions();
            EditorGUILayout.Space(10);
            DrawWiringTable();
            EditorGUILayout.Space(10);
            DrawButtons();
        }

        private void DrawSettings()
        {
            EditorGUI.BeginChangeCheck();
            _targetPrefab = (GameObject)EditorGUILayout.ObjectField("Target Prefab", _targetPrefab, typeof(GameObject), false);
            
            if (_viewTypeNames.Length > 0)
            {
                _selectedViewTypeIndex = EditorGUILayout.Popup("View Type", _selectedViewTypeIndex, _viewTypeNames);
            }
            else
            {
                EditorGUILayout.HelpBox("No BindableView types found in project", MessageType.Warning);
            }

            if (EditorGUI.EndChangeCheck())
            {
                RefreshWiringInfo();
            }
        }

        private void DrawOptions()
        {
            EditorGUILayout.LabelField("Options", EditorStyles.boldLabel);
            _addViewComponentIfMissing = EditorGUILayout.Toggle("Add View component if missing", _addViewComponentIfMissing);
            _savePrefabAfterWiring = EditorGUILayout.Toggle("Save prefab after wiring", _savePrefabAfterWiring);
        }

        private void DrawWiringTable()
        {
            if (_wiringInfos.Count == 0)
            {
                EditorGUILayout.HelpBox("Select a prefab and view type to see wiring preview", MessageType.Info);
                return;
            }

            EditorGUILayout.LabelField($"Wiring Preview ({_wiringInfos.Count} fields)", EditorStyles.boldLabel);

            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
            EditorGUILayout.LabelField("Field", EditorStyles.toolbarButton, GUILayout.Width(180));
            EditorGUILayout.LabelField("←", GUILayout.Width(20));
            EditorGUILayout.LabelField("Element", EditorStyles.toolbarButton, GUILayout.Width(180));
            EditorGUILayout.LabelField("Status", EditorStyles.toolbarButton);
            EditorGUILayout.EndHorizontal();

            _scrollPos = EditorGUILayout.BeginScrollView(_scrollPos, GUILayout.Height(250));

            foreach (var info in _wiringInfos)
            {
                EditorGUILayout.BeginHorizontal();
                
                var fieldColor = info.Status == WiringStatus.Found ? Color.white : 
                    info.Status == WiringStatus.AlreadyWired ? Color.cyan : Color.yellow;
                var prevColor = GUI.color;
                GUI.color = fieldColor;
                
                EditorGUILayout.LabelField(info.FieldName, GUILayout.Width(180));
                EditorGUILayout.LabelField("←", GUILayout.Width(20));
                EditorGUILayout.LabelField(info.ElementName ?? "(not found)", GUILayout.Width(180));
                
                var statusIcon = info.Status switch
                {
                    WiringStatus.Found => "✓ Found",
                    WiringStatus.AlreadyWired => "● Wired",
                    WiringStatus.NotFound => "✗ Missing",
                    WiringStatus.TypeMismatch => "! Type mismatch",
                    _ => "?"
                };
                EditorGUILayout.LabelField(statusIcon);
                
                GUI.color = prevColor;
                EditorGUILayout.EndHorizontal();
            }

            EditorGUILayout.EndScrollView();

            var foundCount = _wiringInfos.Count(w => w.Status == WiringStatus.Found);
            var wiredCount = _wiringInfos.Count(w => w.Status == WiringStatus.AlreadyWired);
            var missingCount = _wiringInfos.Count(w => w.Status == WiringStatus.NotFound);
            
            EditorGUILayout.LabelField($"Found: {foundCount} | Already Wired: {wiredCount} | Missing: {missingCount}", EditorStyles.miniLabel);
        }

        private void DrawButtons()
        {
            EditorGUILayout.BeginHorizontal();
            
            if (GUILayout.Button("Refresh", GUILayout.Height(25)))
            {
                RefreshViewTypes();
                RefreshWiringInfo();
            }

            var canWire = _targetPrefab != null && 
                          _viewTypes.Length > 0 && 
                          _wiringInfos.Any(w => w.Status == WiringStatus.Found);

            EditorGUI.BeginDisabledGroup(!canWire);
            if (GUILayout.Button("Wire All Found", GUILayout.Height(25)))
            {
                WireAll();
            }
            EditorGUI.EndDisabledGroup();

            EditorGUILayout.EndHorizontal();
        }

        private void RefreshViewTypes()
        {
            var bindableViewType = GetBindableViewBaseType();
            if (bindableViewType == null)
            {
                _viewTypes = Array.Empty<Type>();
                _viewTypeNames = Array.Empty<string>();
                return;
            }

            _viewTypes = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(a =>
                {
                    try { return a.GetTypes(); }
                    catch { return Array.Empty<Type>(); }
                })
                .Where(t => !t.IsAbstract && IsSubclassOfBindableView(t, bindableViewType))
                .OrderBy(t => t.Name)
                .ToArray();

            _viewTypeNames = _viewTypes.Select(t => t.Name).ToArray();
            _selectedViewTypeIndex = Mathf.Clamp(_selectedViewTypeIndex, 0, Mathf.Max(0, _viewTypes.Length - 1));
        }

        private Type GetBindableViewBaseType()
        {
            return AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(a =>
                {
                    try { return a.GetTypes(); }
                    catch { return Array.Empty<Type>(); }
                })
                .FirstOrDefault(t => t.Name == "BindableView`1" && t.IsGenericTypeDefinition);
        }

        private bool IsSubclassOfBindableView(Type type, Type bindableViewType)
        {
            var current = type.BaseType;
            while (current != null)
            {
                if (current.IsGenericType && current.GetGenericTypeDefinition() == bindableViewType)
                    return true;
                current = current.BaseType;
            }
            return false;
        }

        private void RefreshWiringInfo()
        {
            _wiringInfos.Clear();

            if (_targetPrefab == null || _viewTypes.Length == 0) return;

            var viewType = _viewTypes[_selectedViewTypeIndex];
            var fields = viewType
                .GetFields(BindingFlags.NonPublic | BindingFlags.Instance)
                .Where(f => f.GetCustomAttribute<SerializeField>() != null)
                .ToList();

            var existingView = _targetPrefab.GetComponent(viewType);

            foreach (var field in fields)
            {
                var info = new WiringInfo
                {
                    FieldName = field.Name,
                    FieldType = field.FieldType,
                    Field = field
                };

                var elementName = NamingConvention.ToElementName(field.Name, field.FieldType);
                info.ElementName = elementName;

                if (elementName != null)
                {
                    var element = FindElementInHierarchy(_targetPrefab.transform, elementName);
                    if (element != null)
                    {
                        info.Element = element;
                        
                        if (existingView != null)
                        {
                            var currentValue = field.GetValue(existingView);
                            if (currentValue != null)
                            {
                                info.Status = WiringStatus.AlreadyWired;
                            }
                            else
                            {
                                info.Status = WiringStatus.Found;
                            }
                        }
                        else
                        {
                            info.Status = WiringStatus.Found;
                        }
                    }
                    else
                    {
                        info.Status = WiringStatus.NotFound;
                    }
                }
                else
                {
                    info.Status = WiringStatus.NotFound;
                }

                _wiringInfos.Add(info);
            }
        }

        private Transform FindElementInHierarchy(Transform root, string name)
        {
            if (root.name == name) return root;
            foreach (Transform child in root)
            {
                var found = FindElementInHierarchy(child, name);
                if (found != null) return found;
            }
            return null;
        }

        private void WireAll()
        {
            if (_targetPrefab == null || _viewTypes.Length == 0) return;

            var viewType = _viewTypes[_selectedViewTypeIndex];
            var prefabPath = AssetDatabase.GetAssetPath(_targetPrefab);
            var isAsset = !string.IsNullOrEmpty(prefabPath);

            GameObject instance;
            if (isAsset)
            {
                instance = (GameObject)PrefabUtility.InstantiatePrefab(_targetPrefab);
            }
            else
            {
                instance = _targetPrefab;
            }

            var view = instance.GetComponent(viewType);
            if (view == null)
            {
                if (_addViewComponentIfMissing)
                {
                    view = instance.AddComponent(viewType);
                }
                else
                {
                    if (isAsset) DestroyImmediate(instance);
                    EditorUtility.DisplayDialog("Error", "View component not found on prefab", "OK");
                    return;
                }
            }

            var so = new SerializedObject(view);
            var wiredCount = 0;

            foreach (var info in _wiringInfos.Where(w => w.Status == WiringStatus.Found))
            {
                WireField(so, info, instance);
                wiredCount++;
            }

            so.ApplyModifiedProperties();

            if (isAsset && _savePrefabAfterWiring)
            {
                PrefabUtility.SaveAsPrefabAsset(instance, prefabPath);
                DestroyImmediate(instance);
                _targetPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            }
            else if (isAsset)
            {
                DestroyImmediate(instance);
            }

            RefreshWiringInfo();
            EditorUtility.DisplayDialog("Success", $"Wired {wiredCount} fields", "OK");
        }

        private void WireField(SerializedObject so, WiringInfo info, GameObject prefab)
        {
            if (info.ElementName == null || info.Element == null) return;

            UnityEngine.Object component = null;

            if (info.FieldType == typeof(GameObject))
            {
                component = info.Element.gameObject;
            }
            else if (info.FieldType == typeof(RectTransform))
            {
                component = info.Element.GetComponent<RectTransform>();
            }
            else if (info.FieldType == typeof(Transform))
            {
                component = info.Element;
            }
            else
            {
                component = info.Element.GetComponent(info.FieldType);
            }

            if (component != null)
            {
                var prop = so.FindProperty(info.FieldName);
                if (prop != null)
                {
                    prop.objectReferenceValue = component;
                }
            }
        }

        private class WiringInfo
        {
            public string FieldName;
            public Type FieldType;
            public FieldInfo Field;
            public string ElementName;
            public Transform Element;
            public WiringStatus Status;
        }

        private enum WiringStatus
        {
            Found,
            AlreadyWired,
            NotFound,
            TypeMismatch
        }
    }
}
#endif
