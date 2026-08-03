using Core.Abstractions.MVVM.ViewModels;
using Core.Abstractions.MVVM.Views;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Core.Infrastructure.MVVM.Animation
{
    public abstract class AnimatedView<TViewModel> : BindableView<TViewModel> where TViewModel : DisposableObject
    {
        private ViewAnimationOrchestrator _orchestrator;

        protected ViewAnimationOrchestrator Orchestrator
        {
            get
            {
                if (_orchestrator == null)
                    _orchestrator = GetComponent<ViewAnimationOrchestrator>();
                return _orchestrator;
            }
        }

        protected override async UniTask PlayShowAnimation()
        {
            if (Orchestrator != null)
            {
                Orchestrator.ResetAllToHidden();
                await Orchestrator.PlayShowAsync();
            }
            else
            {
                await base.PlayShowAnimation();
            }
        }

        protected override async UniTask PlayHideAnimation()
        {
            if (Orchestrator != null)
            {
                await Orchestrator.PlayHideAsync();
            }
            else
            {
                await base.PlayHideAnimation();
            }
        }
    }
}
