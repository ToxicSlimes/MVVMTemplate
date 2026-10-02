using Core.Abstractions.MVVM.Animation;
using Core.Presentation.UI.Effects;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Core.Presentation.MVVM.Animation.Hooks
{
    public class UIEffectAnimationHook : MonoBehaviour, IAnimationHook
    {
        [SerializeField] private AnimationPhase _phase = AnimationPhase.AfterShow;
        [SerializeField] private int _priority;
        [SerializeField] private UIEffect _effect;
        [SerializeField] private UIEffectPlayer _targetPlayer;

        public AnimationPhase Phase => _phase;
        public int Priority => _priority;
        public bool IsBlocking => false;

        private void Awake()
        {
            if (_targetPlayer == null)
                _targetPlayer = GetComponent<UIEffectPlayer>();

            var panel = GetComponent<PanelAnimator>();
            if (panel != null)
                panel.AddHook(this);
        }

        public UniTask ExecuteAsync()
        {
            if (_targetPlayer != null && _effect != null)
                _targetPlayer.Play(_effect);

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
