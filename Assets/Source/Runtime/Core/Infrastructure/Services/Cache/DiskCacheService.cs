using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using Core.Abstractions.Services;
using Core.Data.Domain.Models.Cache;
using Cysharp.Threading.Tasks;
using Newtonsoft.Json;
using UnityEngine;
using ILogger = Core.Abstractions.Services.ILogger;

namespace Core.Infrastructure.Services.Cache
{
    public class DiskCacheService : ICacheService
    {
        private readonly ILogger _logger;
        private readonly CacheConfig _config;
        private readonly string _cacheDirectory;
        private readonly byte[] _encryptionKey;

        private const string CacheFileExtension = ".cache";
        private const string EncryptionSalt = "EndlessAdventure_Cache_Salt_v1";

        public DiskCacheService(ILogger logger, CacheConfig config, string cacheDirectory = null)
        {
            _logger = logger;
            _config = config;
            _cacheDirectory = cacheDirectory ?? Path.Combine(UnityEngine.Application.persistentDataPath, "Cache");
            _encryptionKey = DeriveEncryptionKey();

            EnsureCacheDirectoryExists();
        }

        public UniTask<CacheResult<T>> GetAsync<T>(string key, CancellationToken ct = default)
        {
            try
            {
                var filePath = GetFilePath(key);

                if (!File.Exists(filePath))
                    return UniTask.FromResult(CacheResult<T>.Miss());

                var json = File.ReadAllText(filePath, Encoding.UTF8);
                var entry = JsonConvert.DeserializeObject<CacheEntry>(json);

                if (entry == null)
                    return UniTask.FromResult(CacheResult<T>.Miss());

                if (entry.SchemaVersion != _config.SchemaVersion)
                {
                    _logger.Debug("[Cache] MISS {0} (schema mismatch: {1} != {2})", key, entry.SchemaVersion, _config.SchemaVersion);
                    return UniTask.FromResult(CacheResult<T>.Miss());
                }

                if (entry.ExpiresAtTicks < DateTime.UtcNow.Ticks)
                {
                    _logger.Debug("[Cache] MISS {0} (expired)", key);
                    return UniTask.FromResult(CacheResult<T>.Miss());
                }

                var decryptedJson = Decrypt(entry.EncryptedData, entry.IV);
                var data = JsonConvert.DeserializeObject<T>(decryptedJson);

                _logger.Debug("[Cache] HIT {0}", key);
                return UniTask.FromResult(CacheResult<T>.Hit(data, entry.ContentHash));
            }
            catch (Exception ex)
            {
                _logger.Warn("[Cache] GET error {0}: {1}", key, ex.Message);
                return UniTask.FromResult(CacheResult<T>.Miss());
            }
        }

        public UniTask SetAsync<T>(string key, T data, TimeSpan? ttl = null, CancellationToken ct = default)
        {
            try
            {
                var effectiveTtl = ttl ?? _config.DefaultTtl;
                var dataJson = JsonConvert.SerializeObject(data);
                var contentHash = ComputeHash(dataJson);
                var (encryptedBase64, ivBase64) = Encrypt(dataJson);

                var now = DateTime.UtcNow;
                var entry = new CacheEntry
                {
                    ContentHash = contentHash,
                    EncryptedData = encryptedBase64,
                    IV = ivBase64,
                    CreatedAtTicks = now.Ticks,
                    ExpiresAtTicks = (now + effectiveTtl).Ticks,
                    SchemaVersion = _config.SchemaVersion
                };

                var entryJson = JsonConvert.SerializeObject(entry);
                var filePath = GetFilePath(key);

                EnsureCacheDirectoryExists();
                File.WriteAllText(filePath, entryJson, Encoding.UTF8);

                _logger.Debug("[Cache] SET {0} ttl={1}m", key, effectiveTtl.TotalMinutes);
            }
            catch (Exception ex)
            {
                _logger.Warn("[Cache] SET error {0}: {1}", key, ex.Message);
            }

            return UniTask.CompletedTask;
        }

        public UniTask RemoveAsync(string key)
        {
            try
            {
                var filePath = GetFilePath(key);
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                    _logger.Debug("[Cache] REMOVE {0}", key);
                }
            }
            catch (Exception ex)
            {
                _logger.Warn("[Cache] REMOVE error {0}: {1}", key, ex.Message);
            }

            return UniTask.CompletedTask;
        }

