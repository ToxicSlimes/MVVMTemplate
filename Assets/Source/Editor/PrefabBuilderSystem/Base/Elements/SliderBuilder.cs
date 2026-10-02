#if UNITY_EDITOR
using Core.Editor.PrefabBuilderSystem.Conventions;
using Core.Editor.PrefabBuilderSystem.Theming;
using Core.Presentation.UI;
using Core.UI.Controls;
using UnityEngine;
using UnityEngine.UI;

namespace Core.Editor.PrefabBuilderSystem.Elements
{
    public class SliderBuilder : BaseElementBuilder<SliderBuilder, BaseSlider>
    {
        protected override string NamingPrefix => NamingConvention.Slider;
        
        private float _minValue;
        private float _maxValue = 1f;
        private float _value;
        private bool _wholeNumbers;
        private bool _vertical;
        private Color _backgroundColor = new(0.2f, 0.2f, 0.2f, 1f);
        private Color _fillColor = new(0.4f, 0.6f, 1f, 1f);
        private Color _handleColor = Color.white;
        private float _height = 20f;
        private float _handleSize = 20f;
        private float _trackOffset = 5f;
        private float _handleAreaOffset = 5f;
        private Sprite _backgroundSprite;
        private Sprite _fillSprite;
        private Sprite _handleSprite;

        public SliderBuilder(string name) : base(name) { }

        public SliderBuilder WithRange(float min, float max)
        {
            _minValue = min;
            _maxValue = max;
            return this;
        }

        public SliderBuilder WithValue(float value)
        {
            _value = value;
            return this;
        }

        public SliderBuilder WholeNumbers()
        {
            _wholeNumbers = true;
            return this;
        }

        public SliderBuilder Vertical()
        {
            _vertical = true;
            return this;
        }

        public SliderBuilder WithBackgroundColor(Color color)
        {
            _backgroundColor = color;
            return this;
        }

        public SliderBuilder WithFillColor(Color color)
        {
            _fillColor = color;
            return this;
        }

        public SliderBuilder WithHandleColor(Color color)
        {
            _handleColor = color;
            return this;
        }

        public SliderBuilder WithHeight(float height)
        {
            _height = height;
            return this;
        }

        public SliderBuilder WithHandleSize(float size)
        {
            _handleSize = size;
            return this;
        }

        public SliderBuilder WithTrackOffset(float offset)
        {
            _trackOffset = offset;
            return this;
        }

        public SliderBuilder WithHandleAreaOffset(float offset)
        {
            _handleAreaOffset = offset;
            return this;
        }

        public SliderBuilder WithBackgroundSprite(Sprite sprite)
        {
            _backgroundSprite = sprite;
            return this;
        }

        public SliderBuilder WithFillSprite(Sprite sprite)
        {
            _fillSprite = sprite;
            return this;
        }

        public SliderBuilder WithHandleSprite(Sprite sprite)
        {
            _handleSprite = sprite;
            return this;
        }

        public SliderBuilder Themed()
        {
            var colors = DesignSystemProvider.Colors;
            _backgroundColor = colors?.darkSurface ?? new Color(0.2f, 0.2f, 0.2f, 1f);
            _fillColor = colors?.indigo ?? new Color(0.184f, 0.816f, 0.451f, 1f);
            _handleColor = colors?.textPrimary ?? Color.white;
            return this;
        }

        protected override string GetDefaultPrefabPath() => ElementPaths.Interactive.Slider;

        protected override void ApplyCustomizations(GameObject instance, BaseSlider slider)
        {
            if (slider == null)
            {
                var native = instance.GetComponent<Slider>();
                if (native != null)
                {
                    UnityEditorInternal.ComponentUtility.CopyComponent(native);
                    Object.DestroyImmediate(native);
                    slider = instance.AddComponent<BaseSlider>();
                    UnityEditorInternal.ComponentUtility.PasteComponentValues(slider);
                }
            }
            if (slider == null) return;

            slider.minValue = _minValue;
            slider.maxValue = _maxValue;
            slider.value = _value;
            slider.wholeNumbers = _wholeNumbers;
            if (_vertical) slider.direction = Slider.Direction.BottomToTop;

            if (slider.fillRect == null)
            {
                var fillArea = instance.transform.Find("Fill Area");
                if (fillArea != null)
                {
                    var fill = fillArea.Find("Fill");
                    if (fill != null) slider.fillRect = fill as RectTransform;
                }
            }

            if (slider.handleRect == null)
            {
                var handleArea = instance.transform.Find("Handle Slide Area");
                if (handleArea != null)
                {
                    var handle = handleArea.Find("Handle");
                    if (handle != null) slider.handleRect = handle as RectTransform;
                }
            }

            if (slider.targetGraphic == null && slider.handleRect != null)
                slider.targetGraphic = slider.handleRect.GetComponent<Graphic>();

            var bg = instance.transform.Find("Background");
            if (bg != null)
            {
                var bgImg = bg.GetComponent<Image>();
                if (bgImg != null) bgImg.color = _backgroundColor;
            }

            if (slider.fillRect != null)
            {
                var fillImg = slider.fillRect.GetComponent<Image>();
                if (fillImg != null) fillImg.color = _fillColor;
            }

            if (slider.handleRect != null)
            {
                var handleImg = slider.handleRect.GetComponent<Image>();
                if (handleImg != null) handleImg.color = _handleColor;
            }

            if (instance.GetComponent<CanvasGroup>() == null)
                instance.AddComponent<CanvasGroup>();
            var bcb = instance.GetComponent<BaseControlBehaviour>() ?? instance.AddComponent<BaseControlBehaviour>();
            ControlDefaults.ApplyTo(bcb);

            ApplyLayoutElement(instance);
        }

