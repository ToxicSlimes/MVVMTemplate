using System;
using System.Collections.Concurrent;
using System.IO;
using System.Threading;
using Core.Abstractions.Results;
using Core.Abstractions.Services.Save;
using Core.Infrastructure.Services.Save.Files;
using Cysharp.Threading.Tasks;
using ILogger = Core.Abstractions.Services.ILogger;

namespace Core.Infrastructure.Services.Save
{
    public sealed class FileRawSaveStore : IRawSaveStore
    {
        public const int MaxSaveFileSizeBytes = VersionedJsonSnapshotCodec.MaxSnapshotSizeBytes;

        private const string BackupSuffix = ".bak";
        private const string TempSuffix = ".tmp";

        private readonly string _rootPath;
        private readonly ILogger _logger;
        private readonly ConcurrentDictionary<string, SemaphoreSlim> _locks =
            new(StringComparer.OrdinalIgnoreCase);

        public FileRawSaveStore(ILogger logger)
            : this(UnityEngine.Application.persistentDataPath, logger)
        {
        }

        public FileRawSaveStore(string rootPath, ILogger logger)
        {
            if (string.IsNullOrWhiteSpace(rootPath))
                throw new ArgumentException("Save root path is null or empty", nameof(rootPath));

            _rootPath = Path.GetFullPath(rootPath);
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            Directory.CreateDirectory(_rootPath);
            if ((File.GetAttributes(_rootPath) & FileAttributes.ReparsePoint) != 0)
                throw new ArgumentException("Save root path cannot be a symbolic link or junction", nameof(rootPath));
        }

        public UniTask<bool> ExistsAsync(string pathKey, CancellationToken ct = default)
        {
            ct.ThrowIfCancellationRequested();
            return UniTask.FromResult(TryResolveSlot(pathKey, out var path) && File.Exists(path));
        }

        public async UniTask<OperationResult<RawSaveCopies>> LoadCopiesAsync(
            string pathKey,
            CancellationToken ct = default)
        {
            if (TryResolveSlot(pathKey, out var path) == false)
                return OperationResult<RawSaveCopies>.Failure($"Unsafe or reserved save path: {pathKey}");

            var gate = GetLock(path);
            await gate.WaitAsync(ct);

            try
            {
                var copies = await UniTask.RunOnThreadPool(
                    () =>
                    {
                        EnsurePathStillSafe(pathKey, path);
                        return new RawSaveCopies(
                            ReadCopy(EnsureDerivedPathSafe(path), pathKey),
                            ReadCopy(EnsureDerivedPathSafe(path + BackupSuffix), pathKey + BackupSuffix));
                    },
                    cancellationToken: ct);
                return OperationResult<RawSaveCopies>.Success(copies);
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception exception)
            {
                _logger.Error(exception, "Failed to read save copies for '{0}'", pathKey);
                return OperationResult<RawSaveCopies>.Failure(exception.Message);
            }
            finally
            {
                gate.Release();
            }
        }

        public async UniTask<OperationResult> SaveAsync(
            string pathKey,
            byte[] data,
            CancellationToken ct = default)
        {
            if (data == null)
                return OperationResult.Failure("Save data is null");

            if (data.Length > MaxSaveFileSizeBytes)
                return OperationResult.Failure(
                    $"Save data exceeds maximum size of {MaxSaveFileSizeBytes} bytes");

            if (TryResolveSlot(pathKey, out var path) == false)
                return OperationResult.Failure($"Unsafe or reserved save path: {pathKey}");

            var gate = GetLock(path);
            await gate.WaitAsync(ct);

            try
            {
                await UniTask.RunOnThreadPool(
                    () =>
                    {
                        EnsurePathStillSafe(pathKey, path);
                        WriteAtomic(
                            EnsureDerivedPathSafe(path),
                            EnsureDerivedPathSafe(path + TempSuffix),
                            EnsureDerivedPathSafe(path + BackupSuffix),
                            data);
                    },
                    cancellationToken: ct);
                return OperationResult.Success();
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception exception)
            {
                _logger.Error(exception, "Failed to save '{0}'", pathKey);
                return OperationResult.Failure(exception.Message);
            }
            finally
            {
                gate.Release();
            }
        }