        public UniTask ClearAllAsync()
        {
            try
            {
                if (!Directory.Exists(_cacheDirectory))
                    return UniTask.CompletedTask;

                var files = Directory.GetFiles(_cacheDirectory, "*" + CacheFileExtension);
                foreach (var file in files)
                {
                    try
                    {
                        File.Delete(file);
                    }
                    catch (Exception ex)
                    {
                        _logger.Warn("[Cache] ClearAll delete error: {0}", ex.Message);
                    }
                }

                _logger.Info("[Cache] Cleared all ({0} files)", files.Length);
            }
            catch (Exception ex)
            {
                _logger.Warn("[Cache] ClearAll error: {0}", ex.Message);
            }

            return UniTask.CompletedTask;
        }

        public UniTask CleanupAsync()
        {
            try
            {
                if (!Directory.Exists(_cacheDirectory))
                    return UniTask.CompletedTask;

                var files = Directory.GetFiles(_cacheDirectory, "*" + CacheFileExtension);
                var removedCount = 0;
                var maxAge = TimeSpan.FromDays(_config.MaxAgeDays);

                foreach (var file in files)
                {
                    try
                    {
                        var shouldRemove = false;
                        var fileAge = DateTime.UtcNow - File.GetLastWriteTimeUtc(file);

                        if (fileAge > maxAge)
                        {
                            shouldRemove = true;
                        }
                        else
                        {
                            var json = File.ReadAllText(file, Encoding.UTF8);
                            var entry = JsonConvert.DeserializeObject<CacheEntry>(json);
                            if (entry != null && entry.SchemaVersion != _config.SchemaVersion)
                                shouldRemove = true;
                        }

                        if (shouldRemove)
                        {
                            File.Delete(file);
                            removedCount++;
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.Warn("[Cache] Cleanup file error: {0}", ex.Message);
                    }
                }

                if (removedCount > 0)
                    _logger.Info("[Cache] Cleanup removed {0} files", removedCount);
            }
            catch (Exception ex)
            {
                _logger.Warn("[Cache] Cleanup error: {0}", ex.Message);
            }

            return UniTask.CompletedTask;
        }

        private string GetFilePath(string key)
        {
            var keyHash = ComputeHash(key);
            return Path.Combine(_cacheDirectory, keyHash + CacheFileExtension);
        }

        private void EnsureCacheDirectoryExists()
        {
            if (!Directory.Exists(_cacheDirectory))
                Directory.CreateDirectory(_cacheDirectory);
        }

        private (string encryptedBase64, string ivBase64) Encrypt(string plainText)
        {
            using (var aes = Aes.Create())
            {
                aes.Key = _encryptionKey;
                aes.Mode = CipherMode.CBC;
                aes.Padding = PaddingMode.PKCS7;
                aes.GenerateIV();

                using (var encryptor = aes.CreateEncryptor())
                {
                    var plainBytes = Encoding.UTF8.GetBytes(plainText);
                    var encryptedBytes = encryptor.TransformFinalBlock(plainBytes, 0, plainBytes.Length);

                    return (Convert.ToBase64String(encryptedBytes), Convert.ToBase64String(aes.IV));
                }
            }
        }

        private string Decrypt(string encryptedBase64, string ivBase64)
        {
            using (var aes = Aes.Create())
            {
                aes.Key = _encryptionKey;
                aes.IV = Convert.FromBase64String(ivBase64);
                aes.Mode = CipherMode.CBC;
                aes.Padding = PaddingMode.PKCS7;

                using (var decryptor = aes.CreateDecryptor())
                {
                    var encryptedBytes = Convert.FromBase64String(encryptedBase64);
                    var plainBytes = decryptor.TransformFinalBlock(encryptedBytes, 0, encryptedBytes.Length);

                    return Encoding.UTF8.GetString(plainBytes);
                }
            }
        }

        private static string ComputeHash(string input)
        {
            using (var sha256 = SHA256.Create())
            {
                var bytes = Encoding.UTF8.GetBytes(input);
                var hashBytes = sha256.ComputeHash(bytes);
                var sb = new StringBuilder(hashBytes.Length * 2);
                foreach (var b in hashBytes)
                    sb.Append(b.ToString("x2"));
                return sb.ToString();
            }
        }

        private static byte[] DeriveEncryptionKey()
        {
            var source = SystemInfo.deviceUniqueIdentifier + EncryptionSalt;
            using (var sha256 = SHA256.Create())
            {
                return sha256.ComputeHash(Encoding.UTF8.GetBytes(source));
            }
        }
    }
}
