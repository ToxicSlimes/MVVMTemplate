using Core.Abstractions.UI;
using Core.Presentation.UI.Services;
using UnityEngine;

namespace Core.Presentation.UI.Theming
{
    public sealed class ThemeApplicator : MonoBehaviour
    {
        [SerializeField] private bool _applyOnEnable = true;

        private void OnEnable()
        {
            if (!_applyOnEnable) return;
            var service = UIControlService.Instance;
            if (service == null) return;

            var themeables = GetComponentsInChildren<IThemeable>(true);
            for (var i = 0; i < themeables.Length; i++)
            {
                if (themeables[i] is BaseControlBehaviour) continue;
                themeables[i].ApplyTheme(service.CurrentTheme);
            }
        }

        public void ApplyToChildren()
        {
            var service = UIControlService.Instance;
            if (service == null) return;

            var themeables = GetComponentsInChildren<IThemeable>(true);
            for (var i = 0; i < themeables.Length; i++)
                themeables[i].ApplyTheme(service.CurrentTheme);
        }
    }
}
