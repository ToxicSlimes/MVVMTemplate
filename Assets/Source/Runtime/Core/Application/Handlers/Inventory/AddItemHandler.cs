using System.Threading;
using Core.Abstractions.MVVM.Models.Inventory;
using Core.Application.Commands.Inventory;
using Core.Application.Notifications.Inventory;
using Core.Infrastructure.Mediator.Abstractions;
using Core.Infrastructure.Mediator.Handlers;
using Cysharp.Threading.Tasks;

namespace Core.Application.Handlers.Inventory
{
    public sealed class AddItemHandler : IRequestHandler<AddItemCommand, Nothing>
    {
        private readonly IInventoryModel _model;
        private readonly IMediator _mediator;

        public AddItemHandler(IInventoryModel model, IMediator mediator)
        {
            _model = model;
            _mediator = mediator;
        }

        public async UniTask<Nothing> Handle(AddItemCommand command, CancellationToken ct)
        {
            _model.AddItem(command.Item);
            await _mediator.Publish(new InventoryChangedNotification(_model.Items), ct);
            return Nothing.Value;
        }
    }
}
