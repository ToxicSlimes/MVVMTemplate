using UnityEngine;

namespace Core.Presentation.UI.DragDrop
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
