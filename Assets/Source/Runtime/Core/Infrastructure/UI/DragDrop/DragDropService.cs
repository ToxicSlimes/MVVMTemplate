using System;
using System.Collections.Generic;
using Core.Abstractions.UI.DragDrop;
using R3;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Core.Infrastructure.UI.DragDrop
{
    public class DragDropService : IDragDropService, IDisposable
    {
        private readonly ReactiveProperty<bool> _isDragging = new(false);
        private readonly Subject<DragStartedEvent> _dragStarted = new();
        private readonly Subject<DragEndedEvent> _dragEnded = new();
        private readonly Subject<DropEvent> _dropCompleted = new();
        private readonly List<IDropZone> _dropZones = new();
        private readonly List<RaycastResult> _raycastResults = new();

        public ReadOnlyReactiveProperty<bool> IsDragging => _isDragging;
        public IDraggable CurrentDraggable { get; private set; }
        public object CurrentDragData { get; private set; }

        public Observable<DragStartedEvent> OnDragStarted => _dragStarted;
        public Observable<DragEndedEvent> OnDragEnded => _dragEnded;
        public Observable<DropEvent> OnDropCompleted => _dropCompleted;

        public void BeginDrag(IDraggable source)
        {
            if (source == null) return;

            CurrentDraggable = source;
            CurrentDragData = source.DragData;
            _isDragging.Value = true;

            source.OnDragStarted();
            _dragStarted.OnNext(new DragStartedEvent(source, CurrentDragData));
        }

        public void EndDrag(IDropZone target)
        {
            if (CurrentDraggable == null) return;

            var source = CurrentDraggable;
            var data = CurrentDragData;
            var success = target != null && target.CanAccept(data);

            if (success)
            {
                target.OnDrop(data);
                _dropCompleted.OnNext(new DropEvent(source, target, data));
            }

            source.OnDragEnded(success);
            _dragEnded.OnNext(new DragEndedEvent(source, target, success));

            CurrentDraggable = null;
            CurrentDragData = null;
            _isDragging.Value = false;
        }

        public void CancelDrag()
        {
            EndDrag(null);
        }

        public void RegisterDropZone(IDropZone zone)
        {
            if (zone != null && !_dropZones.Contains(zone))
                _dropZones.Add(zone);
        }

        public void UnregisterDropZone(IDropZone zone)
        {
            _dropZones.Remove(zone);
        }

        public IDropZone FindDropZoneAt(Vector2 screenPosition)
        {
            var eventSystem = EventSystem.current;
            if (eventSystem == null) return null;

            var pointerData = new PointerEventData(eventSystem) { position = screenPosition };
            _raycastResults.Clear();
            eventSystem.RaycastAll(pointerData, _raycastResults);

            for (var i = 0; i < _raycastResults.Count; i++)
            {
                var zone = _raycastResults[i].gameObject.GetComponent<IDropZone>();
                if (zone != null && zone.CanAccept(CurrentDragData))
                    return zone;
            }

            return null;
        }

        public void SetupDraggable(GameObject target, Func<object> dataProvider, Func<bool> canDragProvider = null)
        {
            if (target == null) return;
            var draggable = target.GetComponent<DraggableBehaviour>() ?? target.AddComponent<DraggableBehaviour>();
            draggable.Initialize(this, dataProvider, canDragProvider);
        }

        public void SetupDropZone(GameObject target, Func<object, bool> acceptFilter = null)
        {
            SetupDropZone(target, null, acceptFilter);
        }

        public void SetupDropZone(GameObject target, Action<object> onDrop, Func<object, bool> acceptFilter = null)
        {
            if (target == null) return;
            var zone = target.GetComponent<DropZoneBehaviour>() ?? target.AddComponent<DropZoneBehaviour>();
            zone.Initialize(this, acceptFilter);
            if (onDrop != null)
                zone.OnItemDropped.AddListener(data => onDrop(data));
            RegisterDropZone(zone);
        }

        public void Dispose()
        {
            if (_isDragging.Value)
                CancelDrag();

            _isDragging.Dispose();
            _dragStarted.Dispose();
            _dragEnded.Dispose();
            _dropCompleted.Dispose();
            _dropZones.Clear();
        }
    }
}
