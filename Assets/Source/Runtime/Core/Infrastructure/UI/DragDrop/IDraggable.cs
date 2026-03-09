using UnityEngine;

namespace Core.Infrastructure.UI.DragDrop
{
    public interface IDraggable
    {
        RectTransform RectTransform { get; }
        object DragData { get; }
        bool CanDrag { get; }
        void OnDragStarted();
        void OnDragEnded(bool success);
    }
}
