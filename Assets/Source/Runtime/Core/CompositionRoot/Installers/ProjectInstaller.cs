using System.Collections.Generic;
using Core.Abstractions.StateMachines;
using Core.Network.Abstractions.Gateway.Transport;
using Core.DependencyInjection.Bindings;
using Core.Infrastructure.Services.Config;
using Cysharp.Threading.Tasks;
using Core.DependencyInjection;
using Reflex.Core;
using Reflex.Exceptions;
using UnityEngine;

public class ProjectInstaller : MonoBehaviour, IInstaller
{
  [SerializeField] private bool _networkEnabled = true;

  private Container _ownedFallbackContainer;

  private void Start() => BootAsync().Forget();

  private async UniTaskVoid BootAsync()
  {
    await ConfigPreloader.PreloadAsync();

    var container = Container.ProjectContainer ?? BuildProjectContainer();

    try
    {
      Boot(container);
    }
    catch (UnknownContractException)
    {
      Boot(BuildProjectContainer());
    }

    DontDestroyOnLoad(this);
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

  private void OnDestroy()
  {
    _ownedFallbackContainer?.Dispose();
    _ownedFallbackContainer = null;
  }

  private Container BuildProjectContainer()
  {
    _ownedFallbackContainer?.Dispose();

    var builder = new ContainerBuilder();
    foreach (var installer in GetComponentsInChildren<IInstaller>())
    {
      installer.InstallBindings(builder);
    }

    _ownedFallbackContainer = builder.Build();
    return _ownedFallbackContainer;
  }
}