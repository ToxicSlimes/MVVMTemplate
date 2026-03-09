using System.Collections.Generic;
using System.Threading;
using Core.Abstractions.Services;
using Core.Abstractions.UI;
using Core.Application.Notifications.UI;
using Core.Configs.DesignSystem;
using Core.Data.Domain.Enums;
using Core.Infrastructure.Mediator.Abstractions;
using Core.Infrastructure.UI.Theming;
using Cysharp.Threading.Tasks;

namespace Core.Infrastructure.UI.Services
{
    public sealed class UIControlService : IUIControlService
    {
        public static UIControlService Instance { get; private set; }

        private readonly IAudioService _audioService;
        private readonly IMediator _mediator;
        private readonly HashSet<IThemeable> _controls = new();
        private IThemeProvider _currentTheme;

        public IThemeProvider CurrentTheme => _currentTheme;

        public UIControlService(IAudioService audioService, IMediator mediator, DesignSystemSettings designSystem)
        {
            _audioService = audioService;
            _mediator = mediator;
            _currentTheme = new ThemeResolver(designSystem);
            Instance = this;
        }

        public void Register(IThemeable control)
        {
            if (control == null || !_controls.Add(control)) return;
            control.ApplyTheme(_currentTheme);
        }

        public void Unregister(IThemeable control)
        {
            _controls.Remove(control);
        }

        public void ApplyThemeToAll()
        {
            foreach (var control in _controls)
                control.ApplyTheme(_currentTheme);
        }

        public void SetTheme(DesignSystemSettings newSettings)
        {
            _currentTheme = new ThemeResolver(newSettings);
            ApplyThemeToAll();
            _mediator.Publish(new ThemeChangedNotification(), CancellationToken.None).Forget();
        }

        public void PlaySound(SoundType sound)
        {
            if (sound != SoundType.Unknown)
                _audioService?.Play(sound);
        }
    }
}
