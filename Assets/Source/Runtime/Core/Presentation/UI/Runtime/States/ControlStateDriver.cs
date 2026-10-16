using Core.Abstractions.UI;
using LitMotion;
using UnityEngine;

namespace Core.Presentation.UI.States
{
    public class ControlStateDriver
    {
        private readonly RectTransform _rectTransform;
        private readonly CanvasGroup _canvasGroup;
        private readonly UnityEngine.UI.Graphic _graphic;
        private readonly Color _baseColor;
        private ControlVisualConfig _config;
        private ControlState _currentState = ControlState.Normal;
        private MotionHandle _scaleHandle;
        private MotionHandle _alphaHandle;
        private MotionHandle _colorHandle;

        public ControlState CurrentState => _currentState;

        public ControlStateDriver(RectTransform rectTransform, CanvasGroup canvasGroup, UnityEngine.UI.Graphic graphic)
        {
            _rectTransform = rectTransform;
            _canvasGroup = canvasGroup;
            _graphic = graphic;
            _baseColor = graphic != null ? graphic.color : Color.white;
        }

        public void SetConfig(ControlVisualConfig config)
        {
            _config = config;
        }

        /// <summary>
        /// Цвета конфига — подкраска, а не замена: они умножаются на собственный цвет контрола.
        /// Иначе первый же переход состояния стёр бы цвет, заданный билдером.
        /// </summary>
        private Color ResolveColor(ControlState state) => _baseColor * _config.GetColor(state);

        public void TransitionTo(ControlState state)
        {
            if (_config == null || state == _currentState) return;

            _currentState = state;
            CancelActive();

            var targetScale = _config.GetScale(state);
            var targetAlpha = _config.GetAlpha(state);
            var targetColor = ResolveColor(state);
            var duration = _config.GetTransitionDuration(state);

            if (_rectTransform != null)
            {
                var currentScale = _rectTransform.localScale.x;
                var rt = _rectTransform;
                _scaleHandle = LMotion.Create(currentScale, targetScale, duration)
                    .WithScheduler(MotionScheduler.UpdateIgnoreTimeScale)
                    .WithEase(Ease.OutQuad)
                    .Bind(rt, static (s, target) => target.localScale = new Vector3(s, s, 1f));
            }

            if (_canvasGroup != null)
            {
                var currentAlpha = _canvasGroup.alpha;
                var cg = _canvasGroup;
                _alphaHandle = LMotion.Create(currentAlpha, targetAlpha, duration)
                    .WithScheduler(MotionScheduler.UpdateIgnoreTimeScale)
                    .WithEase(Ease.OutQuad)
                    .Bind(cg, static (a, target) => target.alpha = a);
            }

            if (_graphic != null)
            {
                var currentColor = _graphic.color;
                var g = _graphic;
                _colorHandle = LMotion.Create(currentColor, targetColor, duration)
                    .WithScheduler(MotionScheduler.UpdateIgnoreTimeScale)
                    .WithEase(Ease.OutQuad)
                    .Bind(g, static (c, target) => target.color = c);
            }
        }

        public void SetStateImmediate(ControlState state)
        {
            if (_config == null) return;
            _currentState = state;
            CancelActive();

            if (_rectTransform != null)
            {
                var s = _config.GetScale(state);
                _rectTransform.localScale = new Vector3(s, s, 1f);
            }

            if (_canvasGroup != null)
                _canvasGroup.alpha = _config.GetAlpha(state);

            if (_graphic != null)
                _graphic.color = ResolveColor(state);
        }

        public void CancelActive()
        {
            if (_scaleHandle.IsActive()) _scaleHandle.Cancel();
            if (_alphaHandle.IsActive()) _alphaHandle.Cancel();
            if (_colorHandle.IsActive()) _colorHandle.Cancel();
        }
    }
}
