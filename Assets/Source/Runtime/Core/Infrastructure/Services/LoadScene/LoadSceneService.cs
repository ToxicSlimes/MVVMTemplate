using Core.Abstractions.Services;
using Core.Network.Abstractions.Services;
using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine.SceneManagement;

public class LoadSceneService : ILoadSceneService
{
  private readonly INetworkRunnerController _networkRunner;

  public LoadSceneService(INetworkRunnerController networkRunner)
  {
    _networkRunner = networkRunner;
  }

  public async UniTask LoadAsync(SceneLoadPayload payload, CancellationToken cancellationToken = default)
  {
    switch (payload.LoadType)
    {
      case SceneLoadType.Regular:
        await LoadRegularAsync(payload, cancellationToken);
        break;

      case SceneLoadType.Networked:
        await LoadNetworkedAsync(payload, cancellationToken);
        break;
      case SceneLoadType.None:
      default:
        throw new ArgumentOutOfRangeException(nameof(payload.LoadType), payload.LoadType, null);
    }

    payload.SceneLoaded?.Invoke();
  }

  private async UniTask LoadNetworkedAsync(SceneLoadPayload payload, CancellationToken cancellationToken)
  {
    if (_networkRunner == null)
    {
      throw new InvalidOperationException(
        $"{nameof(SceneLoadType.Networked)} недоступен: {nameof(INetworkRunnerController)} не зарегистрирован в контейнере");
    }

    if (payload.SceneName == null)
    {
      throw new ArgumentException(
        "Сетевая загрузка требует имени или пути сцены, индекс не поддерживается", nameof(payload));
    }

    await _networkRunner.LoadNetworkSceneAsync(payload.SceneName, cancellationToken);
  }

  private static async UniTask LoadRegularAsync(SceneLoadPayload payload, CancellationToken cancellationToken)
  {
    var asyncOperation = payload.SceneName != null
      ? SceneManager.LoadSceneAsync(payload.SceneName, payload.Mode)
      : SceneManager.LoadSceneAsync(payload.SceneIndex, payload.Mode);

    if (payload.ActivationAwaiter != null)
    {
      if (asyncOperation != null)
      {
        asyncOperation.allowSceneActivation = false;
        await payload.ActivationAwaiter(cancellationToken);
        if (cancellationToken.IsCancellationRequested)
          return;
        asyncOperation.allowSceneActivation = true;
      }
    }

    await asyncOperation.WithCancellation(cancellationToken);
  }
}