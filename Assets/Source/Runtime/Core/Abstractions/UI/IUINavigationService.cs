using UnityEngine.UI;

namespace Core.Abstractions.UI
{
    public interface IUINavigationService
    {
        void SetActiveFocusGroup(IFocusGroup group);
        void ClearFocusGroup();
        void Select(Selectable selectable);
    }
}
