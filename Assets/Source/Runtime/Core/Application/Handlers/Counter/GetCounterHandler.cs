using System.Threading;
using Core.Abstractions.MVVM.Models.Counter;
using Core.Application.Queries.Counter;
using Core.Mediator.Handlers;
using Cysharp.Threading.Tasks;

namespace Core.Application.Handlers.Counter
{
    /// <summary>
    /// TEMPLATE EXAMPLE: Query handler — returns data without side effects.
    /// </summary>
    public sealed class GetCounterHandler : IRequestHandler<GetCounterQuery, CounterState>
    {
        private readonly ICounterModel _model;

        public GetCounterHandler(ICounterModel model)
        {
            _model = model;
        }

        public UniTask<CounterState> Handle(GetCounterQuery query, CancellationToken ct)
        {
            return UniTask.FromResult(new CounterState(_model.Value, _model.TotalIncrements));
        }
    }
}
