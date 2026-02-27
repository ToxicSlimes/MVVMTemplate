using System.Threading;
using Core.Abstractions.MVVM.Models.Counter;
using Core.Application.Commands.Counter;
using Core.Application.Notifications.Counter;
using Core.Infrastructure.Mediator.Abstractions;
using Core.Infrastructure.Mediator.Handlers;
using Cysharp.Threading.Tasks;

namespace Core.Application.Handlers.Counter
{
    public sealed class ResetCounterHandler : IRequestHandler<ResetCounterCommand, Nothing>
    {
        private readonly ICounterModel _model;
        private readonly IMediator _mediator;

        public ResetCounterHandler(ICounterModel model, IMediator mediator)
        {
            _model = model;
            _mediator = mediator;
        }

        public async UniTask<Nothing> Handle(ResetCounterCommand command, CancellationToken ct)
        {
            var oldValue = _model.Value;
            _model.Reset();

            await _mediator.Publish(new CounterChangedNotification(oldValue, 0), ct);

            return Nothing.Value;
        }
    }
}
