using Core.Infrastructure.Mediator.Abstractions;

namespace Core.Application.Queries.Counter
{
    /// <summary>
    /// TEMPLATE EXAMPLE: A query that returns the current counter value.
    /// Queries represent "give me data" requests — they return a result.
    /// </summary>
    public readonly struct GetCounterQuery : IRequest<CounterState> { }

    public readonly struct CounterState
    {
        public readonly int Value;
        public readonly int TotalIncrements;

        public CounterState(int value, int totalIncrements)
        {
            Value = value;
            TotalIncrements = totalIncrements;
        }
    }
}
