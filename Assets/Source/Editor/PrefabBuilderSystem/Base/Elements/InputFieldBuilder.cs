#if UNITY_EDITOR
using Core.Editor.PrefabBuilderSystem.Conventions;
using Core.Editor.PrefabBuilderSystem.Theming;
using Core.Infrastructure.UI;
using Core.UI.Controls;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Core.Editor.PrefabBuilderSystem.Elements
{
    public class InputFieldBuilder : BaseElementBuilder<InputFieldBuilder, BaseInputField>
    {
        protected override string NamingPrefix => NamingConvention.Input;
        
        private string _text = "";
        private string _placeholder = "Enter text...";
        private float _fontSize = 14f;
        private Color _textColor = Color.white;
        private Color _placeholderColor = new(0.5f, 0.5f, 0.5f, 1f);
        private Color _backgroundColor = new(0.15f, 0.15f, 0.15f, 1f);
        private TMP_InputField.ContentType _contentType = TMP_InputField.ContentType.Standard;
        private int _characterLimit;
        private TMP_InputField.LineType _lineType = TMP_InputField.LineType.SingleLine;
        private float _paddingHorizontal = 12f;
        private float _paddingVertical = 4f;
        private Sprite _backgroundSprite;
        private Color _selectionColor = new(0.4f, 0.6f, 1f, 0.5f);
        private Color _caretColor = Color.white;

        public InputFieldBuilder(string name) : base(name) { }

        public InputFieldBuilder WithText(string text)
        {
            _text = text;
            return this;
        }

        public InputFieldBuilder WithPlaceholder(string placeholder)
        {
            _placeholder = placeholder;
            return this;
        }

        public InputFieldBuilder WithFontSize(float size)
        {
            _fontSize = size;
            return this;
        }

        public InputFieldBuilder WithTextColor(Color color)
        {
            _textColor = color;
            return this;
        }

        public InputFieldBuilder WithPlaceholderColor(Color color)
        {
            _placeholderColor = color;
            return this;
        }

        public InputFieldBuilder WithBackgroundColor(Color color)
        {
            _backgroundColor = color;
            return this;
        }

        public InputFieldBuilder Password()
        {
            _contentType = TMP_InputField.ContentType.Password;
            return this;
        }

        public InputFieldBuilder EmailAddress()
        {
            _contentType = TMP_InputField.ContentType.EmailAddress;
            return this;
        }

        public InputFieldBuilder IntegerNumber()
        {
            _contentType = TMP_InputField.ContentType.IntegerNumber;
            return this;
        }

        public InputFieldBuilder DecimalNumber()
        {
            _contentType = TMP_InputField.ContentType.DecimalNumber;
            return this;
        }

        public InputFieldBuilder WithCharacterLimit(int limit)
        {
            _characterLimit = limit;
            return this;
        }

        public InputFieldBuilder MultiLine()
        {
            _lineType = TMP_InputField.LineType.MultiLineNewline;
            return this;
        }

        public InputFieldBuilder WithPadding(float horizontal, float vertical)
        {
            _paddingHorizontal = horizontal;
            _paddingVertical = vertical;
            return this;
        }

        public InputFieldBuilder WithPadding(float padding)
        {
            _paddingHorizontal = padding;
            _paddingVertical = padding * 0.3f;
            return this;
        }

        public InputFieldBuilder WithBackgroundSprite(Sprite sprite)
        {
            _backgroundSprite = sprite;
            return this;
        }

        public InputFieldBuilder WithSelectionColor(Color color)
        {
            _selectionColor = color;
            return this;
        }

        public InputFieldBuilder WithCaretColor(Color color)
        {
            _caretColor = color;
            return this;
        }

        public InputFieldBuilder Themed()
        {
            var colors = DesignSystemProvider.Colors;
            var spacing = DesignSystemProvider.Spacing;
            
            if (colors != null)
            {
                _backgroundColor = colors.inputBackground;
                _textColor = colors.textPrimary;
                _placeholderColor = colors.inputPlaceholder;
                _selectionColor = new Color(colors.indigo.r, colors.indigo.g, colors.indigo.b, 0.5f);
                _caretColor = colors.textPrimary;
            }

            if (spacing != null)
            {
                _paddingHorizontal = spacing.inputPaddingHorizontal;
                _paddingVertical = 4f;
            }

            return this;
        }

        public InputFieldBuilder WithInputPadding()
        {
            var spacing = DesignSystemProvider.Spacing;
            _paddingHorizontal = spacing?.inputPaddingHorizontal ?? 16f;
            _paddingVertical = 4f;
            return this;
        }

        protected override string GetDefaultPrefabPath() => ElementPaths.Interactive.InputField;

        protected override void ApplyCustomizations(GameObject instance, BaseInputField input)
        {
            if (input == null)
            {
                var native = instance.GetComponent<TMP_InputField>();
                if (native != null)
                {
                    UnityEditorInternal.ComponentUtility.CopyComponent(native);
                    Object.DestroyImmediate(native);
                    input = instance.AddComponent<BaseInputField>();
                    UnityEditorInternal.ComponentUtility.PasteComponentValues(input);
                }
            }
            if (input == null) return;

            if (input.textViewport == null)
            {
                var viewport = instance.transform.Find("Text Area");
                if (viewport != null) input.textViewport = viewport as RectTransform;
            }
            if (input.textComponent == null && input.textViewport != null)
            {
                var text = input.textViewport.Find("Text");
                if (text != null) input.textComponent = text.GetComponent<TMPro.TMP_Text>();
            }
            if (input.placeholder == null && input.textViewport != null)
            {
                var ph = input.textViewport.Find("Placeholder");
                if (ph != null) input.placeholder = ph.GetComponent<Graphic>();
            }
            if (input.targetGraphic == null)
                input.targetGraphic = instance.GetComponent<Graphic>();

            input.text = _text;
            input.contentType = _contentType;
            input.characterLimit = _characterLimit;
            input.lineType = _lineType;
            input.selectionColor = _selectionColor;
            input.caretColor = _caretColor;

            if (input.textComponent != null)
            {
                input.textComponent.fontSize = _fontSize;
                input.textComponent.color = _textColor;
            }

            var placeholder = input.placeholder as TextMeshProUGUI;
            if (placeholder != null)
            {
                placeholder.text = _placeholder;
                placeholder.color = _placeholderColor;
            }

            var bg = instance.GetComponent<Image>();
            if (bg != null) bg.color = _backgroundColor;

            if (instance.GetComponent<CanvasGroup>() == null)
                instance.AddComponent<CanvasGroup>();
            var bcb = instance.GetComponent<BaseControlBehaviour>() ?? instance.AddComponent<BaseControlBehaviour>();
            ControlDefaults.ApplyTo(bcb);

            ApplyLayoutElement(instance);
        }

        public override BaseInputField Build(Transform parent)
        {
            if (ShouldUsePrefab())
                return BuildFromPrefab(parent);
            
            var rt = CreateRectTransform(parent);
            var go = rt.gameObject;

            if (Size == Vector2.zero)
            {
                rt.sizeDelta = new Vector2(200f, 30f);
            }

            var bgImg = go.AddComponent<Image>();
            bgImg.color = _backgroundColor;
            bgImg.sprite = _backgroundSprite;
            if (_backgroundSprite != null)
            {
                bgImg.type = Image.Type.Sliced;
            }

            var textAreaGo = new GameObject("Text Area");
            textAreaGo.transform.SetParent(go.transform, false);
            var textAreaRt = textAreaGo.AddComponent<RectTransform>();
            textAreaRt.anchorMin = Vector2.zero;
            textAreaRt.anchorMax = Vector2.one;
            textAreaRt.offsetMin = new Vector2(_paddingHorizontal, _paddingVertical);
            textAreaRt.offsetMax = new Vector2(-_paddingHorizontal, -_paddingVertical);

            textAreaGo.AddComponent<RectMask2D>();

            var placeholderGo = new GameObject("Placeholder");
            placeholderGo.transform.SetParent(textAreaGo.transform, false);
            var placeholderRt = placeholderGo.AddComponent<RectTransform>();
            placeholderRt.anchorMin = Vector2.zero;
            placeholderRt.anchorMax = Vector2.one;
            placeholderRt.offsetMin = Vector2.zero;
            placeholderRt.offsetMax = Vector2.zero;

            var placeholderTmp = placeholderGo.AddComponent<TextMeshProUGUI>();
            placeholderTmp.text = _placeholder;
            placeholderTmp.fontSize = _fontSize;
            placeholderTmp.color = _placeholderColor;
            placeholderTmp.alignment = TextAlignmentOptions.MidlineLeft;
            placeholderTmp.fontStyle = FontStyles.Italic;

            var textGo = new GameObject("Text");
            textGo.transform.SetParent(textAreaGo.transform, false);
            var textRt = textGo.AddComponent<RectTransform>();
            textRt.anchorMin = Vector2.zero;
            textRt.anchorMax = Vector2.one;
            textRt.offsetMin = Vector2.zero;
            textRt.offsetMax = Vector2.zero;

            var textTmp = textGo.AddComponent<TextMeshProUGUI>();
            textTmp.fontSize = _fontSize;
            textTmp.color = _textColor;
            textTmp.alignment = TextAlignmentOptions.MidlineLeft;

            var inputField = go.AddComponent<BaseInputField>();
            inputField.textViewport = textAreaRt;
            inputField.textComponent = textTmp;
            inputField.placeholder = placeholderTmp;
            inputField.text = _text;
            inputField.contentType = _contentType;
            inputField.characterLimit = _characterLimit;
            inputField.lineType = _lineType;
            inputField.selectionColor = _selectionColor;
            inputField.caretColor = _caretColor;
            inputField.targetGraphic = bgImg;

            go.AddComponent<CanvasGroup>();
            ControlDefaults.ApplyTo(go.AddComponent<BaseControlBehaviour>());

            ApplyLayoutElement(go);

            return inputField;
        }
    }
}
#endif
