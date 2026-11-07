using System;

namespace Core.Network.Abstractions.Services
{
    /// <summary>
    /// Сетевой тик симуляции. <see cref="Ticked"/> вызывается один раз на каждый forward-тик раннера,
    /// <see cref="DeltaTime"/> — длительность тика в секундах.
    /// </summary>
    public interface INetworkTickSource
    {
        float DeltaTime { get; }
        event Action Ticked;
    }
}
