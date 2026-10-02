using System;
using Core.Abstractions.MVVM.Animation;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Core.Presentation.MVVM.Animation.Hooks
{
    public class CallbackAnimationHook : MonoBehaviour, IAnimationHook
    {
        [SerializeField] private AnimationPhase _phase = AnimationPhase.AfterShow;
        [SerializeField] private int _priority;
        [SerializeField] private bool _isBlocking;

        private Func<UniTask> _callback;

        public AnimationPhase Phase => _phase;
        public int Priority => _priority;
        public bool IsBlocking => _isBlocking;

        public CallbackAnimationHook Setup(AnimationPhase phase, Func<UniTask> callback, bool blocking = false, int priority = 0)
        {
            _phase = phase;
            _callback = callback;
            _isBlocking = blocking;
            _priority = priority;
            return this;
        }

        private void Awake()
        {
            var panel = GetComponent<PanelAnimator>();
            if (panel != null)
                panel.AddHook(this);
        }

        public async UniTask ExecuteAsync()
        {
            if (_callback != null)
                await _callback.Invoke();
        }

        private void OnDestroy()
        {
            var panel = GetComponent<PanelAnimator>();
            if (panel != null)
                panel.RemoveHook(this);
        }
    }
}
