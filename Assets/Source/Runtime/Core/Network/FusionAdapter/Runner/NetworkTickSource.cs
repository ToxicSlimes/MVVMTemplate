using System;
using Core.Network.Abstractions.Services;

namespace Core.Network.FusionAdapter.Runner
{
    public sealed class NetworkTickSource : INetworkTickSource
    {
        public float DeltaTime { get; private set; }

        public event Action Ticked;

        public void Raise(float deltaTime)
        {
            DeltaTime = deltaTime;
            Ticked?.Invoke();
        }
    }
}
