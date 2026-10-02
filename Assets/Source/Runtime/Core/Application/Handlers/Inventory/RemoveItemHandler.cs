using System.Threading;
using Core.Abstractions.MVVM.Models.Inventory;
using Core.Application.Commands.Inventory;
using Core.Application.Notifications.Inventory;
using Core.Mediator.Abstractions;
using Core.Mediator.Handlers;
using Cysharp.Threading.Tasks;

namespace Core.Application.Handlers.Inventory
{
    public sealed class RemoveItemHandler : IRequestHandler<RemoveItemCommand, Nothing>
    {
        private readonly IInventoryModel _model;
        private readonly IMediator _mediator;

        public RemoveItemHandler(IInventoryModel model, IMediator mediator)
        {
            _model = model;
            _mediator = mediator;
        }

        public async UniTask<Nothing> Handle(RemoveItemCommand command, CancellationToken ct)
        {
            _model.RemoveItem(command.ItemId);
            await _mediator.Publish(new InventoryChangedNotification(_model.Items), ct);
            return Nothing.Value;
        }
    }
}
