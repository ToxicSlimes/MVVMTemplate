using Core.Infrastructure.Mediator.Abstractions;

namespace Core.Application.Commands.Platform
{
    public readonly struct CopyToClipboardCommand : IRequest<Nothing>
    {
        public readonly string Text;

        public CopyToClipboardCommand(string text)
        {
            Text = text;
        }
    }
}
