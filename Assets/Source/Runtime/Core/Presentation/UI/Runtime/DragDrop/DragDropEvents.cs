namespace Core.Presentation.UI.DragDrop
{
    public readonly struct DragStartedEvent
    {
        public readonly IDraggable Source;
        public readonly object Data;

        public DragStartedEvent(IDraggable source, object data)
        {
            Source = source;
            Data = data;
        }
    }
    public readonly struct DragEndedEvent
    {
        public readonly IDraggable Source;
        public readonly IDropZone Target;
        public readonly bool Success;

        public DragEndedEvent(IDraggable source, IDropZone target, bool success)
        {
            Source = source;
            Target = target;
            Success = success;
        }
    }
    public readonly struct DropEvent
    {
        public readonly IDraggable Source;
        public readonly IDropZone Target;
        public readonly object Data;

        public DropEvent(IDraggable source, IDropZone target, object data)
        {
            Source = source;
            Target = target;
            Data = data;
        }
    }
}
