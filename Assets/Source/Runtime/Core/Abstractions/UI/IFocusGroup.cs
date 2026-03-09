namespace Core.Abstractions.UI
{
    public interface IFocusGroup
    {
        void SelectInitial();
        void SelectNext();
        void SelectPrevious();
        void Refresh();
    }
}