        public async UniTask<OperationResult> DeleteAsync(
            string pathKey,
            CancellationToken ct = default)
        {
            if (TryResolveSlot(pathKey, out var path) == false)
                return OperationResult.Failure($"Unsafe or reserved save path: {pathKey}");

            var gate = GetLock(path);
            await gate.WaitAsync(ct);

            try
            {
                await UniTask.RunOnThreadPool(
                    () =>
                    {
                        EnsurePathStillSafe(pathKey, path);
                        DeleteCopies(
                            EnsureDerivedPathSafe(path),
                            EnsureDerivedPathSafe(path + BackupSuffix),
                            EnsureDerivedPathSafe(path + TempSuffix));
                    },
                    cancellationToken: ct);
                return OperationResult.Success();
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception exception)
            {
                _logger.Error(exception, "Failed to delete save '{0}'", pathKey);
                return OperationResult.Failure(exception.Message);
            }
            finally
            {
                gate.Release();
            }
        }

        private bool TryResolveSlot(string pathKey, out string path)
        {
            path = null;

            if (string.IsNullOrWhiteSpace(pathKey) ||
                HasUnsafeSegment(pathKey) ||
                HasReservedSegment(pathKey))
                return false;

            return FilePathUtility.TryResolve(_rootPath, pathKey, out path);
        }

        private static bool HasReservedSegment(string pathKey)
        {
            var segments = pathKey.Split(
                new[] { '/', '\\' },
                StringSplitOptions.RemoveEmptyEntries);

            for (var i = 0; i < segments.Length; i++)
            {
                var normalized = segments[i].TrimEnd(' ', '.');
                if (normalized.EndsWith(BackupSuffix, StringComparison.OrdinalIgnoreCase) ||
                    normalized.EndsWith(TempSuffix, StringComparison.OrdinalIgnoreCase))
                    return true;
            }

            return false;
        }

        private static bool HasUnsafeSegment(string pathKey)
        {
            var segments = pathKey.Split(new[] { '/', '\\' }, StringSplitOptions.RemoveEmptyEntries);
            var invalidCharacters = Path.GetInvalidFileNameChars();

            if (segments.Length == 0)
                return true;

            for (var i = 0; i < segments.Length; i++)
            {
                var segment = segments[i];
                if (segment is "." or ".." ||
                    segment.EndsWith(" ", StringComparison.Ordinal) ||
                    segment.EndsWith(".", StringComparison.Ordinal) ||
                    segment.IndexOf(':') >= 0 ||
                    segment.IndexOfAny(invalidCharacters) >= 0)
                    return true;
            }

            return false;
        }

        private void EnsurePathStillSafe(string pathKey, string expectedPath)
        {
            if (TryResolveSlot(pathKey, out var currentPath) == false ||
                string.Equals(currentPath, expectedPath, StringComparison.OrdinalIgnoreCase) == false)
                throw new IOException($"Save path became unsafe: {pathKey}");
        }

        private string EnsureDerivedPathSafe(string path)
        {
            if (FilePathUtility.IsContainedAndLinkFree(_rootPath, path) == false)
                throw new IOException($"Derived save path is unsafe: {path}");

            return path;
        }

        private SemaphoreSlim GetLock(string path) =>
            _locks.GetOrAdd(path, _ => new SemaphoreSlim(1, 1));

        private static OperationResult<byte[]> ReadCopy(string path, string displayPath)
        {
            try
            {
                if (File.Exists(path) == false)
                    return OperationResult<byte[]>.Failure($"Save file not found: {displayPath}");

                using var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read);
                if (stream.Length > MaxSaveFileSizeBytes)
                    return OperationResult<byte[]>.Failure(
                        $"Save file exceeds maximum size of {MaxSaveFileSizeBytes} bytes: {displayPath}");

                var bytes = new byte[(int)stream.Length];
                var offset = 0;
                while (offset < bytes.Length)
                {
                    var read = stream.Read(bytes, offset, bytes.Length - offset);
                    if (read == 0)
                        return OperationResult<byte[]>.Failure($"Unexpected end of save file: {displayPath}");
                    offset += read;
                }

                return OperationResult<byte[]>.Success(bytes);
            }
            catch (Exception exception)
            {
                return OperationResult<byte[]>.Failure(exception.Message);
            }
        }

        private static void WriteAtomic(string path, string tempPath, string backupPath, byte[] data)
        {
            var directory = Path.GetDirectoryName(path);
            if (string.IsNullOrEmpty(directory) == false)
                Directory.CreateDirectory(directory);

            try
            {
                File.WriteAllBytes(tempPath, data);

                if (File.Exists(path))
                    File.Replace(tempPath, path, backupPath);
                else
                    File.Move(tempPath, path);
            }
            finally
            {
                if (File.Exists(tempPath))
                    File.Delete(tempPath);
            }
        }

        private static void DeleteCopies(string path, string backupPath, string tempPath)
        {
            DeleteIfExists(path);
            DeleteIfExists(backupPath);
            DeleteIfExists(tempPath);
        }

        private static void DeleteIfExists(string path)
        {
            if (File.Exists(path))
                File.Delete(path);
        }
    }
}
