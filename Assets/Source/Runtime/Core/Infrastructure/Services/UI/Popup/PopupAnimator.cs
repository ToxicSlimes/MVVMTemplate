using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using LitMotion;
using LitMotion.Extensions;
using UnityEngine;

namespace Core.Infrastructure.Popup
{
    /// <summary>
    /// Fluent API wrapper around LitMotion for popup animations.
    /// Zero allocation, Burst-optimized animations.
    /// </summary>
    public class PopupAnimator
    {
        private readonly CanvasGroup _canvasGroup;
        private readonly RectTransform _rectTransform;
        private readonly List<Func<MotionHandle>> _motionBuilders = new();
        private readonly List<MotionHandle> _activeHandles = new();
        
        private Action _onStart;
        private Action _onComplete;
        private float _globalDelay;

        public PopupAnimator(CanvasGroup canvasGroup, RectTransform rectTransform)
        {
            _canvasGroup = canvasGroup;
            _rectTransform = rectTransform;
        }

        public PopupAnimator FadeIn(float duration, Ease ease = Ease.OutQuad)
        {
            return Fade(0f, 1f, duration, ease);
        }

        public PopupAnimator FadeOut(float duration, Ease ease = Ease.InQuad)
        {
            return Fade(1f, 0f, duration, ease);
        }

        public PopupAnimator Fade(float from, float to, float duration, Ease ease = Ease.Linear)
        {
            if (_canvasGroup == null) return this;

            var canvasGroup = _canvasGroup;
            _motionBuilders.Add(() =>
            {
                if (canvasGroup == null) return default;
                canvasGroup.alpha = from;
                return LMotion.Create(from, to, duration)
                    .WithScheduler(MotionScheduler.UpdateIgnoreTimeScale)
                    .WithEase(ease)
                    .WithDelay(_globalDelay)
                    .Bind(alpha =>
                    {
                        if (canvasGroup != null)
                            canvasGroup.alpha = alpha;
                    });
            });
            return this;
        }

        public PopupAnimator Scale(float from, float to, float duration, Ease ease = Ease.OutBack)
        {
            if (_rectTransform == null) return this;

            var rectTransform = _rectTransform;
            _motionBuilders.Add(() =>
            {
                if (rectTransform == null) return default;
                rectTransform.localScale = Vector3.one * from;
                return LMotion.Create(from, to, duration)
                    .WithScheduler(MotionScheduler.UpdateIgnoreTimeScale)
                    .WithEase(ease)
                    .WithDelay(_globalDelay)
                    .Bind(s =>
                    {
                        if (rectTransform != null)
                            rectTransform.localScale = new Vector3(s, s, 1f);
                    });
            });
            return this;
        }

        public PopupAnimator ScaleIn(float duration, Ease ease = Ease.OutBack)
        {
            return Scale(0.85f, 1f, duration, ease);
        }

        public PopupAnimator ScaleOut(float duration, Ease ease = Ease.InBack)
        {
            return Scale(1f, 0.85f, duration, ease);
        }

        public PopupAnimator MoveOffset(Vector2 from, Vector2 to, float duration, Ease ease = Ease.OutQuad)
        {
            if (_rectTransform == null) return this;

            var rectTransform = _rectTransform;
            var basePosition = rectTransform.anchoredPosition;

            _motionBuilders.Add(() =>
            {
                if (rectTransform == null) return default;
                return LMotion.Create(from, to, duration)
                    .WithScheduler(MotionScheduler.UpdateIgnoreTimeScale)
                    .WithEase(ease)
                    .WithDelay(_globalDelay)
                    .Bind(offset =>
                    {
                        if (rectTransform != null)
                            rectTransform.anchoredPosition = basePosition + offset;
                    });
            });
            return this;
        }

        public PopupAnimator SlideIn(Vector2 direction, float distance, float duration, Ease ease = Ease.OutQuad)
        {
            var from = direction.normalized * distance;
            return MoveOffset(from, Vector2.zero, duration, ease);
        }

        public PopupAnimator Punch(Vector3 strength, float duration, int frequency = 10, float dampingRatio = 1f)
        {
            if (_rectTransform == null) return this;

            var rectTransform = _rectTransform;
            _motionBuilders.Add(() =>
            {
                if (rectTransform == null) return default;
                return LMotion.Punch.Create(Vector3.zero, strength, duration)
                    .WithScheduler(MotionScheduler.UpdateIgnoreTimeScale)
                    .WithFrequency(frequency)
                    .WithDampingRatio(dampingRatio)
                    .WithDelay(_globalDelay)
                    .BindToLocalPosition(rectTransform);
            });
            return this;
        }

        public PopupAnimator Shake(Vector3 strength, float duration, int frequency = 10, float dampingRatio = 1f)
        {
            if (_rectTransform == null) return this;

            var rectTransform = _rectTransform;
            _motionBuilders.Add(() =>
            {
                if (rectTransform == null) return default;
                return LMotion.Shake.Create(Vector3.zero, strength, duration)
                    .WithScheduler(MotionScheduler.UpdateIgnoreTimeScale)
                    .WithFrequency(frequency)
                    .WithDampingRatio(dampingRatio)
                    .WithDelay(_globalDelay)
                    .BindToLocalPosition(rectTransform);
            });
            return this;
        }

        public PopupAnimator Delay(float seconds)
        {
            _globalDelay += seconds;
            return this;
        }

        public PopupAnimator OnStart(Action callback)
        {
            _onStart = callback;
            return this;
        }

        public PopupAnimator OnComplete(Action callback)
        {
            _onComplete = callback;
            return this;
        }

        public async UniTask Play(CancellationToken ct = default)
        {
            if (_motionBuilders.Count == 0)
            {
                _onComplete?.Invoke();
                return;
            }

            _onStart?.Invoke();

            Cancel();

            foreach (var builder in _motionBuilders)
            {
                var handle = builder();
                if (handle.IsActive())
                    _activeHandles.Add(handle);
            }

            try
            {
                var tasks = new UniTask[_activeHandles.Count];
                for (var i = 0; i < _activeHandles.Count; i++)
                {
                    tasks[i] = _activeHandles[i].ToUniTask(ct);
                }
                
                await UniTask.WhenAll(tasks);
            }
            catch (OperationCanceledException)
            {
                Cancel();
                throw;
            }

            _activeHandles.Clear();
            _onComplete?.Invoke();
        }

        public void PlayAndForget(CancellationToken ct = default)
        {
            Play(ct).Forget();
        }

        public void Cancel()
        {
            if (_activeHandles.Count == 0) return;
            
            var handles = _activeHandles.ToArray();
            _activeHandles.Clear();
            
            foreach (var handle in handles)
            {
                if (handle.IsActive())
                    handle.Cancel();
            }
        }

        public void Complete()
        {
            if (_activeHandles.Count == 0) return;
            
            var handles = _activeHandles.ToArray();
            _activeHandles.Clear();
            
            foreach (var handle in handles)
            {
                if (handle.IsActive())
                    handle.Complete();
            }
        }
    }
}
