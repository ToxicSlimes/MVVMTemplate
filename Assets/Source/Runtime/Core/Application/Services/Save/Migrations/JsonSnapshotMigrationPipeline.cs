using System;
using System.Collections.Generic;
using Core.Abstractions.Results;
using Newtonsoft.Json.Linq;

namespace Core.Application.Services.Save.Migrations
{
    public sealed class JsonSnapshotMigrationPipeline
    {
        private readonly Dictionary<int, IJsonSnapshotMigration> _bySourceVersion;

        public JsonSnapshotMigrationPipeline(
            IReadOnlyList<IJsonSnapshotMigration> migrations,
            int currentVersion)
        {
            if (currentVersion < 1)
                throw new ArgumentOutOfRangeException(nameof(currentVersion));

            CurrentVersion = currentVersion;
            _bySourceVersion = new Dictionary<int, IJsonSnapshotMigration>();

            if (migrations == null)
                return;

            for (var i = 0; i < migrations.Count; i++)
            {
                var migration = migrations[i] ?? throw new ArgumentException("Migration cannot be null", nameof(migrations));

                if (migration.FromVersion < 1 || migration.ToVersion <= migration.FromVersion)
                    throw new ArgumentException(
                        $"Invalid migration {migration.FromVersion} -> {migration.ToVersion}",
                        nameof(migrations));

                if (_bySourceVersion.TryAdd(migration.FromVersion, migration) == false)
                    throw new ArgumentException(
                        $"Duplicate migration from schema version {migration.FromVersion}",
                        nameof(migrations));
            }
        }

        public int CurrentVersion { get; }

        public OperationResult<JObject> Migrate(JObject snapshot, int sourceVersion)
        {
            if (snapshot == null)
                return OperationResult<JObject>.Failure("Snapshot is null");

            if (sourceVersion < 1)
                return OperationResult<JObject>.Failure($"Invalid snapshot schema version {sourceVersion}");

            if (sourceVersion > CurrentVersion)
                return OperationResult<JObject>.Failure(
                    $"Snapshot schema version {sourceVersion} is newer than supported version {CurrentVersion}");

            var migrated = (JObject)snapshot.DeepClone();
            var version = sourceVersion;

            try
            {
                while (version < CurrentVersion)
                {
                    if (_bySourceVersion.TryGetValue(version, out var migration) == false)
                        return OperationResult<JObject>.Failure(
                            $"Missing migration from schema version {version}");

                    migrated = migration.Apply(migrated);

                    if (migrated == null)
                        return OperationResult<JObject>.Failure(
                            $"Migration {migration.FromVersion} -> {migration.ToVersion} returned null");

                    version = migration.ToVersion;
                }

                if (version != CurrentVersion)
                    return OperationResult<JObject>.Failure(
                        $"Migration chain ended at schema version {version}, expected {CurrentVersion}");

                return OperationResult<JObject>.Success(migrated);
            }
            catch (Exception exception)
            {
                return OperationResult<JObject>.Failure(
                    $"Snapshot migration from schema version {version} failed: {exception.Message}");
            }
        }
    }
}
