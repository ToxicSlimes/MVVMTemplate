using System;
using System.Threading;
using Core.Abstractions.Services;
using Cysharp.Threading.Tasks;

namespace Services
{
  public class DataPreloadService : IDataPreloadService
  {
    public UniTask PreloadDataAsync(IProgress<float> progress = null, CancellationToken cancellationToken = default)
    {
      return UniTask.CompletedTask;
    }
  }
}

