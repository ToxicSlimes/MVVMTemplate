using Core.Data.Domain.Models.Elements.Samples;
using UnityEngine.UIElements;

namespace Core.UI.Toolkit.Elements.Samples
{
    public class StatBarToolkitElement : ToolkitBindableElement<StatBarData>
    {
        private readonly Label _label;
        private readonly Label _valueLabel;
        private readonly VisualElement _fill;

        public StatBarToolkitElement() : this(new VisualElement())
        {
        }

        public StatBarToolkitElement(VisualElement root) : base(root)
        {
            _label = root.Q<Label>("stat-label");
            _valueLabel = root.Q<Label>("stat-value");
            _fill = root.Q<VisualElement>("stat-fill");
        }

        protected override void OnBind(StatBarData data)
        {
            if (_label != null)
                _label.text = data.Label;

            UpdateValue(data);
        }

        protected override void OnRefresh(StatBarData oldData, StatBarData newData)
        {
            UpdateValue(newData);
        }

        protected override void OnUnbind()
        {
            if (_label != null)
                _label.text = string.Empty;

            if (_valueLabel != null)
                _valueLabel.text = string.Empty;

            if (_fill != null)
                _fill.style.width = Length.Percent(0f);
        }

        protected override (string title, string message) GetTooltip(StatBarData data)
        {
            var valueText = !string.IsNullOrEmpty(data.FormattedValue)
                ? data.FormattedValue
                : $"{data.CurrentValue:F0}/{data.MaxValue:F0}";
            return (data.Label, valueText);
        }

        private void UpdateValue(StatBarData data)
        {
            if (_valueLabel != null)
            {
                _valueLabel.text = !string.IsNullOrEmpty(data.FormattedValue)
                    ? data.FormattedValue
                    : $"{data.CurrentValue:F0}/{data.MaxValue:F0}";
            }

            if (_fill != null && data.MaxValue > 0f)
                _fill.style.width = Length.Percent(data.CurrentValue / data.MaxValue * 100f);
        }
    }
}
