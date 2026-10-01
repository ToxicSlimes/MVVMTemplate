using Core.Infrastructure.Mediator.Abstractions;

namespace Core.Application.Commands.Common
{
    public readonly struct HideViewCommand<TViewModel> : IRequest<bool>
        where TViewModel : class
    {
        public readonly bool Animate;

        public HideViewCommand(bool animate = true)
        {
            Animate = animate;
        }
    }
}