        public override BaseSlider Build(Transform parent)
        {
            if (ShouldUsePrefab())
                return BuildFromPrefab(parent);
            
            var rt = CreateRectTransform(parent);
            var go = rt.gameObject;

            if (Size == Vector2.zero)
            {
                rt.sizeDelta = _vertical ? new Vector2(_height, 160f) : new Vector2(160f, _height);
            }

            var bgGo = new GameObject("Background");
            bgGo.transform.SetParent(go.transform, false);
            var bgRt = bgGo.AddComponent<RectTransform>();
            bgRt.anchorMin = Vector2.zero;
            bgRt.anchorMax = Vector2.one;
            bgRt.offsetMin = Vector2.zero;
            bgRt.offsetMax = Vector2.zero;

            var bgImg = bgGo.AddComponent<Image>();
            bgImg.color = _backgroundColor;
            bgImg.sprite = _backgroundSprite;
            if (_backgroundSprite != null)
            {
                bgImg.type = Image.Type.Sliced;
            }

            var fillAreaGo = new GameObject("Fill Area");
            fillAreaGo.transform.SetParent(go.transform, false);
            var fillAreaRt = fillAreaGo.AddComponent<RectTransform>();
            fillAreaRt.anchorMin = Vector2.zero;
            fillAreaRt.anchorMax = Vector2.one;
            if (_vertical)
            {
                fillAreaRt.offsetMin = new Vector2(0, _trackOffset);
                fillAreaRt.offsetMax = new Vector2(0, -_trackOffset);
            }
            else
            {
                fillAreaRt.offsetMin = new Vector2(_trackOffset, 0);
                fillAreaRt.offsetMax = new Vector2(-_trackOffset, 0);
            }

            var fillGo = new GameObject("Fill");
            fillGo.transform.SetParent(fillAreaGo.transform, false);
            var fillRt = fillGo.AddComponent<RectTransform>();
            fillRt.anchorMin = Vector2.zero;
            fillRt.anchorMax = _vertical ? new Vector2(1, 0) : new Vector2(0, 1);
            fillRt.pivot = Vector2.zero;
            fillRt.offsetMin = Vector2.zero;
            fillRt.offsetMax = Vector2.zero;

            var fillImg = fillGo.AddComponent<Image>();
            fillImg.color = _fillColor;
            fillImg.sprite = _fillSprite;
            if (_fillSprite != null)
            {
                fillImg.type = Image.Type.Sliced;
            }

            var handleAreaGo = new GameObject("Handle Slide Area");
            handleAreaGo.transform.SetParent(go.transform, false);
            var handleAreaRt = handleAreaGo.AddComponent<RectTransform>();
            handleAreaRt.anchorMin = Vector2.zero;
            handleAreaRt.anchorMax = Vector2.one;
            if (_vertical)
            {
                handleAreaRt.offsetMin = new Vector2(0, _handleAreaOffset);
                handleAreaRt.offsetMax = new Vector2(0, -_handleAreaOffset);
            }
            else
            {
                handleAreaRt.offsetMin = new Vector2(_handleAreaOffset, 0);
                handleAreaRt.offsetMax = new Vector2(-_handleAreaOffset, 0);
            }

            var handleGo = new GameObject("Handle");
            handleGo.transform.SetParent(handleAreaGo.transform, false);
            var handleRt = handleGo.AddComponent<RectTransform>();
            // Handle stretch with -5 offset on all sides
            handleRt.anchorMin = Vector2.zero;
            handleRt.anchorMax = Vector2.one;
            handleRt.offsetMin = new Vector2(-5, -5);
            handleRt.offsetMax = new Vector2(5, 5);

            var handleImg = handleGo.AddComponent<Image>();
            handleImg.color = _handleColor;
            handleImg.sprite = _handleSprite;
            if (_handleSprite != null)
            {
                handleImg.type = Image.Type.Sliced;
            }

            var slider = go.AddComponent<BaseSlider>();
            slider.direction = _vertical ? Slider.Direction.BottomToTop : Slider.Direction.LeftToRight;
            slider.minValue = _minValue;
            slider.maxValue = _maxValue;
            slider.value = _value;
            slider.wholeNumbers = _wholeNumbers;
            slider.fillRect = fillRt;
            slider.handleRect = handleRt;
            slider.targetGraphic = handleImg;

            go.AddComponent<CanvasGroup>();
            ControlDefaults.ApplyTo(go.AddComponent<BaseControlBehaviour>());

            ApplyLayoutElement(go);

            return slider;
        }
    }
}
#endif
