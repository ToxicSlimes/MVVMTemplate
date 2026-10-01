using UnityEngine;
using UnityEngine.UIElements;

namespace Core.UI.Toolkit
{
    public static class ToolkitDropdownPopupStyles
    {
        private const string ResourcesPath = "UI/pixel-dropdown-popup";
        private static StyleSheet _sheet;

        public static void EnsureApplied(VisualElement panelRoot)
        {
            if (panelRoot == null)
                return;

            var sheet = ResolveSheet();
            if (sheet == null)
                return;

            if (!panelRoot.styleSheets.Contains(sheet))
                panelRoot.styleSheets.Add(sheet);
        }

        private static StyleSheet ResolveSheet()
        {
            if (_sheet != null)
                return _sheet;

            _sheet = Resources.Load<StyleSheet>(ResourcesPath);
            return _sheet;
        }
    }
}
