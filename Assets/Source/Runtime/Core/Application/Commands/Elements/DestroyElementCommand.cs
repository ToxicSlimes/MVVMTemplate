using Core.Abstractions.MVVM.Elements;
using Core.Infrastructure.Mediator.Abstractions;

namespace Core.Application.Commands.Elements
{
    public readonly struct DestroyElementCommand : IRequest<Nothing>
    {
        public readonly IBindableElement Element;

        public DestroyElementCommand(IBindableElement element)
        {
            Element = element;
        }
    }
}
