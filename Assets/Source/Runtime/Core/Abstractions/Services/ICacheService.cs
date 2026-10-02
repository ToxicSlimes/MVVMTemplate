using System;
using System.Threading;
using Core.Domain.Models.Cache;
using Cysharp.Threading.Tasks;

namespace Core.Abstractions.Services
{
    public interface ICacheService
    {
        UniTask<CacheResult<T>> GetAsync<T>(string key, CancellationToken ct = default);
        UniTask SetAsync<T>(string key, T data, TimeSpan? ttl = null, CancellationToken ct = default);
        UniTask RemoveAsync(string key);
        UniTask ClearAllAsync();
        UniTask CleanupAsync();
    }
}
