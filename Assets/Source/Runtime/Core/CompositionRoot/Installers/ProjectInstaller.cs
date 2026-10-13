using Core.Abstractions.StateMachines;
using Core.Network.Abstractions.Gateway.Transport;
using Core.DependencyInjection.Bindings;
using Reflex.Core;
using Reflex.Exceptions;
using UnityEngine;

public class ProjectInstaller : MonoBehaviour, IInstaller
{
  [SerializeField] private bool _networkEnabled = true;

  private Container _ownedFallbackContainer;

  private void Start()
  {
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
    container.Resolve<MorpehWorldProvider>();

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
    _ownedFallbackContainer = new ContainerBuilder()
      .AddCore(_networkEnabled)
      .Build();

    return _ownedFallbackContainer;
  }
}