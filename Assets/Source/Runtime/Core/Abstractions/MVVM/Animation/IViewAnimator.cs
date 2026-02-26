using Core.Abstractions.MVVM.Views;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Core.Abstractions.MVVM.Animation
{
    public interface IViewAnimator
    {
        UniTask AnimateShowAsync(IView view, ViewTransitionStyle transitionStyle, float? duration = null);

        UniTask AnimateHideAsync(IView view, ViewTransitionStyle transitionStyle, float? duration = null);

        void StopAnimation(IView view);
    }
}
