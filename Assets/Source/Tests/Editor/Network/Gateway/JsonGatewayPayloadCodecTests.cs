using System;
using Core.Network.FusionAdapter.Gateway;
using NUnit.Framework;

namespace Core.Tests.Network.Gateway
{
    public sealed class JsonGatewayPayloadCodecTests
    {
        [Test]
        public void SerializeThenDeserialize_RoundTripsDeclaredContract()
        {
            var codec = new JsonGatewayPayloadCodec();
            var source = new Payload("value", 7);

            var json = codec.Serialize(source, typeof(Payload));
            var result = (Payload)codec.Deserialize(json, typeof(Payload));

            Assert.That(result.Name, Is.EqualTo("value"));
            Assert.That(result.Count, Is.EqualTo(7));
            Assert.That(json, Does.Not.Contain("$type"));
        }

        [Test]
        public void Deserialize_DoesNotActivateTypeMetadata()
        {
            var codec = new JsonGatewayPayloadCodec();
            const string json = "{\"$type\":\"System.Version, System.Private.CoreLib\",\"name\":\"safe\",\"count\":1}";

            var result = (Payload)codec.Deserialize(json, typeof(Payload));

            Assert.That(result.Name, Is.EqualTo("safe"));
            Assert.That(result.Count, Is.EqualTo(1));
        }

        [Serializable]
        private sealed class Payload
        {
            public string Name { get; set; }
            public int Count { get; set; }

            public Payload()
            {
            }

            public Payload(string name, int count)
            {
                Name = name;
                Count = count;
            }
        }
    }
}
