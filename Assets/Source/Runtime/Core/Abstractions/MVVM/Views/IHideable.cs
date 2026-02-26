using Cysharp.Threading.Tasks;

namespace Core.Abstractions.MVVM.Views
{
    public interface IHideable
    {
        UniTask HideAsync();
    }
}