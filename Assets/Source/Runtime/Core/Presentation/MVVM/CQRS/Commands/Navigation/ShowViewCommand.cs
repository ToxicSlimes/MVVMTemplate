using Core.Abstractions.MVVM.Views;
using Core.Infrastructure.Mediator.Abstractions;

namespace Core.Application.Commands.Common
{
    public readonly struct ShowViewCommand<TViewModel> : IRequest<bool>
        where TViewModel : class
    {
        public readonly ViewShowParams ShowParams;

        public ShowViewCommand(ViewShowParams showParams = null)
        {
            ShowParams = showParams;
        }
    }
}
