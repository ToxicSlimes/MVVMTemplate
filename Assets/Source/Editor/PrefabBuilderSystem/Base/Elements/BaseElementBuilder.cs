#if UNITY_EDITOR
using Core.Editor.PrefabBuilderSystem.Conventions;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using System.IO;

namespace Core.Editor.PrefabBuilderSystem.Elements
{
    public abstract class BaseElementBuilder<TSelf, TComponent> : IElementBuilder<TComponent>
        where TSelf : BaseElementBuilder<TSelf, TComponent>
        where TComponent : Component
    {
        protected string Name { get; private set; }
        protected Vector2 Size { get; set; } = Vector2.zero;
        protected Vector2 AnchorMin { get; set; } = new(0.5f, 0.5f);
        protected Vector2 AnchorMax { get; set; } = new(0.5f, 0.5f);
        protected Vector2 Pivot { get; set; } = new(0.5f, 0.5f);
        protected Vector2 AnchoredPosition { get; set; } = Vector2.zero;
        protected bool Stretch { get; set; }
        
        protected string PrefabSourcePath { get; set; }
        protected bool UsePrefabSource { get; set; }

        protected float? PreferredWidth { get; set; }
        protected float? PreferredHeight { get; set; }
        protected float? MinWidth { get; set; }
        protected float? MinHeight { get; set; }
        protected float? FlexibleWidth { get; set; }
        protected float? FlexibleHeight { get; set; }

        protected bool AutoNaming { get; set; } = true;
        protected abstract string NamingPrefix { get; }

        protected BaseElementBuilder(string name)
        {
            Name = name;
        }

        public TSelf NoAutoNaming()
        {
            AutoNaming = false;
            return (TSelf)this;
        }

        public TSelf WithSize(float width, float height)
        {
            Size = new Vector2(width, height);
            return (TSelf)this;
        }

        public TSelf WithSize(Vector2 size)
        {
            Size = size;
            return (TSelf)this;
        }

        public TSelf WithAnchors(Vector2 min, Vector2 max)
        {
            AnchorMin = min;
            AnchorMax = max;
            return (TSelf)this;
        }

        public TSelf WithAnchor(Vector2 min, Vector2 max)
        {
            AnchorMin = min;
            AnchorMax = max;
            return (TSelf)this;
        }

        public TSelf WithPivot(Vector2 pivot)
        {
            Pivot = pivot;
            return (TSelf)this;
        }

        public TSelf WithPosition(Vector2 position)
        {
            AnchoredPosition = position;
            return (TSelf)this;
        }

        public TSelf WithAnchoredPosition(Vector2 position)
        {
            AnchoredPosition = position;
            return (TSelf)this;
        }

        public TSelf WithSizeDelta(Vector2 sizeDelta)
        {
            Size = sizeDelta;
            return (TSelf)this;
        }

        public TSelf WithStretch()
        {
            Stretch = true;
            return (TSelf)this;
        }

        public TSelf WithPreferredSize(float? width = null, float? height = null)
        {
            PreferredWidth = width;
            PreferredHeight = height;
            return (TSelf)this;
        }

        public TSelf WithMinSize(float? width = null, float? height = null)
        {
            MinWidth = width;
            MinHeight = height;
            return (TSelf)this;
        }

        public TSelf WithFlexible(float? width = null, float? height = null)
        {
            FlexibleWidth = width;
            FlexibleHeight = height;
            return (TSelf)this;
        }

        public abstract TComponent Build(Transform parent);

        protected RectTransform CreateRectTransform(Transform parent)
        {
            var finalName = AutoNaming && NamingConvention.AutoApply 
                ? NamingConvention.ApplyPrefix(Name, NamingPrefix) 
                : Name;
            
            var go = new GameObject(finalName);
            go.transform.SetParent(parent, false);

            var rt = go.AddComponent<RectTransform>();

            if (Stretch)
            {
                rt.anchorMin = Vector2.zero;
                rt.anchorMax = Vector2.one;
                rt.offsetMin = Vector2.zero;
                rt.offsetMax = Vector2.zero;
            }
            else
            {
                rt.anchorMin = AnchorMin;
                rt.anchorMax = AnchorMax;
                rt.pivot = Pivot;
                rt.anchoredPosition = AnchoredPosition;

                if (Size != Vector2.zero)
                {
                    rt.sizeDelta = Size;
                }
            }

            return rt;
        }

        protected LayoutElement ApplyLayoutElement(GameObject go)
        {
            if (!PreferredWidth.HasValue && !PreferredHeight.HasValue &&
                !MinWidth.HasValue && !MinHeight.HasValue &&
                !FlexibleWidth.HasValue && !FlexibleHeight.HasValue)
            {
                return null;
            }

            var le = go.GetComponent<LayoutElement>() ?? go.AddComponent<LayoutElement>();

            if (PreferredWidth.HasValue) le.preferredWidth = PreferredWidth.Value;
            if (PreferredHeight.HasValue) le.preferredHeight = PreferredHeight.Value;
            if (MinWidth.HasValue) le.minWidth = MinWidth.Value;
            if (MinHeight.HasValue) le.minHeight = MinHeight.Value;
            if (FlexibleWidth.HasValue) le.flexibleWidth = FlexibleWidth.Value;
            if (FlexibleHeight.HasValue) le.flexibleHeight = FlexibleHeight.Value;

            return le;
        }

        public TSelf FromPrefab(string path = null)
        {
            UsePrefabSource = true;
            PrefabSourcePath = path ?? GetDefaultPrefabPath();
            return (TSelf)this;
        }

        protected virtual string GetDefaultPrefabPath() => null;

        public TComponent BuildFromPrefab(Transform parent)
        {
            if (string.IsNullOrEmpty(PrefabSourcePath))
            {
                Debug.LogError($"[{GetType().Name}] Prefab path not set. Use FromPrefab(path) or override GetDefaultPrefabPath()");
                return null;
            }

            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabSourcePath);
            if (prefab == null)
            {
                Debug.LogError($"[{GetType().Name}] Prefab not found at: {PrefabSourcePath}");
                return null;
            }

            var instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab, parent);
            instance.name = Name;

            var component = instance.GetComponent<TComponent>();
            if (component == null)
            {
                component = instance.GetComponentInChildren<TComponent>();
            }

            ApplyCustomizations(instance, component);
            return component;
        }

        protected virtual void ApplyCustomizations(GameObject instance, TComponent component) { }

        public GameObject SaveAsPrefab(string path = null)
        {
            var savePath = path ?? GetDefaultPrefabPath();
            if (string.IsNullOrEmpty(savePath))
            {
                Debug.LogError($"[{GetType().Name}] Save path not specified");
                return null;
            }

            var directory = Path.GetDirectoryName(savePath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
                AssetDatabase.Refresh();
            }

            var tempParent = new GameObject("_TempPrefabParent");
            try
            {
                var component = Build(tempParent.transform);
                var go = component.gameObject;
                go.transform.SetParent(null);

                var prefab = PrefabUtility.SaveAsPrefabAsset(go, savePath);
                Debug.Log($"[PrefabBuilder] Saved prefab: {savePath}");

                Object.DestroyImmediate(go);
                return prefab;
            }
            finally
            {
                Object.DestroyImmediate(tempParent);
            }
        }
    }
}
#endif
