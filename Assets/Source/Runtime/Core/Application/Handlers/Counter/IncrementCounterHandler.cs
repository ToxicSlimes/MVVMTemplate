using System.Threading;
using Core.Abstractions.MVVM.Models.Counter;
using Core.Application.Commands.Counter;
using Core.Application.Notifications.Counter;
using Core.Mediator.Abstractions;
using Core.Mediator.Handlers;
using Cysharp.Threading.Tasks;

namespace Core.Application.Handlers.Counter
{
    /// <summary>
    /// TEMPLATE EXAMPLE: Command handler — executes the increment logic.
    /// Handlers are where the "work" happens. They receive Commands/Queries
    /// and interact with Models/Services.
    /// Note how the handler also publishes a Notification — this is the
    /// recommended way to broadcast state changes to other parts of the app.
    /// </summary>
    public sealed class IncrementCounterHandler : IRequestHandler<IncrementCounterCommand, Nothing>
    {
        private readonly ICounterModel _model;
        private readonly IMediator _mediator;

        public IncrementCounterHandler(ICounterModel model, IMediator mediator)
        {
            _model = model;
            _mediator = mediator;
        }

        public async UniTask<Nothing> Handle(IncrementCounterCommand command, CancellationToken ct)
        {
            var oldValue = _model.Value;
            _model.Increment(command.Amount);

            await _mediator.Publish(new CounterChangedNotification(oldValue, _model.Value), ct);

            return Nothing.Value;
        }
    }
}
