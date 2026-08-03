using Core.Abstractions.MVVM.Animation;
using Core.Abstractions.Services;
using Core.Data.Domain.Enums;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Core.Infrastructure.MVVM.Animation.Hooks
{
    public class SoundAnimationHook : MonoBehaviour, IAnimationHook
    {
        [SerializeField] private AnimationPhase _phase = AnimationPhase.BeforeShow;
        [SerializeField] private int _priority;
        [SerializeField] private SoundType _sound;
        [SerializeField] private bool _randomPitch = true;

        private IAudioService _audioService;

        public AnimationPhase Phase => _phase;
        public int Priority => _priority;
        public bool IsBlocking => false;

        public void Construct(IAudioService audioService)
        {
            _audioService = audioService;
        }

        private void Awake()
        {
            var panel = GetComponent<PanelAnimator>();
            if (panel != null)
                panel.AddHook(this);
        }

        public UniTask ExecuteAsync()
        {
            _audioService?.Play(_sound, _randomPitch);
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
