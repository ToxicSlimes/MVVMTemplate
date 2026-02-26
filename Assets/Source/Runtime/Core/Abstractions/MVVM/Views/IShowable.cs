using Cysharp.Threading.Tasks;

namespace Core.Abstractions.MVVM.Views
{
    public interface IShowable
    {
        UniTask ShowAsync();
    }
}