using Fusion;

namespace Core.Network.FusionAdapter.Runner
{
    /// <summary>
    /// Доступ к живому <see cref="NetworkRunner"/> для сервисов сетевого слоя.
    /// За пределы Core.Network.FusionAdapter не выходит.
    /// </summary>
    public interface INetworkRunnerAccessor
    {
        NetworkRunner Runner { get; }
        bool IsRunning { get; }
        void RegisterCallbacks(INetworkRunnerCallbacks callbacks);
    }
}
