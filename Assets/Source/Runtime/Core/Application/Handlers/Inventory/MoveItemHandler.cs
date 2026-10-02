using System.Threading;
using Core.Abstractions.MVVM.Models.Inventory;
using Core.Application.Commands.Inventory;
using Core.Application.Notifications.Inventory;
using Core.Mediator.Abstractions;
using Core.Mediator.Handlers;
using Cysharp.Threading.Tasks;

namespace Core.Application.Handlers.Inventory
{
    public sealed class MoveItemHandler : IRequestHandler<MoveItemCommand, Nothing>
    {
        private readonly IInventoryModel _model;
        private readonly IMediator _mediator;

        public MoveItemHandler(IInventoryModel model, IMediator mediator)
        {
            _model = model;
            _mediator = mediator;
        }

        public async UniTask<Nothing> Handle(MoveItemCommand command, CancellationToken ct)
        {
            _model.MoveItem(command.FromIndex, command.ToIndex);
            await _mediator.Publish(new InventoryChangedNotification(_model.Items), ct);
            return Nothing.Value;
        }
    }
}
