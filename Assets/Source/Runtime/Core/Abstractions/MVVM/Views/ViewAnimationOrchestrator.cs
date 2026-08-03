using System;
using System.Collections.Generic;
using System.Linq;
using Core.Abstractions.MVVM.Animation;
using Cysharp.Threading.Tasks;
using UnityEngine;
using AnimationPlayMode = Core.Abstractions.MVVM.Animation.AnimationPlayMode;

namespace Core.Infrastructure.MVVM.Animation
{
    [DisallowMultipleComponent]
    public class ViewAnimationOrchestrator : MonoBehaviour
    {
        [SerializeField] private AnimationPlayMode _showMode = AnimationPlayMode.Stagger;
        [SerializeField] private AnimationPlayMode _hideMode = AnimationPlayMode.Stagger;
        [SerializeField] private float _staggerDelay = 0.05f;
        [SerializeField] private bool _hideInReverseOrder = true;
        [SerializeField] private bool _autoCollectPanels = true;

        private readonly List<IPanelAnimator> _panels = new();
        private bool _isPlaying;

        public bool IsPlaying => _isPlaying;
        public IReadOnlyList<IPanelAnimator> Panels => _panels;

        private void Awake()
        {
            if (_autoCollectPanels)
                CollectPanels();
        }

        public void CollectPanels()
        {
            _panels.Clear();
            _panels.AddRange(
                GetComponentsInChildren<IPanelAnimator>(true)
                    .OrderBy(p => p.Order)
            );
        }

        public void Register(IPanelAnimator panel)
        {
            if (!_panels.Contains(panel))
                _panels.Add(panel);
        }

        public void Unregister(IPanelAnimator panel)
        {
            _panels.Remove(panel);
        }

        public async UniTask PlayShowAsync()
        {
            _isPlaying = true;
            try
            {
                var ordered = _panels.OrderBy(p => p.Order).ToList();
                await PlayPanels(ordered, _showMode, panel => panel.PlayShowAsync());
            }
            finally
            {
                _isPlaying = false;
            }
        }

        public async UniTask PlayHideAsync()
        {
            _isPlaying = true;
            try
            {
                var ordered = _hideInReverseOrder
                    ? _panels.OrderByDescending(p => p.Order).ToList()
                    : _panels.OrderBy(p => p.Order).ToList();

                await PlayPanels(ordered, _hideMode, panel => panel.PlayHideAsync());
            }
            finally
            {
                _isPlaying = false;
            }
        }

        public async UniTask PlayShowForAsync(string panelId)
        {
            var panel = _panels.FirstOrDefault(p => p.PanelId == panelId);
            if (panel != null)
                await panel.PlayShowAsync();
        }

        public async UniTask PlayHideForAsync(string panelId)
        {
            var panel = _panels.FirstOrDefault(p => p.PanelId == panelId);
            if (panel != null)
                await panel.PlayHideAsync();
        }

        public void StopAll()
        {
            foreach (var panel in _panels)
                panel.StopImmediate();
            _isPlaying = false;
        }

        public void ResetAllToHidden()
        {
            foreach (var panel in _panels)
                panel.ResetToHiddenState();
        }

        public void ResetAllToShown()
        {
            foreach (var panel in _panels)
                panel.ResetToShownState();
        }

        private async UniTask PlayPanels(List<IPanelAnimator> panels, AnimationPlayMode mode, Func<IPanelAnimator, UniTask> action)
        {
            switch (mode)
            {
                case AnimationPlayMode.Parallel:
                    await UniTask.WhenAll(panels.Select(action));
                    break;

                case AnimationPlayMode.Sequential:
                    foreach (var panel in panels)
                        await action(panel);
                    break;

                case AnimationPlayMode.Stagger:
                    var groups = panels.GroupBy(p => p.Order).OrderBy(g => g.Key);
                    foreach (var group in groups)
                    {
                        var groupTasks = group.Select(action).ToList();
                        UniTask.WhenAll(groupTasks).Forget();
                        await UniTask.Delay(TimeSpan.FromSeconds(_staggerDelay), cancellationToken: destroyCancellationToken);
                    }
                    // ждём последнюю группу
                    await UniTask.WaitWhile(() => panels.Any(p => p.IsAnimating), cancellationToken: destroyCancellationToken);
                    break;
            }
        }
    }
}
