using Core.Abstractions.StateMachines.States;
using Core.Abstractions.StateMachines;
using Core.Abstractions.Services;
using Cysharp.Threading.Tasks;
using UnityEngine.SceneManagement;

public class BootstrapState : GameStateBase<BootstrapState>, IEnterState
{
  private const string BootstrapSceneName = "BootstrapScene";
  private readonly ILogger _logger;
  private readonly ICacheService _cache;

  public BootstrapState(ILogger logger, ICacheService cache)
  {
    _logger = logger;
    _cache = cache;
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
