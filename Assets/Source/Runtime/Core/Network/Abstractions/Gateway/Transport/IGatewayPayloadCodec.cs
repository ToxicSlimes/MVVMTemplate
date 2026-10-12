using System;

namespace Core.Network.Abstractions.Gateway.Transport
{
    public interface IGatewayPayloadCodec
    {
        string Serialize(object value, Type type);
        object Deserialize(string payload, Type type);
    }
}
