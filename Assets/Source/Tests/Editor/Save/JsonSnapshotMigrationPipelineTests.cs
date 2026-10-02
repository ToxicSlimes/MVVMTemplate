using System;
using System.Collections.Generic;
using Core.Application.Services.Save.Migrations;
using Newtonsoft.Json.Linq;
using NUnit.Framework;

namespace Core.Tests.Application.Save
{
    public sealed class JsonSnapshotMigrationPipelineTests
    {
        [Test]
        public void Migrate_AppliesEveryStepUntilCurrentVersion()
        {
            var calls = new List<int>();
            var pipeline = new JsonSnapshotMigrationPipeline(
                new IJsonSnapshotMigration[]
                {
                    new TestMigration(1, 2, calls),
                    new TestMigration(2, 3, calls),
                },
                currentVersion: 3);
            var snapshot = JObject.Parse("{\"score\":10}");

            var result = pipeline.Migrate(snapshot, sourceVersion: 1);

            Assert.That(result.IsSuccess, Is.True, result.Message);
            Assert.That(calls, Is.EqualTo(new[] { 1, 2 }));
            Assert.That((int)result.Value["versionReached"], Is.EqualTo(3));
        }

        [Test]
        public void Migrate_WhenLinkIsMissing_ReturnsFailureWithoutMutatingInput()
        {
            var pipeline = new JsonSnapshotMigrationPipeline(
                new IJsonSnapshotMigration[] { new TestMigration(1, 2, new List<int>()) },
                currentVersion: 3);
            var snapshot = JObject.Parse("{\"score\":10}");
            var original = snapshot.ToString();

            var result = pipeline.Migrate(snapshot, sourceVersion: 1);

            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.Message, Does.Contain("2"));
            Assert.That(snapshot.ToString(), Is.EqualTo(original));
        }

        [Test]
        public void Constructor_WhenSourceVersionIsDuplicated_Throws()
        {
            var migrations = new IJsonSnapshotMigration[]
            {
                new TestMigration(1, 2, new List<int>()),
                new TestMigration(1, 3, new List<int>()),
            };

            Assert.Throws<ArgumentException>(() =>
                new JsonSnapshotMigrationPipeline(migrations, currentVersion: 3));
        }

        [Test]
        public void Migrate_WhenSnapshotIsFromFuture_ReturnsFailure()
        {
            var pipeline = new JsonSnapshotMigrationPipeline(
                Array.Empty<IJsonSnapshotMigration>(),
                currentVersion: 3);

            var result = pipeline.Migrate(new JObject(), sourceVersion: 4);

            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.Message, Does.Contain("newer"));
        }

        private sealed class TestMigration : IJsonSnapshotMigration
        {
            private readonly List<int> _calls;

            public TestMigration(int fromVersion, int toVersion, List<int> calls)
            {
                FromVersion = fromVersion;
                ToVersion = toVersion;
                _calls = calls;
            }

            public int FromVersion { get; }
            public int ToVersion { get; }

            public JObject Apply(JObject snapshot)
            {
                _calls.Add(FromVersion);
                snapshot["versionReached"] = ToVersion;
                return snapshot;
            }
        }
    }
}
