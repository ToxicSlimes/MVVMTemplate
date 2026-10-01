using System;
using Core.Abstractions.MVVM.Elements;
using Core.Data.Domain.Models.Elements;
using UnityEngine.UIElements;

namespace Core.UI.Toolkit.Elements
{
    public abstract class ToolkitBindableElement<TData> : IToolkitBindableElement<TData>
        where TData : struct, IElementData
    {
        private TData _currentData;
        private bool _isBound;

        protected ToolkitBindableElement(VisualElement root)
        {
            Root = root ?? throw new ArgumentNullException(nameof(root));
        }

        public VisualElement Root { get; }
        public string ElementId => _isBound ? _currentData.Id : null;
        public bool IsBound => _isBound;
        protected TData CurrentData => _currentData;

        public void Bind(TData data)
        {
            if (_isBound)
            {
                Refresh(data);
                return;
            }

            _currentData = data;
            _isBound = true;
            OnBind(data);
            ApplyTooltip(data);
        }

        public void Refresh(TData data)
        {
            if (!_isBound)
            {
                Bind(data);
                return;
            }

            var oldData = _currentData;
            _currentData = data;
            OnRefresh(oldData, data);
            ApplyTooltip(data);
        }

        public void Unbind()
        {
            if (!_isBound) return;

            _isBound = false;
            _currentData = default;
            OnUnbind();
        }

        public void Dispose()
        {
            Unbind();
            OnDispose();
        }

        protected abstract void OnBind(TData data);
        protected virtual void OnRefresh(TData oldData, TData newData) => OnBind(newData);
        protected virtual void OnUnbind() { }
        protected virtual void OnDispose() { }
        protected virtual (string title, string message) GetTooltip(TData data) => default;

        private void ApplyTooltip(TData data)
        {
            var tooltip = GetTooltip(data);
            if (string.IsNullOrEmpty(tooltip.title))
                return;

            Root.tooltip = string.IsNullOrEmpty(tooltip.message)
                ? tooltip.title
                : $"{tooltip.title}\n{tooltip.message}";
        }
    }
}
