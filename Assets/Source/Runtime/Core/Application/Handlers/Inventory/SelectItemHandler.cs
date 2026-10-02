using System.Threading;
using Core.Abstractions.MVVM.Models.Inventory;
using Core.Application.Commands.Inventory;
using Core.Application.Notifications.Inventory;
using Core.Mediator.Abstractions;
using Core.Mediator.Handlers;
using Cysharp.Threading.Tasks;

namespace Core.Application.Handlers.Inventory
{
    public sealed class SelectItemHandler : IRequestHandler<SelectItemCommand, Nothing>
    {
        private readonly IInventoryModel _model;
        private readonly IMediator _mediator;

        public SelectItemHandler(IInventoryModel model, IMediator mediator)
        {
            _model = model;
            _mediator = mediator;
        }

        public async UniTask<Nothing> Handle(SelectItemCommand command, CancellationToken ct)
        {
            _model.SelectItem(command.ItemId);
            await _mediator.Publish(new ItemSelectedNotification(_model.SelectedItem), ct);
            return Nothing.Value;
        }
    }
}
