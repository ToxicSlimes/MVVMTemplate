#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using Core.Editor.PrefabBuilderSystem.Theming;
using System.IO;

namespace Core.Editor.PrefabBuilderSystem.Elements
{
    public class ScrollViewResult
    {
        public ScrollRect ScrollRect { get; set; }
        public RectTransform Viewport { get; set; }
        public RectTransform Content { get; set; }
        public RectTransform Root { get; set; }
    }

    public class ScrollViewBuilder
    {
        private readonly string _name;
        private bool _horizontal;
        private bool _vertical = true;
        private float _scrollSensitivity = 20f;
        private ScrollRect.MovementType _movementType = ScrollRect.MovementType.Elastic;
        private bool _inertia = true;
        private float _decelerationRate = 0.135f;
        private Color? _backgroundColor;
        private bool _addMask = true;

        private Vector2 _anchorMin = Vector2.zero;
        private Vector2 _anchorMax = Vector2.one;
        private Vector2 _pivot = new(0.5f, 0.5f);
        private Vector2 _sizeDelta = Vector2.zero;
        private Vector2 _anchoredPosition = Vector2.zero;
        private bool _stretch = true;

        private float? _preferredHeight;

        public ScrollViewBuilder(string name)
        {
            _name = name;
        }

        public ScrollViewBuilder Horizontal()
        {
            _horizontal = true;
            _vertical = false;
            return this;
        }

        public ScrollViewBuilder Vertical()
        {
            _horizontal = false;
            _vertical = true;
            return this;
        }

        public ScrollViewBuilder Both()
        {
            _horizontal = true;
            _vertical = true;
            return this;
        }

        public ScrollViewBuilder WithScrollSensitivity(float sensitivity)
        {
            _scrollSensitivity = sensitivity;
            return this;
        }

        public ScrollViewBuilder Clamped()
        {
            _movementType = ScrollRect.MovementType.Clamped;
            return this;
        }

        public ScrollViewBuilder Unrestricted()
        {
            _movementType = ScrollRect.MovementType.Unrestricted;
            return this;
        }

        public ScrollViewBuilder NoInertia()
        {
            _inertia = false;
            return this;
        }

        public ScrollViewBuilder WithBackground(Color color)
        {
            _backgroundColor = color;
            return this;
        }

        public ScrollViewBuilder NoMask()
        {
            _addMask = false;
            return this;
        }

        public ScrollViewBuilder WithAnchors(Vector2 min, Vector2 max)
        {
            _anchorMin = min;
            _anchorMax = max;
            _stretch = false;
            return this;
        }

        public ScrollViewBuilder WithSize(Vector2 size)
        {
            _sizeDelta = size;
            _stretch = false;
            return this;
        }

        public ScrollViewBuilder WithPosition(Vector2 position)
        {
            _anchoredPosition = position;
            return this;
        }

        public ScrollViewBuilder WithPivot(Vector2 pivot)
        {
            _pivot = pivot;
            return this;
        }

        public ScrollViewBuilder WithStretch()
        {
            _stretch = true;
            return this;
        }

        public ScrollViewBuilder WithPreferredHeight(float height)
        {
            _preferredHeight = height;
            return this;
        }

        public ScrollViewBuilder WithPreferredSize(float? width, float? height)
        {
            if (height.HasValue) _preferredHeight = height.Value;
            if (width.HasValue || height.HasValue)
            {
                _stretch = false;
                _sizeDelta = new Vector2(width ?? 300f, height ?? 400f);
            }
            return this;
        }

        public ScrollViewBuilder Themed()
        {
            var colors = DesignSystemProvider.Colors;
            _backgroundColor = colors?.darkSurface ?? new Color(0.1f, 0.1f, 0.1f, 0.5f);
            return this;
        }

        public GameObject SaveAsPrefab(string path = null)
        {
            var savePath = path ?? ElementPaths.Containers.ScrollView;

            var directory = Path.GetDirectoryName(savePath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
                AssetDatabase.Refresh();
            }

            var tempParent = new GameObject("_TempPrefabParent");
            try
            {
                var result = Build(tempParent.transform);
                var go = result.Root.gameObject;
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

        public ScrollViewResult Build(Transform parent)
        {
            var rootGo = new GameObject(_name);
            rootGo.transform.SetParent(parent, false);

            var rootRt = rootGo.AddComponent<RectTransform>();
            if (_stretch)
            {
                rootRt.anchorMin = Vector2.zero;
                rootRt.anchorMax = Vector2.one;
                rootRt.offsetMin = Vector2.zero;
                rootRt.offsetMax = Vector2.zero;
            }
            else
            {
                rootRt.anchorMin = _anchorMin;
                rootRt.anchorMax = _anchorMax;
                rootRt.pivot = _pivot;
                rootRt.sizeDelta = _sizeDelta;
                rootRt.anchoredPosition = _anchoredPosition;
            }

            if (_backgroundColor.HasValue)
            {
                var bg = rootGo.AddComponent<Image>();
                bg.color = _backgroundColor.Value;
            }

            if (_preferredHeight.HasValue)
            {
                var le = rootGo.AddComponent<LayoutElement>();
                le.preferredHeight = _preferredHeight.Value;
            }

            var scrollRect = rootGo.AddComponent<ScrollRect>();
            scrollRect.horizontal = _horizontal;
            scrollRect.vertical = _vertical;
            scrollRect.scrollSensitivity = _scrollSensitivity;
            scrollRect.movementType = _movementType;
            scrollRect.inertia = _inertia;
            scrollRect.decelerationRate = _decelerationRate;

            var viewportGo = new GameObject("Viewport");
            viewportGo.transform.SetParent(rootGo.transform, false);

            var viewportRt = viewportGo.AddComponent<RectTransform>();
            viewportRt.anchorMin = Vector2.zero;
            viewportRt.anchorMax = Vector2.one;
            viewportRt.offsetMin = Vector2.zero;
            viewportRt.offsetMax = Vector2.zero;

            if (_addMask)
            {
                var mask = viewportGo.AddComponent<Mask>();
                mask.showMaskGraphic = false;
                viewportGo.AddComponent<Image>();
            }
            else
            {
                viewportGo.AddComponent<RectMask2D>();
            }

            scrollRect.viewport = viewportRt;

            var contentGo = new GameObject("Content");
            contentGo.transform.SetParent(viewportGo.transform, false);

            var contentRt = contentGo.AddComponent<RectTransform>();
            contentRt.anchorMin = new Vector2(0, 1);
            contentRt.anchorMax = new Vector2(1, 1);
            contentRt.pivot = new Vector2(0.5f, 1);
            contentRt.sizeDelta = Vector2.zero;

            scrollRect.content = contentRt;

            return new ScrollViewResult
            {
                ScrollRect = scrollRect,
                Viewport = viewportRt,
                Content = contentRt,
                Root = rootRt
            };
        }
    }
}
#endif
