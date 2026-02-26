using Core.Abstractions.MVVM.Views;

namespace Core.Abstractions.MVVM.ViewModels
{
    public interface IContextApplicable
    {
        void ApplyContext(IViewContext context);
    }
}
