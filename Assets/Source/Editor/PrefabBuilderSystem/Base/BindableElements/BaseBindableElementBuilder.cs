#if UNITY_EDITOR
using System.IO;
using Core.Abstractions.MVVM.Elements;
using Core.Infrastructure.MVVM.Config;
using Core.Editor.PrefabBuilderSystem.Core;
using Core.Abstractions.Popup;
using Core.Data.Domain.Models.Elements;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace Core.Editor.PrefabBuilderSystem.BindableElements
{
    public abstract class BaseBindableElementBuilder<TElement, TData>
        where TElement : BindableElement<TData>
        where TData : struct, IElementData
    {
        public PrefabBuilderContext Context { get; private set; }

        protected abstract string PrefabPath { get; }
        protected abstract string ElementName { get; }

        protected virtual bool AddClickButton => true;
        protected virtual bool AddSelectionIndicator => false;
        protected virtual bool AddPopupTrigger => false;

        public void Build()
        {
            EnsureDirectoryExists(Path.GetDirectoryName(PrefabPath));

            Context = new PrefabBuilderContext
            {
                Root = new GameObject(ElementName, typeof(RectTransform))
            };

            SetupRootRectTransform();
            BuildElementStructure();

            var element = Context.Root.AddComponent<TElement>();
            WireBaseFields(element);
            WireFields(element);

            SavePrefab();
            Cleanup();

            Debug.Log($"<color=green>✓ Element prefab created: {PrefabPath}</color>");
        }

        protected virtual void SetupRootRectTransform()
        {
            var rt = Context.Root.GetComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
        }

        protected abstract void BuildElementStructure();

        protected virtual void WireBaseFields(TElement element)
        {
            var so = new SerializedObject(element);

            if (AddClickButton && Context.TryGet<Button>("ClickButton", out var clickButton))
            {
                SerializationHelper.Wire(so, "_clickButton", clickButton);
            }

            if (AddSelectionIndicator)
            {
                var selectionIndicator = Context.GetGameObject("SelectionIndicator");
                if (selectionIndicator != null)
                {
                    SerializationHelper.Wire(so, "_selectionIndicator", selectionIndicator);
                }
            }

            if (AddPopupTrigger && Context.TryGet<PopupTrigger>("PopupTrigger", out var popupTrigger))
            {
                SerializationHelper.Wire(so, "_popupTrigger", popupTrigger);
            }

            so.ApplyModifiedProperties();
        }

        protected abstract void WireFields(TElement element);

        protected void SavePrefab()
        {
            PrefabUtility.SaveAsPrefabAsset(Context.Root, PrefabPath);

            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath);
            if (prefab != null)
            {
                UpdateElementMapping(prefab);
                Selection.activeObject = prefab;
                EditorGUIUtility.PingObject(prefab);
            }
        }

        private void UpdateElementMapping(GameObject prefab)
        {
            const string configPath = "Assets/Source/00. Configs/ElementConfig.asset";
            var config = AssetDatabase.LoadAssetAtPath<ElementConfig>(configPath);
            if (config == null)
            {
                Debug.LogWarning($"[ElementBuilder] ElementConfig not found at {configPath}");
                return;
            }

            if (!config.TryGetMapping(typeof(TElement), out var mapping))
            {
                Debug.LogWarning($"[ElementBuilder] No mapping found for {typeof(TElement).Name}. Run Element Mapping Generator first.");
                return;
            }

            var so = new SerializedObject(mapping);
            var prefabProp = so.FindProperty("_elementPrefab");
            if (prefabProp == null)
            {
                Debug.LogError($"[ElementBuilder] _elementPrefab property not found on {mapping.GetType().Name}");
                return;
            }

            prefabProp.objectReferenceValue = prefab;
            so.ApplyModifiedProperties();
            EditorUtility.SetDirty(mapping);
            AssetDatabase.SaveAssets();

            Debug.Log($"<color=cyan>→ Updated {mapping.name} with prefab: {prefab.name}</color>");
        }

        protected void Cleanup()
        {
            if (Context?.Root != null)
            {
                Object.DestroyImmediate(Context.Root);
            }
            Context = null;
        }

        protected GameObject CreateChild(string name)
        {
            return CreateChild(Context.Root.transform, name);
        }

        protected GameObject CreateChild(Transform parent, string name)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            return go;
        }

        protected T CreateComponent<T>(Transform parent, string name) where T : Component
        {
            var go = CreateChild(parent, name);
            var component = go.AddComponent<T>();
            Context.Register(name, component);
            return component;
        }

        protected Button CreateClickableBackground(Transform parent)
        {
            var bg = parent.gameObject.AddComponent<Image>();
            bg.color = new Color(0, 0, 0, 0.01f);
            bg.raycastTarget = true;

            var button = parent.gameObject.AddComponent<Button>();
            button.transition = Selectable.Transition.None;

            Context.Register("ClickButton", button);
            return button;
        }

        protected GameObject CreateSelectionIndicator(Transform parent)
        {
            var indicator = CreateChild(parent, "go_SelectionIndicator");
            var rt = indicator.GetComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = new Vector2(-2, -2);
            rt.offsetMax = new Vector2(2, 2);

            indicator.AddComponent<CanvasRenderer>();
            var img = indicator.AddComponent<Image>();
            img.color = new Color(1f, 0.8f, 0f, 0f);
            img.raycastTarget = false;

            var outline = indicator.AddComponent<Outline>();
            outline.effectColor = new Color(1f, 0.84f, 0f, 1f);
            outline.effectDistance = new Vector2(3, -3);
            outline.useGraphicAlpha = false;

            indicator.SetActive(false);
            Context.RegisterGameObject("SelectionIndicator", indicator);

            return indicator;
        }

        protected PopupTrigger CreatePopupTrigger(Transform parent)
        {
            var trigger = parent.gameObject.AddComponent<PopupTrigger>();
            Context.Register("PopupTrigger", trigger);
            return trigger;
        }

        protected static void EnsureDirectoryExists(string path)
        {
            if (string.IsNullOrEmpty(path)) return;
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
                AssetDatabase.Refresh();
            }
        }

        protected static bool TryLoadPrefab(string path, out GameObject prefab, string displayName = null)
        {
            prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (prefab != null) return true;
            
            Debug.LogError($"Prefab '{displayName ?? Path.GetFileNameWithoutExtension(path)}' not found at: {path}");
            return false;
        }

        protected static GameObject SpawnPrefab(GameObject prefab, Transform parent, string name)
        {
            var instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab, parent);
            instance.name = name;
            return instance;
        }
    }
}
#endif
