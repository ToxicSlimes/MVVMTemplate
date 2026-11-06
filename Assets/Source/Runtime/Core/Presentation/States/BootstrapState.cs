using System;
using Core.Abstractions.StateMachines.States;
using Core.Abstractions.StateMachines;
using Core.Abstractions.Services;
using Core.Application.Handlers.Player;
using Cysharp.Threading.Tasks;
using UnityEngine.SceneManagement;

public class BootstrapState : GameStateBase<BootstrapState>, IEnterState
{
  public const string BootstrapSceneName = "BootstrapScene";
  private readonly ILogger _logger;
  private readonly ICacheService _cache;

  public BootstrapState(ILogger logger, ICacheService cache, PauseInputPublisher pauseInput)
  {
    _logger = logger;
    _cache = cache;

    if (pauseInput == null)
    {
      throw new ArgumentNullException(nameof(pauseInput));
    }
  }

  public void Enter()
  {
    if (SceneManager.GetActiveScene().name != BootstrapSceneName)
    {
      _logger.Info("[BootstrapState] Loading bootstrap scene from {0}", SceneManager.GetActiveScene().name);

      var bootstrapPayload = new SceneLoadPayload
      (
        BootstrapSceneName,
        SceneLoadType.Regular,
        LoadSceneMode.Single,
        () => StateMachine.Enter<BootstrapState>()
      );

      StateMachine.Enter<LoadSceneState, SceneLoadPayload>(bootstrapPayload);
      return;
    }

    _cache.CleanupAsync().Forget();

    StateMachine.Enter<DataPreloadState>();
  }

  public override void Exit()
  {
  }
}
