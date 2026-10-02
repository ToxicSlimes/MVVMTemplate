using Core.Mediator.Abstractions;

namespace Core.Application.Notifications.Counter
{
    /// <summary>
    /// TEMPLATE EXAMPLE: A notification published when the counter value changes.
    /// Notifications are pub/sub — multiple handlers can subscribe to the same event.
    /// Unlike Commands/Queries, notifications don't return results.
    /// </summary>
    public readonly struct CounterChangedNotification : INotification
    {
        public readonly int OldValue;
        public readonly int NewValue;

        public CounterChangedNotification(int oldValue, int newValue)
        {
            OldValue = oldValue;
            NewValue = newValue;
        }
    }
}
