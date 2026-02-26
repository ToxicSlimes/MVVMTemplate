namespace Core.Abstractions.MVVM.Elements
{
    public interface ISelectableElement<TData> : IClickableElement<TData>
        where TData : struct
    {
        bool IsSelected { get; }
        void SetSelected(bool selected);
    }
}
