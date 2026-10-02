using System;
using System.Collections.Generic;
using Core.Application.Services.Save.Migrations;
using Core.Infrastructure.Services.Save;
using Newtonsoft.Json.Linq;
using NUnit.Framework;

namespace Core.Tests.Infrastructure.Save
{
    public sealed class VersionedJsonSnapshotCodecTests
    {
        [Test]
        public void EncodeThenDecode_RoundTripsCurrentSnapshot()
        {
            var codec = CreateCodec(currentVersion: 2);
            var snapshot = new TestSnapshot { Name = "hero", Score = 42 };

            var encoded = codec.Encode(snapshot);
            var decoded = codec.Decode<TestSnapshot>(encoded.Value);

            Assert.That(encoded.IsSuccess, Is.True, encoded.Message);
            Assert.That(decoded.IsSuccess, Is.True, decoded.Message);
            Assert.That(decoded.Value.Name, Is.EqualTo("hero"));
            Assert.That(decoded.Value.Score, Is.EqualTo(42));
        }

        [Test]
        public void EncodeThenDecode_RoundTripsCollectionSnapshot()
        {
            var codec = CreateCodec(currentVersion: 1);
            var snapshot = new List<int> { 1, 2, 3 };

            var encoded = codec.Encode(snapshot);
            var decoded = codec.Decode<List<int>>(encoded.Value);

            Assert.That(encoded.IsSuccess, Is.True, encoded.Message);
            Assert.That(decoded.IsSuccess, Is.True, decoded.Message);
            Assert.That(decoded.Value, Is.EqualTo(snapshot));
        }

        [Test]
        public void Decode_MigratesOldEnvelopeBeforeDeserialization()
        {
            var migration = new RenamePointsMigration();
            var codec = CreateCodec(currentVersion: 2, migration);
            var oldCodec = CreateCodec(currentVersion: 1);
            var oldFile = oldCodec.Encode(new LegacySnapshot { Name = "hero", Points = 7 }).Value;

            var decoded = codec.Decode<TestSnapshot>(oldFile);

            Assert.That(decoded.IsSuccess, Is.True, decoded.Message);
            Assert.That(decoded.Value.Score, Is.EqualTo(7));
        }

        [Test]
        public void Decode_RejectsFutureSchemaVersion()
        {
            var codec = CreateCodec(currentVersion: 2);
            var futureCodec = CreateCodec(currentVersion: 3);
            var file = futureCodec.Encode(new TestSnapshot()).Value;

            var decoded = codec.Decode<TestSnapshot>(file);

            Assert.That(decoded.IsSuccess, Is.False);
            Assert.That(decoded.Message, Does.Contain("newer"));
        }

        [Test]
        public void Decode_RejectsInvalidContainerHeader()
        {
            var codec = CreateCodec(currentVersion: 1);

            var decoded = codec.Decode<TestSnapshot>(new byte[] { 1, 2, 3 });

            Assert.That(decoded.IsSuccess, Is.False);
            Assert.That(decoded.Message, Does.Contain("header"));
        }

        private static VersionedJsonSnapshotCodec CreateCodec(
            int currentVersion,
            params IJsonSnapshotMigration[] migrations)
        {
            return new VersionedJsonSnapshotCodec(
                new JsonSnapshotMigrationPipeline(migrations, currentVersion));
        }

        [Serializable]
        private sealed class TestSnapshot
        {
            public string Name;
            public int Score;
        }

        [Serializable]
        private sealed class LegacySnapshot
        {
            public string Name;
            public int Points;
        }

        private sealed class RenamePointsMigration : IJsonSnapshotMigration
        {
            public int FromVersion => 1;
            public int ToVersion => 2;

            public JObject Apply(JObject snapshot)
            {
                snapshot["score"] = snapshot["points"];
                snapshot.Remove("points");
                return snapshot;
            }
        }
    }
}
