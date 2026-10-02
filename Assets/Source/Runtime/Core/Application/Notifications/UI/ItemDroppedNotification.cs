using Core.Mediator.Abstractions;

namespace Core.Application.Notifications.UI
{
    public readonly struct ItemDroppedNotification : INotification
    {
        public readonly object SourceData;
        public readonly object TargetData;

        public ItemDroppedNotification(object sourceData, object targetData)
        {
            SourceData = sourceData;
            TargetData = targetData;
        }
    }
}
