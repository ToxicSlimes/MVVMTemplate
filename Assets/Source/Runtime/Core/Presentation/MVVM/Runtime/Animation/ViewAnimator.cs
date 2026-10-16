using System.Collections.Generic;
using Core.Abstractions.MVVM.Animation;
using Core.Abstractions.MVVM.Views;
using Cysharp.Threading.Tasks;
using LitMotion;
using UnityEngine;

namespace Core.Presentation.MVVM.Animation
{
    public class ViewAnimator : IViewAnimator
    {
        private const float DefaultShowDuration = 0.25f;
        private const float DefaultHideDuration = 0.2f;

        private readonly Dictionary<IView, List<MotionHandle>> _activeAnimations = new();

        public async UniTask AnimateShowAsync(IView view, ViewTransitionStyle transitionStyle, float? duration = null)
        {
            if (view is not Component component)
                return;

            if (transitionStyle == ViewTransitionStyle.None)
                return;

            var animDuration = duration ?? DefaultShowDuration;
            var handles = new List<MotionHandle>();
            _activeAnimations[view] = handles;

            try
            {
                var tasks = new List<UniTask>();

                if (transitionStyle.HasFlag(ViewTransitionStyle.Fade))
                {
                    var handle = AnimateFade(component, 0f, 1f, animDuration, Ease.OutQuad);
                    handles.Add(handle);
                    tasks.Add(handle.ToUniTask());
                }

                if (HasSlideFlag(transitionStyle))
                {
                    var useBounce = transitionStyle.HasFlag(ViewTransitionStyle.Bounce);
                    var handle = AnimateSlideIn(component, transitionStyle, animDuration, useBounce);
                    handles.Add(handle);
                    tasks.Add(handle.ToUniTask());
                }

                if (transitionStyle.HasFlag(ViewTransitionStyle.Scale))
                {
                    var handle = AnimateScale(component, 0f, 1f, animDuration, Ease.OutBack);
                    handles.Add(handle);
                    tasks.Add(handle.ToUniTask());
                }

                if (tasks.Count > 0)
                    await UniTask.WhenAll(tasks);
            }
            finally
            {
                _activeAnimations.Remove(view);
            }
        }

        public async UniTask AnimateHideAsync(IView view, ViewTransitionStyle transitionStyle, float? duration = null)
        {
            if (view is not Component component)
                return;

            if (transitionStyle == ViewTransitionStyle.None)
                return;

            var animDuration = duration ?? DefaultHideDuration;
            var handles = new List<MotionHandle>();
            _activeAnimations[view] = handles;

            try
            {
                var tasks = new List<UniTask>();

                if (transitionStyle.HasFlag(ViewTransitionStyle.Fade))
                {
                    var handle = AnimateFade(component, 1f, 0f, animDuration, Ease.InQuad);
                    handles.Add(handle);
                    tasks.Add(handle.ToUniTask());
                }

                if (HasSlideFlag(transitionStyle))
                {
                    var useBounce = transitionStyle.HasFlag(ViewTransitionStyle.Bounce);
                    var handle = AnimateSlideOut(component, transitionStyle, animDuration, useBounce);
                    handles.Add(handle);
                    tasks.Add(handle.ToUniTask());
                }

                if (transitionStyle.HasFlag(ViewTransitionStyle.Scale))
                {
                    var handle = AnimateScale(component, 1f, 0f, animDuration, Ease.InCubic);
                    handles.Add(handle);
                    tasks.Add(handle.ToUniTask());
                }

                if (tasks.Count > 0)
                    await UniTask.WhenAll(tasks);
            }
            finally
            {
                _activeAnimations.Remove(view);
            }
        }

        public void StopAnimation(IView view)
        {
            if (view == null || !_activeAnimations.TryGetValue(view, out var handles))
                return;

            foreach (var handle in handles)
            {
                if (handle.IsActive())
                    handle.Cancel();
            }

            _activeAnimations.Remove(view);
        }

