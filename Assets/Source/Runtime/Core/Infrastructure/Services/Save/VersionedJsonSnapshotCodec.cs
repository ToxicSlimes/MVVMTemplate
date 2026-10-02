using System;
using System.Text;
using Core.Abstractions.Results;
using Core.Abstractions.Services.Save;
using Core.Application.Services.Save.Migrations;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;

namespace Core.Infrastructure.Services.Save
{
    public sealed class VersionedJsonSnapshotCodec : ISnapshotCodec
    {
        public const int MaxSnapshotSizeBytes = 16 * 1024 * 1024;

        private const string SchemaVersionProperty = "schemaVersion";
        private const string SnapshotProperty = "snapshot";

        private static readonly UTF8Encoding StrictUtf8 = new(false, true);

        private readonly JsonSnapshotMigrationPipeline _migrations;
        private readonly JsonSerializer _serializer;

        public VersionedJsonSnapshotCodec(JsonSnapshotMigrationPipeline migrations)
        {
            _migrations = migrations ?? throw new ArgumentNullException(nameof(migrations));
            _serializer = JsonSerializer.Create(new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.None,
                Formatting = Formatting.None,
                ContractResolver = new CamelCasePropertyNamesContractResolver(),
            });
        }

        public OperationResult<byte[]> Encode<TSnapshot>(TSnapshot snapshot)
        {
            if (snapshot == null)
                return OperationResult<byte[]>.Failure("Snapshot is null");

            try
            {
                var envelope = new JObject
                {
                    [SchemaVersionProperty] = _migrations.CurrentVersion,
                    [SnapshotProperty] = JToken.FromObject(snapshot, _serializer),
                };
                var payload = StrictUtf8.GetBytes(envelope.ToString(Formatting.None));
                var header = new SaveHeader(SaveFlags.None).ToBytes();
                var file = new byte[header.Length + payload.Length];
                Buffer.BlockCopy(header, 0, file, 0, header.Length);
                Buffer.BlockCopy(payload, 0, file, header.Length, payload.Length);

                if (file.Length > MaxSnapshotSizeBytes)
                    return OperationResult<byte[]>.Failure(
                        $"Snapshot exceeds maximum size of {MaxSnapshotSizeBytes} bytes");

                return OperationResult<byte[]>.Success(file);
            }
            catch (Exception exception)
            {
                return OperationResult<byte[]>.Failure($"Snapshot serialization failed: {exception.Message}");
            }
        }

        public OperationResult<TSnapshot> Decode<TSnapshot>(byte[] data)
        {
            if (data != null && data.Length > MaxSnapshotSizeBytes)
                return OperationResult<TSnapshot>.Failure(
                    $"Snapshot exceeds maximum size of {MaxSnapshotSizeBytes} bytes");

            if (SaveHeader.TryParse(data, out var header) == false)
                return OperationResult<TSnapshot>.Failure("Invalid or unsupported save header");

            if (header.Flags != SaveFlags.None)
                return OperationResult<TSnapshot>.Failure($"Unsupported save flags: {header.Flags}");

            try
            {
                var json = StrictUtf8.GetString(data, SaveHeader.HeaderSize, data.Length - SaveHeader.HeaderSize);
                var envelope = JObject.Parse(json);

                if (envelope[SchemaVersionProperty]?.Type != JTokenType.Integer)
                    return OperationResult<TSnapshot>.Failure("Snapshot schemaVersion is missing or invalid");

                var snapshot = envelope[SnapshotProperty];
                if (snapshot == null || snapshot.Type == JTokenType.Null)
                    return OperationResult<TSnapshot>.Failure("Snapshot payload is missing or invalid");

                var schemaVersion = envelope[SchemaVersionProperty].Value<int>();
                JToken currentSnapshot = snapshot;

                if (schemaVersion != _migrations.CurrentVersion)
                {
                    if (snapshot is not JObject objectSnapshot)
                        return OperationResult<TSnapshot>.Failure(
                            "Only object snapshots can be migrated between schema versions");

                    var migrated = _migrations.Migrate(objectSnapshot, schemaVersion);
                    if (migrated.IsSuccess == false)
                        return OperationResult<TSnapshot>.Failure(migrated.Message);

                    currentSnapshot = migrated.Value;
                }

                var value = currentSnapshot.ToObject<TSnapshot>(_serializer);
                return value == null
                    ? OperationResult<TSnapshot>.Failure($"Snapshot deserialized to null ({typeof(TSnapshot).Name})")
                    : OperationResult<TSnapshot>.Success(value);
            }
            catch (Exception exception)
            {
                return OperationResult<TSnapshot>.Failure($"Snapshot deserialization failed: {exception.Message}");
            }
        }
    }
}
