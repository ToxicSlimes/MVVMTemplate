using System;
using System.Collections.Generic;
using System.Linq;
using Core.Abstractions.MVVM.Animation;
using Core.Abstractions.MVVM.Views;
using Cysharp.Threading.Tasks;
using LitMotion;
using UnityEngine;

namespace Core.Presentation.MVVM.Animation
{
    [DisallowMultipleComponent]
    public class PanelAnimator : MonoBehaviour, IPanelAnimator
    {
        [SerializeField] private string _panelId;
        [SerializeField] private int _order;

        [Header("Transition (inline or preset)")]
        [SerializeField] private PanelTransitionPreset _preset;

        [Header("Inline Config (used when no preset)")]
        [SerializeField] private ViewTransitionStyle _showTransition = ViewTransitionStyle.Fade;
        [SerializeField] private float _showDuration = 0.25f;
        [SerializeField] private float _showDelay;
        [SerializeField] private Ease _showEase = Ease.OutQuad;

        [SerializeField] private ViewTransitionStyle _hideTransition = ViewTransitionStyle.Fade;
        [SerializeField] private float _hideDuration = 0.2f;
        [SerializeField] private float _hideDelay;
        [SerializeField] private Ease _hideEase = Ease.InQuad;

        [SerializeField, Range(0f, 2f)] private float _slideOffset = 1f;

        public string PanelId => string.IsNullOrEmpty(_panelId) ? gameObject.name : _panelId;
        public int Order => _order;
        public bool IsAnimating => _activeHandles.Count > 0 && _activeHandles.Any(h => h.IsActive());

        private readonly List<MotionHandle> _activeHandles = new();
        private readonly List<IAnimationHook> _hooks = new();

        private RectTransform _rect;
        private CanvasGroup _canvasGroup;
        private Vector2 _originalPosition;
        private bool _positionCaptured;

        private ViewTransitionStyle ShowStyle => _preset != null ? _preset.ShowTransition : _showTransition;
        private float ShowDuration => _preset != null ? _preset.ShowDuration : _showDuration;
        private float ShowDelay => _preset != null ? _preset.ShowDelay : _showDelay;
        private Ease ShowEase => _preset != null ? _preset.ShowEase : _showEase;
        private ViewTransitionStyle HideStyle => _preset != null ? _preset.HideTransition : _hideTransition;
        private float HideDuration => _preset != null ? _preset.HideDuration : _hideDuration;
        private float HideDelay => _preset != null ? _preset.HideDelay : _hideDelay;
        private Ease HideEase => _preset != null ? _preset.HideEase : _hideEase;
        private float SlideOffsetValue => _preset != null ? _preset.SlideOffset : _slideOffset;

        private void Awake()
        {
            _rect = GetComponent<RectTransform>();
            _canvasGroup = GetComponent<CanvasGroup>();
            if (_canvasGroup == null)
                _canvasGroup = gameObject.AddComponent<CanvasGroup>();

            CaptureOriginalPosition();
        }

        public void AddHook(IAnimationHook hook)
        {
            _hooks.Add(hook);
        }

        public void RemoveHook(IAnimationHook hook)
        {
            _hooks.Remove(hook);
        }

        public void ClearHooks()
        {
            _hooks.Clear();
        }

        public async UniTask PlayShowAsync()
        {
            StopImmediate();
            CaptureOriginalPosition();

            await RunHooksAsync(AnimationPhase.BeforeShow);

            if (ShowDelay > 0)
                await UniTask.Delay(TimeSpan.FromSeconds(ShowDelay), cancellationToken: destroyCancellationToken);

            var tasks = new List<UniTask>();

            if (ShowStyle.HasFlag(ViewTransitionStyle.Fade))
                tasks.Add(AnimateFade(0f, 1f, ShowDuration, ShowEase));

            if (HasSlideFlag(ShowStyle))
                tasks.Add(AnimateSlideIn(ShowStyle, ShowDuration, ShowEase));

            if (ShowStyle.HasFlag(ViewTransitionStyle.Scale))
                tasks.Add(AnimateScale(0f, 1f, ShowDuration, ShowStyle.HasFlag(ViewTransitionStyle.Bounce) ? Ease.OutBack : ShowEase));

            if (ShowStyle == ViewTransitionStyle.None)
            {
                _canvasGroup.alpha = 1f;
                _canvasGroup.interactable = true;
                _canvasGroup.blocksRaycasts = true;
            }

            if (tasks.Count > 0)
                await UniTask.WhenAll(tasks);

            _canvasGroup.interactable = true;
            _canvasGroup.blocksRaycasts = true;

            await RunHooksAsync(AnimationPhase.AfterShow);
        }

        public async UniTask PlayHideAsync()
        {
            StopImmediate();

            await RunHooksAsync(AnimationPhase.BeforeHide);

            if (HideDelay > 0)
                await UniTask.Delay(TimeSpan.FromSeconds(HideDelay), cancellationToken: destroyCancellationToken);

            _canvasGroup.interactable = false;

            var tasks = new List<UniTask>();

            if (HideStyle.HasFlag(ViewTransitionStyle.Fade))
                tasks.Add(AnimateFade(1f, 0f, HideDuration, HideEase));

            if (HasSlideFlag(HideStyle))
                tasks.Add(AnimateSlideOut(HideStyle, HideDuration, HideEase));

            if (HideStyle.HasFlag(ViewTransitionStyle.Scale))
                tasks.Add(AnimateScale(1f, 0f, HideDuration, HideStyle.HasFlag(ViewTransitionStyle.Bounce) ? Ease.InBack : HideEase));

            if (HideStyle == ViewTransitionStyle.None)
            {
                _canvasGroup.alpha = 0f;
                _canvasGroup.blocksRaycasts = false;
            }

            if (tasks.Count > 0)
                await UniTask.WhenAll(tasks);

            _canvasGroup.blocksRaycasts = false;

            await RunHooksAsync(AnimationPhase.AfterHide);
        }

