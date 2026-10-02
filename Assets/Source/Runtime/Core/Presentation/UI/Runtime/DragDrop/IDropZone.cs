using UnityEngine;

namespace Core.Presentation.UI.DragDrop
{
    public interface IDropZone
    {
        RectTransform RectTransform { get; }
        bool CanAccept(object dragData);
        void OnDragEnter(object dragData);
        void OnDragExit();
        void OnDrop(object dragData);
    }
}