        private static MotionHandle AnimateFade(Component component, float from, float to, float duration, Ease ease)
        {
            var canvasGroup = component.GetComponent<CanvasGroup>();
            if (canvasGroup == null)
                canvasGroup = component.gameObject.AddComponent<CanvasGroup>();

            canvasGroup.alpha = from;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = to > 0.5f;

            return LMotion.Create(from, to, duration)
                .WithScheduler(MotionScheduler.UpdateIgnoreTimeScale)
                .WithEase(ease)
                .WithOnComplete(() =>
                {
                    if (canvasGroup != null)
                    {
                        canvasGroup.alpha = to;
                        canvasGroup.interactable = to > 0.5f;
                        canvasGroup.blocksRaycasts = to > 0.5f;
                    }
                })
                .Bind(alpha =>
                {
                    if (canvasGroup != null)
                        canvasGroup.alpha = alpha;
                });
        }

        private static MotionHandle AnimateScale(Component component, float from, float to, float duration, Ease ease)
        {
            var transform = component.transform;
            transform.localScale = Vector3.one * from;

            return LMotion.Create(from, to, duration)
                .WithScheduler(MotionScheduler.UpdateIgnoreTimeScale)
                .WithEase(ease)
                .WithOnComplete(() => transform.localScale = Vector3.one * to)
                .Bind(s => transform.localScale = Vector3.one * s);
        }

        private static MotionHandle AnimateSlideIn(Component component, ViewTransitionStyle style, float duration, bool useBounce = false)
        {
            var rectTransform = component.GetComponent<RectTransform>();
            if (rectTransform == null)
                return default;

            var startPosition = GetOffscreenPosition(rectTransform, style);
            var endPosition = Vector2.zero;

            rectTransform.anchoredPosition = startPosition;

            var ease = useBounce ? Ease.OutBack : Ease.OutCubic;

            return LMotion.Create(startPosition, endPosition, duration)
                .WithScheduler(MotionScheduler.UpdateIgnoreTimeScale)
                .WithEase(ease)
                .WithOnComplete(() => rectTransform.anchoredPosition = endPosition)
                .Bind(pos => rectTransform.anchoredPosition = pos);
        }

        private static MotionHandle AnimateSlideOut(Component component, ViewTransitionStyle style, float duration, bool useBounce = false)
        {
            var rectTransform = component.GetComponent<RectTransform>();
            if (rectTransform == null)
                return default;

            var startPosition = rectTransform.anchoredPosition;
            var endPosition = GetOffscreenPosition(rectTransform, style);

            var ease = useBounce ? Ease.InBack : Ease.InCubic;

            return LMotion.Create(startPosition, endPosition, duration)
                .WithScheduler(MotionScheduler.UpdateIgnoreTimeScale)
                .WithEase(ease)
                .WithOnComplete(() => rectTransform.anchoredPosition = endPosition)
                .Bind(pos => rectTransform.anchoredPosition = pos);
        }

        private static Vector2 GetOffscreenPosition(RectTransform rectTransform, ViewTransitionStyle style)
        {
            var offset = Vector2.zero;
            var rect = rectTransform.rect;

            if (style.HasFlag(ViewTransitionStyle.SlideUp))
                offset.y -= rect.height + 100f;

            if (style.HasFlag(ViewTransitionStyle.SlideDown))
                offset.y += rect.height + 100f;

            if (style.HasFlag(ViewTransitionStyle.SlideLeft))
                offset.x += rect.width + 100f;

            if (style.HasFlag(ViewTransitionStyle.SlideRight))
                offset.x -= rect.width + 100f;

            return offset;
        }

        private static bool HasSlideFlag(ViewTransitionStyle style)
        {
            return style.HasFlag(ViewTransitionStyle.SlideUp) ||
                   style.HasFlag(ViewTransitionStyle.SlideDown) ||
                   style.HasFlag(ViewTransitionStyle.SlideLeft) ||
                   style.HasFlag(ViewTransitionStyle.SlideRight);
        }
    }
}
