using Core.Abstractions.Services;
using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine.SceneManagement;

public class LoadSceneService : ILoadSceneService
{
  public async UniTask LoadAsync(SceneLoadPayload payload, CancellationToken cancellationToken = default)
  {
    switch (payload.LoadType)
    {
      case SceneLoadType.Regular:
        await LoadRegularAsync(payload, cancellationToken);
        break;

      case SceneLoadType.Networked:
        break;
      case SceneLoadType.None:
      default:
        throw new ArgumentOutOfRangeException(nameof(payload.LoadType), payload.LoadType, null);
    }

    payload.SceneLoaded?.Invoke();
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