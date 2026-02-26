using Cysharp.Threading.Tasks;

namespace Core.Abstractions.Config
{
    public interface IConfigProvider
    {
        UniTask<T> GetConfigAsync<T>() where T : ConfigBase;
        T GetConfig<T>() where T : ConfigBase;
        bool IsConfigLoaded<T>() where T : ConfigBase;
        UniTask PreloadAllConfigsAsync();
        void ReleaseConfig<T>() where T : ConfigBase;
    }
}
