using System;
using Core.Network.Abstractions.Gateway.Transport;
using Newtonsoft.Json;

namespace Core.Network.FusionAdapter.Gateway
{
    public sealed class JsonGatewayPayloadCodec : IGatewayPayloadCodec
    {
        private static readonly JsonSerializerSettings Settings = new()
        {
            TypeNameHandling = TypeNameHandling.None,
            MetadataPropertyHandling = MetadataPropertyHandling.Ignore,
            MaxDepth = 64
        };

        public string Serialize(object value, Type type)
        {
            if (type == null)
                throw new ArgumentNullException(nameof(type));
            if (value != null && type.IsInstanceOfType(value) == false)
                throw new ArgumentException($"Value is not assignable to {type.Name}", nameof(value));

            return JsonConvert.SerializeObject(value, type, Settings);
        }

        public object Deserialize(string payload, Type type)
        {
            if (payload == null)
                throw new ArgumentNullException(nameof(payload));
            if (type == null)
                throw new ArgumentNullException(nameof(type));

            return JsonConvert.DeserializeObject(payload, type, Settings);
        }
    }
}
