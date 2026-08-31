namespace Core.Network.Abstractions.Services
{
    /// <summary>
    /// Агрегирующий фасад сетевого слоя. Потребителям, которым нужна часть API,
    /// следует запрашивать соответствующий сегрегированный интерфейс.
    /// </summary>
    public interface INetworkService :
        INetworkRunnerController,
        INetworkSpawner,
        INetworkMessenger,
        INetworkInputSource
    {
    }
}
