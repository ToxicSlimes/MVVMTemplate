using System;
using System.Threading;
using Cysharp.Threading.Tasks;

namespace Core.Abstractions.Services
{
    public interface IDataPreloadService
    {
        UniTask PreloadDataAsync(IProgress<float> progress = null, CancellationToken cancellationToken = default);
    }
}
