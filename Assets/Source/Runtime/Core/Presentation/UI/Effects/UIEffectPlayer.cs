using LitMotion;
using LitMotion.Extensions;
using UnityEngine;

namespace Core.Presentation.UI.Effects
{
    public class UIEffectPlayer : MonoBehaviour
    {
        private RectTransform _rectTransform;
        private CanvasGroup _canvasGroup;
        private MotionHandle _activeHandle;

        private void Awake()
        {
            _rectTransform = GetComponent<RectTransform>();
            _canvasGroup = GetComponent<CanvasGroup>();
        }

        public void Play(UIEffect effect)
        {
            if (effect == null) return;
            CancelActive();

            switch (effect.Type)
            {
                case UIEffectType.Scale:
                    PlayScale(effect);
                    break;
                case UIEffectType.Shake:
                    PlayShake(effect);
                    break;
                case UIEffectType.Punch:
                    PlayPunch(effect);
                    break;
                case UIEffectType.Fade:
                    PlayFade(effect);
                    break;
                case UIEffectType.ColorFlash:
                    PlayColorFlash(effect);
                    break;
            }
        }

        public void CancelActive()
        {
            if (_activeHandle.IsActive()) _activeHandle.Cancel();
        }

        private void PlayScale(UIEffect effect)
        {
            if (_rectTransform == null) return;

            var target = 1f + effect.Strength * 0.1f;
            var rt = _rectTransform;

            _activeHandle = LMotion.Create(1f, target, effect.Duration)
                .WithScheduler(MotionScheduler.UpdateIgnoreTimeScale)
                .WithEase(effect.Easing)
                .WithLoops(2, LoopType.Yoyo)
                .Bind(rt, static (s, t) => t.localScale = new Vector3(s, s, 1f));
        }

        private void PlayShake(UIEffect effect)
        {
            if (_rectTransform == null) return;
            var strength = Vector3.one * effect.Strength;
            _activeHandle = LMotion.Shake.Create(Vector3.zero, strength, effect.Duration)
                .WithScheduler(MotionScheduler.UpdateIgnoreTimeScale)
                .WithFrequency(effect.Frequency)
                .WithDampingRatio(effect.DampingRatio)
                .BindToLocalPosition(_rectTransform);
        }

        private void PlayPunch(UIEffect effect)
        {
            if (_rectTransform == null) return;
            var strength = Vector3.one * effect.Strength;
            _activeHandle = LMotion.Punch.Create(Vector3.zero, strength, effect.Duration)
                .WithScheduler(MotionScheduler.UpdateIgnoreTimeScale)
                .WithFrequency(effect.Frequency)
                .WithDampingRatio(effect.DampingRatio)
                .BindToLocalPosition(_rectTransform);
        }

        private void PlayFade(UIEffect effect)
        {
            if (_canvasGroup == null) return;
            _activeHandle = LMotion.Create(1f, 0f, effect.Duration)
                .WithScheduler(MotionScheduler.UpdateIgnoreTimeScale)
                .WithEase(effect.Easing)
                .WithLoops(2, LoopType.Yoyo)
                .BindToAlpha(_canvasGroup);
        }

        private void PlayColorFlash(UIEffect effect)
        {
            var graphic = GetComponent<UnityEngine.UI.Graphic>();
            if (graphic == null) return;
            var original = graphic.color;
            var flash = Color.white;
            _activeHandle = LMotion.Create(original, flash, effect.Duration)
                .WithScheduler(MotionScheduler.UpdateIgnoreTimeScale)
                .WithEase(effect.Easing)
                .WithLoops(2, LoopType.Yoyo)
                .BindToColor(graphic);
        }

        private void OnDisable()
        {
            CancelActive();
        }
    }
}
