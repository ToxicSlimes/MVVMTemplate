using Core.Domain.Enums;

namespace Core.Abstractions.UI
{
    public interface IControlBehavior
    {
        SoundType ClickSound { get; }
        SoundType HoverSound { get; }
        void TransitionTo(ControlState state);
        void PlayHoverEffect();
        void PlayPressEffect();
        void PlaySound(SoundType sound);
    }
}
