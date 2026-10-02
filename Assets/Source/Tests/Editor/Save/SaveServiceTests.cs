using System.Threading;
using System.Threading.Tasks;
using Core.Abstractions.Results;
using Core.Abstractions.Services;
using Core.Abstractions.Services.Save;
using Core.Application.Services.Save;
using Cysharp.Threading.Tasks;
using NUnit.Framework;

namespace Core.Tests.Application.Save
{
    public sealed class SaveServiceTests
    {
        [Test]
        public async Task SaveThenLoad_RoundTripsThroughStoreAndCodec()
        {
            var store = new MemoryRawSaveStore();
            var service = new SaveService(store, new IntCodec(), new TestLogger());

            var saved = await service.SaveAsync("slot-1", 42);
            var loaded = await service.LoadAsync<int>("slot-1");

            Assert.That(saved.IsSuccess, Is.True, saved.Message);
            Assert.That(loaded.IsSuccess, Is.True, loaded.Message);
            Assert.That(loaded.Value, Is.EqualTo(42));
        }

        [Test]
        public async Task Load_WhenPrimaryCannotDecode_LoadsBackup()
        {
            var store = new MemoryRawSaveStore
            {
                Primary = new byte[] { 255 },
                Backup = new byte[] { 7 },
            };
            var logger = new TestLogger();
            var service = new SaveService(store, new IntCodec(), logger);

            var loaded = await service.LoadAsync<int>("slot-1");

            Assert.That(loaded.IsSuccess, Is.True, loaded.Message);
            Assert.That(loaded.Value, Is.EqualTo(7));
            Assert.That(logger.WarningCount, Is.EqualTo(1));
        }

        [Test]
        public async Task Load_WhenBothCopiesFail_ReturnsPrimaryFailure()
        {
            var store = new MemoryRawSaveStore
            {
                Primary = new byte[] { 255 },
                Backup = new byte[] { 255 },
            };
            var service = new SaveService(store, new IntCodec(), new TestLogger());

            var loaded = await service.LoadAsync<int>("slot-1");

            Assert.That(loaded.IsSuccess, Is.False);
            Assert.That(loaded.Message, Does.Contain("primary"));
        }

        private sealed class MemoryRawSaveStore : IRawSaveStore
        {
            public byte[] Primary;
            public byte[] Backup;

            public UniTask<bool> ExistsAsync(string pathKey, CancellationToken ct = default) =>
                UniTask.FromResult(Primary != null);

            public UniTask<OperationResult<RawSaveCopies>> LoadCopiesAsync(
                string pathKey,
                CancellationToken ct = default)
            {
                var primary = Primary == null
                    ? OperationResult<byte[]>.Failure("primary missing")
                    : OperationResult<byte[]>.Success(Primary);
                var backup = Backup == null
                    ? OperationResult<byte[]>.Failure("backup missing")
                    : OperationResult<byte[]>.Success(Backup);
                return UniTask.FromResult(OperationResult<RawSaveCopies>.Success(
                    new RawSaveCopies(primary, backup)));
            }

            public UniTask<OperationResult> SaveAsync(string pathKey, byte[] data, CancellationToken ct = default)
            {
                Backup = Primary;
                Primary = data;
                return UniTask.FromResult(OperationResult.Success());
            }

            public UniTask<OperationResult> DeleteAsync(string pathKey, CancellationToken ct = default)
            {
                Primary = null;
                Backup = null;
                return UniTask.FromResult(OperationResult.Success());
            }
        }

        private sealed class IntCodec : ISnapshotCodec
        {
            public OperationResult<byte[]> Encode<TSnapshot>(TSnapshot snapshot) =>
                snapshot is int value
                    ? OperationResult<byte[]>.Success(new[] { (byte)value })
                    : OperationResult<byte[]>.Failure("unsupported type");

            public OperationResult<TSnapshot> Decode<TSnapshot>(byte[] data)
            {
                if (data == null || data.Length != 1 || data[0] == 255 || typeof(TSnapshot) != typeof(int))
                    return OperationResult<TSnapshot>.Failure("decode failed");

                return OperationResult<TSnapshot>.Success((TSnapshot)(object)(int)data[0]);
            }
        }

        private sealed class TestLogger : ILogger
        {
            public LogLevel MinimumLevel { get; set; }
            public int WarningCount { get; private set; }
            public void Log(LogLevel level, string messageTemplate, params object[] args) { }
            public void Debug(string messageTemplate, params object[] args) { }
            public void Info(string messageTemplate, params object[] args) { }
            public void Warn(string messageTemplate, params object[] args) => WarningCount++;
            public void Error(string messageTemplate, params object[] args) { }
            public void Error(System.Exception exception, string messageTemplate = "", params object[] args) { }
        }
    }
}
