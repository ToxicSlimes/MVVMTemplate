using System;
using System.Collections.Generic;
using Core.Abstractions.StateMachines;
using Core.Network.Abstractions.Gateway.Transport;
using Core.DependencyInjection.Bindings;
using Core.Infrastructure.Services.Config;
using Cysharp.Threading.Tasks;
using Core.DependencyInjection;
using Reflex.Core;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ProjectInstaller : MonoBehaviour, IInstaller
{
  [SerializeField] private bool _networkEnabled = true;

  private void Start() => BootAsync().Forget();

  private async UniTaskVoid BootAsync()
  {
    DontDestroyOnLoad(this);

    await ConfigPreloader.PreloadAsync();

    if (Container.ProjectContainer == null)
      await SceneManager.LoadSceneAsync(BootstrapState.BootstrapSceneName);

    Boot(Container.ProjectContainer ?? throw new InvalidOperationException(
      $"Reflex ProjectContainer was not built: {BootstrapState.BootstrapSceneName} must contain a SceneScope"));
  }

  public void InstallBindings(ContainerBuilder containerBuilder)
  {
    containerBuilder.AddCore(_networkEnabled);
  }

  private static void Boot(Container container)
  {
    if (container.HasBinding<IReadOnlyList<ICoreRegistrationContributor>>())
    {
      foreach (var contributor in container.Resolve<IReadOnlyList<ICoreRegistrationContributor>>())
      {
        if (contributor is ICoreStartupContributor startup)
          startup.Initialize(container);
      }
    }

    if (container.HasBinding<GatewayServerIngress>())
    {
      container.Resolve<GatewayServerIngress>();
    }

    container.Resolve<IGameStateMachine>().Enter<BootstrapState>();
  }
}
