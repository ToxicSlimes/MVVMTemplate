using System;
using System.Threading;
using Core.Abstractions.Services;
using Cysharp.Threading.Tasks;

namespace Services
{
  public class DataPreloadService : IDataPreloadService
  {
    private readonly ILocalizationService _localizationService;

    public DataPreloadService(ILocalizationService localizationService)
    {
      _localizationService = localizationService;
    }

    public async UniTask PreloadDataAsync(IProgress<float> progress = null, CancellationToken cancellationToken = default)
    {
      await _localizationService.InitializeAsync(cancellationToken);
    }
  }
}

