using Core.Data.Domain.Models.Elements.Samples;
using UnityEngine.UIElements;

namespace Core.UI.Toolkit.Elements.Samples
{
    public class TextCardToolkitElement : ToolkitBindableElement<TextCardData>
    {
        private readonly Label _titleLabel;
        private readonly Label _descriptionLabel;

        public TextCardToolkitElement() : this(new VisualElement())
        {
        }

        public TextCardToolkitElement(VisualElement root) : base(root)
        {
            _titleLabel = root.Q<Label>("title-label");
            _descriptionLabel = root.Q<Label>("description-label");
        }

        protected override void OnBind(TextCardData data)
        {
            if (_titleLabel != null)
                _titleLabel.text = data.Title;

            if (_descriptionLabel != null)
                _descriptionLabel.text = data.Description;
        }

        protected override void OnUnbind()
        {
            if (_titleLabel != null)
                _titleLabel.text = string.Empty;

            if (_descriptionLabel != null)
                _descriptionLabel.text = string.Empty;
        }

        protected override (string title, string message) GetTooltip(TextCardData data)
        {
            return (data.Title, data.Description);
        }
    }
}
