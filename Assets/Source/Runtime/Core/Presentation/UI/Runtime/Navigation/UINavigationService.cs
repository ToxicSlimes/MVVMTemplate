using System;
using Core.Abstractions.Services.Input;
using Core.Abstractions.UI;
using R3;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Core.Presentation.UI.Navigation
{
    public sealed class UINavigationService : IUINavigationService, IDisposable
    {
        private readonly NavigationConfig _config;
        private DisposableBag _disposables;
        private IFocusGroup _activeFocusGroup;

        public bool AutoSelectOnViewShow => _config != null && _config.AutoSelectOnViewShow;

        public UINavigationService(IInputService inputService, NavigationConfig config = null)
        {
            _config = config;

            inputService.OnNext
                .Subscribe(this, static (_, state) => state._activeFocusGroup?.SelectNext())
                .AddTo(ref _disposables);

            inputService.OnPrevious
                .Subscribe(this, static (_, state) => state._activeFocusGroup?.SelectPrevious())
                .AddTo(ref _disposables);
        }

        public void SetActiveFocusGroup(IFocusGroup group)
        {
            _activeFocusGroup = group;
            group?.SelectInitial();
        }

        public void ClearFocusGroup()
        {
            _activeFocusGroup = null;
            EventSystem.current?.SetSelectedGameObject(null);
        }

        public void Select(Selectable selectable)
        {
            if (selectable == null) return;
            EventSystem.current?.SetSelectedGameObject(selectable.gameObject);
        }

        public void Dispose()
        {
            _disposables.Dispose();
        }
    }
}
