using Cysharp.Threading.Tasks;

namespace Core.Abstractions.MVVM.Animation
{
    public enum AnimationPhase
    {
        BeforeShow,
        AfterShow,
        BeforeHide,
        AfterHide
    }

    public interface IAnimationHook
    {
        AnimationPhase Phase { get; }
        int Priority { get; }
        bool IsBlocking { get; }

        UniTask ExecuteAsync();
    }
}
