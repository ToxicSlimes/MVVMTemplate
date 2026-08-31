using System;
using Core.Network.Abstractions.Messaging;
using Core.Network.Abstractions.Types;

namespace Core.Network.Abstractions.Services
{
    public interface INetworkMessenger
    {
        void Send<TMessage>(TMessage message, NetworkTarget target) where TMessage : INetworkMessage;

        IDisposable Subscribe<TMessage>(Action<NetworkPlayerId, TMessage> handler) where TMessage : INetworkMessage;
    }
}
