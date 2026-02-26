using System;
using Core.Infrastructure.Mediator.Abstractions;

namespace Core.Application.Notifications
{
    public readonly struct EmbeddedPanelStateChangedNotification : INotification
    {
        public readonly Type ViewModelType;
        public readonly bool IsVisible;

        public EmbeddedPanelStateChangedNotification(Type vmType, bool isVisible)
        {
            ViewModelType = vmType;
            IsVisible = isVisible;
        }
    }
}
