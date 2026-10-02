using System;
using System.Threading;
using Core.Abstractions.Results;
using Core.Abstractions.Services;
using Core.Abstractions.Services.Save;
using Cysharp.Threading.Tasks;

namespace Core.Application.Services.Save
{
    public sealed class SaveService : ISaveService
    {
        private readonly IRawSaveStore _store;
        private readonly ISnapshotCodec _codec;
        private readonly ILogger _logger;

        public SaveService(IRawSaveStore store, ISnapshotCodec codec, ILogger logger)
        {
            _store = store ?? throw new ArgumentNullException(nameof(store));
            _codec = codec ?? throw new ArgumentNullException(nameof(codec));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public UniTask<bool> ExistsAsync(string slot, CancellationToken ct = default)
        {
            return IsSlotValid(slot)
                ? _store.ExistsAsync(slot, ct)
                : UniTask.FromResult(false);
        }

        public async UniTask<OperationResult> SaveAsync<TSnapshot>(
            string slot,
            TSnapshot snapshot,
            CancellationToken ct = default)
        {
            if (IsSlotValid(slot) == false)
                return OperationResult.Failure("Save slot is null or empty");

            var encoded = _codec.Encode(snapshot);
            return encoded.IsSuccess
                ? await _store.SaveAsync(slot, encoded.Value, ct)
                : OperationResult.Failure(encoded.Message);
        }

        public async UniTask<OperationResult<TSnapshot>> LoadAsync<TSnapshot>(
            string slot,
            CancellationToken ct = default)
        {
            if (IsSlotValid(slot) == false)
                return OperationResult<TSnapshot>.Failure("Save slot is null or empty");

            var copies = await _store.LoadCopiesAsync(slot, ct);
            if (copies.IsSuccess == false)
                return OperationResult<TSnapshot>.Failure(copies.Message);

            var primary = copies.Value.Primary.IsSuccess
                ? _codec.Decode<TSnapshot>(copies.Value.Primary.Value)
                : OperationResult<TSnapshot>.Failure(copies.Value.Primary.Message);

            if (primary.IsSuccess)
                return primary;

            var backup = copies.Value.Backup.IsSuccess
                ? _codec.Decode<TSnapshot>(copies.Value.Backup.Value)
                : OperationResult<TSnapshot>.Failure(copies.Value.Backup.Message);

            if (backup.IsSuccess)
            {
                _logger.Warn("Primary save '{0}' failed ({1}); loaded backup", slot, primary.Message);
                return backup;
            }

            return OperationResult<TSnapshot>.Failure(
                $"Failed to load primary save ({primary.Message}); backup also failed ({backup.Message})");
        }

        public UniTask<OperationResult> DeleteAsync(string slot, CancellationToken ct = default)
        {
            return IsSlotValid(slot)
                ? _store.DeleteAsync(slot, ct)
                : UniTask.FromResult(OperationResult.Failure("Save slot is null or empty"));
        }

        private static bool IsSlotValid(string slot) => string.IsNullOrWhiteSpace(slot) == false;
    }
}
