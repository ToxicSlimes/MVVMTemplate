using Core.Mediator.Abstractions;

namespace Core.Application.Queries.Common
{
    public readonly struct ConfirmDialogQuery : IRequest<bool>
    {
        public readonly string Title;
        public readonly string Message;
        public readonly string ConfirmText;
        public readonly string CancelText;

        public ConfirmDialogQuery(string title, string message, string confirmText = "OK", string cancelText = "Cancel")
        {
            Title = title;
            Message = message;
            ConfirmText = confirmText;
            CancelText = cancelText;
        }
    }
}
