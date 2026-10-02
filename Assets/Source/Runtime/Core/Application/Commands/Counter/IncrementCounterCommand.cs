using Core.Mediator.Abstractions;

namespace Core.Application.Commands.Counter
{
    /// <summary>
    /// TEMPLATE EXAMPLE: A simple command that increments the counter by a given amount.
    /// Commands represent "do something" intentions — fire-and-forget with no return value.
    /// </summary>
    public readonly struct IncrementCounterCommand : IRequest<Nothing>
    {
        public readonly int Amount;

        public IncrementCounterCommand(int amount = 1)
        {
            Amount = amount;
        }
    }
}
