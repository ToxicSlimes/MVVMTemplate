using Cysharp.Threading.Tasks;

namespace Core.Abstractions.MVVM.Animation
{
    public interface IPanelAnimator
    {
        string PanelId { get; }
        int Order { get; }
        bool IsAnimating { get; }

        UniTask PlayShowAsync();
        UniTask PlayHideAsync();
        void StopImmediate();
        void ResetToHiddenState();
        void ResetToShownState();
    }
}
