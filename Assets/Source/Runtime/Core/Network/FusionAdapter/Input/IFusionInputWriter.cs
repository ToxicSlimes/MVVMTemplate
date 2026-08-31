using System;
using Fusion;

namespace Core.Network.FusionAdapter.Input
{
    public interface IFusionInputWriter : IDisposable
    {
        Type InputType { get; }

        void Write(NetworkInput input);

        void Snapshot(NetworkRunner runner, PlayerRef player);

        bool TryWriteCached(NetworkInput input, PlayerRef player);

        void Forget(PlayerRef player);

        void ClearCache();
    }
}
