using System;
using UnityEngine;

namespace Core.Abstractions.UI.DragDrop
{
    public interface IDragDropService
    {
        void SetupDraggable(GameObject target, Func<object> dataProvider, Func<bool> canDragProvider = null);
        void SetupDropZone(GameObject target, Func<object, bool> acceptFilter = null);
        void SetupDropZone(GameObject target, Action<object> onDrop, Func<object, bool> acceptFilter = null);
    }
}
