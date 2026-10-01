using Core.Abstractions.StateMachines;
using Reflex.Core;
using Reflex.Exceptions;
using UnityEngine;

public class ProjectInstaller : MonoBehaviour, IInstaller
{
  private void Start()
  {
    var container = Container.ProjectContainer ?? BuildProjectContainer();

    try
    {
      container.Resolve<IGameStateMachine>().Enter<BootstrapState>();
    }
    catch (UnknownContractException)
    {
      container = BuildProjectContainer();
      container.Resolve<IGameStateMachine>().Enter<BootstrapState>();
    }

    DontDestroyOnLoad(this);
  }

  public void InstallBindings(ContainerBuilder containerBuilder)
  {
    containerBuilder.AddCore();
  }

  private static Container BuildProjectContainer()
  {
    var container = new ContainerBuilder()
      .AddCore()
      .Build();

    return container;
  }
}