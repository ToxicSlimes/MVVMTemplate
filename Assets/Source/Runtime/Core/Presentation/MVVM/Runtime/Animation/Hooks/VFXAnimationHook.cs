using Core.Abstractions.MVVM.Animation;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Core.Presentation.MVVM.Animation.Hooks
{
    public class VFXAnimationHook : MonoBehaviour, IAnimationHook
    {
        [SerializeField] private AnimationPhase _phase = AnimationPhase.AfterShow;
        [SerializeField] private int _priority;
        [SerializeField] private ParticleSystem _particleSystem;
        [SerializeField] private bool _stopOnOppositePhase = true;
        [SerializeField] private bool _isBlocking;

        public AnimationPhase Phase => _phase;
        public int Priority => _priority;
        public bool IsBlocking => _isBlocking;

        private void Awake()
        {
            var panel = GetComponent<PanelAnimator>();
            if (panel != null)
                panel.AddHook(this);
        }

        public UniTask ExecuteAsync()
        {
            if (_particleSystem == null)
                return UniTask.CompletedTask;

            _particleSystem.Play();
            return UniTask.CompletedTask;
        }

        private void OnDestroy()
        {
            var panel = GetComponent<PanelAnimator>();
            if (panel != null)
                panel.RemoveHook(this);
        }
    }
}
