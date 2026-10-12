using Core.Abstractions.StateMachines;
using Core.Network.Abstractions.Gateway.Transport;
using Reflex.Core;
using Reflex.Exceptions;
using UnityEngine;

public class ProjectInstaller : MonoBehaviour, IInstaller
{
  private Container _ownedFallbackContainer;

  private void Start()
  {
    var container = Container.ProjectContainer ?? BuildProjectContainer();

    try
    {
      container.Resolve<GatewayServerIngress>();
      container.Resolve<IGameStateMachine>().Enter<BootstrapState>();
    }
    catch (UnknownContractException)
    {
      container = BuildProjectContainer();
      container.Resolve<GatewayServerIngress>();
      container.Resolve<IGameStateMachine>().Enter<BootstrapState>();
    }

    DontDestroyOnLoad(this);
  }

  public void InstallBindings(ContainerBuilder containerBuilder)
  {
    containerBuilder.AddCore();
  }

  private void OnDestroy()
  {
    _ownedFallbackContainer?.Dispose();
    _ownedFallbackContainer = null;
  }

  private Container BuildProjectContainer()
  {
    _ownedFallbackContainer?.Dispose();
    _ownedFallbackContainer = new ContainerBuilder()
      .AddCore()
      .Build();

    return _ownedFallbackContainer;
  }
}