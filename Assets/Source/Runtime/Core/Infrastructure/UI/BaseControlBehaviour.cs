using Core.Abstractions.UI;
using Core.Data.Domain.Enums;
using Core.Infrastructure.UI.Effects;
using Core.Infrastructure.UI.Services;
using Core.Infrastructure.UI.States;
using UnityEngine;
using UnityEngine.UI;

namespace Core.Infrastructure.UI
{
    [RequireComponent(typeof(CanvasGroup))]
    public sealed class BaseControlBehaviour : MonoBehaviour, IThemeable, IControlBehavior
    {
        [SerializeField] private ThemeTag _themeTag = ThemeTag.Primary;
        [SerializeField] private ControlVisualConfig _visualConfig;
        [SerializeField] private UIEffect _hoverEffect;
        [SerializeField] private UIEffect _pressEffect;
        [SerializeField] private SoundType _clickSound = SoundType.UI_ButtonClick;
        [SerializeField] private SoundType _hoverSound = SoundType.UI_ButtonHover;

        private ControlStateDriver _stateDriver;
        private UIEffectPlayer _effectPlayer;
        private IUIControlService _controlService;

        public ThemeTag ThemeTag => _themeTag;
        public SoundType ClickSound => _clickSound;
        public SoundType HoverSound => _hoverSound;

        public void Initialize(IUIControlService service)
        {
            _controlService = service;
        }

        private void Awake()
        {
            _effectPlayer = GetComponent<UIEffectPlayer>();
            if (_visualConfig == null) return;
            var rt = transform as RectTransform;
            var cg = GetComponent<CanvasGroup>();
            var graphic = GetComponent<Graphic>();
            _stateDriver = new ControlStateDriver(rt, cg, graphic);
            _stateDriver.SetConfig(_visualConfig);
        }

        private void OnEnable()
        {
            if (_controlService == null && UIControlService.Instance != null)
                _controlService = UIControlService.Instance;
            _controlService?.Register(this);
        }

        private void OnDisable()
        {
            _controlService?.Unregister(this);
        }

        private void OnDestroy() => _stateDriver?.CancelActive();

        public void TransitionTo(ControlState state) => _stateDriver?.TransitionTo(state);
        public void PlayEffect(UIEffect effect) => _effectPlayer?.Play(effect);
        public void PlayHoverEffect() => PlayEffect(_hoverEffect);
        public void PlayPressEffect() => PlayEffect(_pressEffect);

        public void PlaySound(SoundType sound)
        {
            if (sound != SoundType.Unknown)
                _controlService?.PlaySound(sound);
        }

        public void ApplyTheme(IThemeProvider provider)
        {
            if (provider == null) return;
            var selectable = GetComponent<Selectable>();
            if (selectable != null)
            {
                var cb = selectable.colors;
                cb.normalColor = provider.GetColor(_themeTag);
                cb.highlightedColor = provider.GetHoverColor(_themeTag);
                cb.pressedColor = provider.GetPressedColor(_themeTag);
                cb.disabledColor = provider.GetDisabledColor(_themeTag);
                selectable.colors = cb;
                return;
            }
            var text = GetComponent<TMPro.TMP_Text>();
            if (text != null)
            {
                text.color = provider.GetTextColor(_themeTag);
            }
        }
    }
}
