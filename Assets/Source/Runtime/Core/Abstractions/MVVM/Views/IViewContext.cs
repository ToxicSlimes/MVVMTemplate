namespace Core.Abstractions.MVVM.Views
{
    public interface IViewContext
    {
    }

    public readonly struct EmptyContext : IViewContext
    {
        public static readonly EmptyContext Instance = new();
    }
}