        public void StopImmediate()
        {
            foreach (var handle in _activeHandles)
            {
                if (handle.IsActive())
                    handle.Cancel();
            }
            _activeHandles.Clear();
        }

        public void ResetToHiddenState()
        {
            StopImmediate();
            _canvasGroup.alpha = 0f;
            _canvasGroup.interactable = false;
            _canvasGroup.blocksRaycasts = false;
        }

        public void ResetToShownState()
        {
            StopImmediate();
            if (_rect != null && _positionCaptured)
                _rect.anchoredPosition = _originalPosition;
            transform.localScale = Vector3.one;
            _canvasGroup.alpha = 1f;
            _canvasGroup.interactable = true;
            _canvasGroup.blocksRaycasts = true;
        }

        private void CaptureOriginalPosition()
        {
            if (_rect != null && !_positionCaptured)
            {
                _originalPosition = _rect.anchoredPosition;
                _positionCaptured = true;
            }
        }

        private UniTask AnimateFade(float from, float to, float duration, Ease ease)
        {
            _canvasGroup.alpha = from;
            var handle = LMotion.Create(from, to, duration)
                .WithScheduler(MotionScheduler.UpdateIgnoreTimeScale)
                .WithEase(ease)
                .Bind(a => { if (_canvasGroup != null) _canvasGroup.alpha = a; });
            _activeHandles.Add(handle);
            return handle.ToUniTask();
        }

        private UniTask AnimateScale(float from, float to, float duration, Ease ease)
        {
            transform.localScale = Vector3.one * from;
            var handle = LMotion.Create(from, to, duration)
                .WithScheduler(MotionScheduler.UpdateIgnoreTimeScale)
                .WithEase(ease)
                .Bind(s => transform.localScale = Vector3.one * s);
            _activeHandles.Add(handle);
            return handle.ToUniTask();
        }

        private UniTask AnimateSlideIn(ViewTransitionStyle style, float duration, Ease ease)
        {
            if (_rect == null) return UniTask.CompletedTask;

            var offset = GetSlideOffset(style);
            _rect.anchoredPosition = _originalPosition + offset;

            var handle = LMotion.Create(_rect.anchoredPosition, _originalPosition, duration)
                .WithScheduler(MotionScheduler.UpdateIgnoreTimeScale)
                .WithEase(ease)
                .Bind(p => { if (_rect != null) _rect.anchoredPosition = p; });
            _activeHandles.Add(handle);
            return handle.ToUniTask();
        }

        private UniTask AnimateSlideOut(ViewTransitionStyle style, float duration, Ease ease)
        {
            if (_rect == null) return UniTask.CompletedTask;

            var target = _originalPosition + GetSlideOffset(style);

            var handle = LMotion.Create(_rect.anchoredPosition, target, duration)
                .WithScheduler(MotionScheduler.UpdateIgnoreTimeScale)
                .WithEase(ease)
                .Bind(p => { if (_rect != null) _rect.anchoredPosition = p; });
            _activeHandles.Add(handle);
            return handle.ToUniTask();
        }

        private Vector2 GetSlideOffset(ViewTransitionStyle style)
        {
            var offset = Vector2.zero;
            var rect = _rect.rect;
            var multiplier = SlideOffsetValue;

            if (style.HasFlag(ViewTransitionStyle.SlideUp))
                offset.y -= (rect.height + 50f) * multiplier;
            if (style.HasFlag(ViewTransitionStyle.SlideDown))
                offset.y += (rect.height + 50f) * multiplier;
            if (style.HasFlag(ViewTransitionStyle.SlideLeft))
                offset.x -= (rect.width + 50f) * multiplier;
            if (style.HasFlag(ViewTransitionStyle.SlideRight))
                offset.x += (rect.width + 50f) * multiplier;

            return offset;
        }

        private async UniTask RunHooksAsync(AnimationPhase phase)
        {
            var phaseHooks = _hooks
                .Where(h => h.Phase == phase)
                .OrderBy(h => h.Priority)
                .ToList();

            var fireAndForget = new List<UniTask>();

            foreach (var hook in phaseHooks)
            {
                if (hook.IsBlocking)
                    await hook.ExecuteAsync();
                else
                    fireAndForget.Add(hook.ExecuteAsync());
            }

            if (fireAndForget.Count > 0)
                UniTask.WhenAll(fireAndForget).Forget();
        }

        private static bool HasSlideFlag(ViewTransitionStyle style)
        {
            return style.HasFlag(ViewTransitionStyle.SlideUp) ||
                   style.HasFlag(ViewTransitionStyle.SlideDown) ||
                   style.HasFlag(ViewTransitionStyle.SlideLeft) ||
                   style.HasFlag(ViewTransitionStyle.SlideRight);
        }

        private void OnDestroy()
        {
            StopImmediate();
        }
    }
}
