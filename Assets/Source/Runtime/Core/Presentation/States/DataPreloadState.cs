using Core.Abstractions.StateMachines.States;
using Core.Abstractions.StateMachines;
using System.Threading;
using Core.Abstractions.Services;
using Cysharp.Threading.Tasks;

/// <summary>
/// Template pass-through state for data preloading.
/// Replace with your actual data preloading logic and next state transition.
/// </summary>
public class DataPreloadState : GameStateBase<DataPreloadState>, IEnterState
{
  private readonly IDataPreloadService _dataPreloadService;
  private readonly ILogger _logger;

  public DataPreloadState(IDataPreloadService dataPreloadService, ILogger logger)
  {
    _dataPreloadService = dataPreloadService;
    _logger = logger;
  }

  public void Enter()
  {
    LoadDataAndTransition().Forget();
  }

  private async UniTaskVoid LoadDataAndTransition()
  {
    try
    {
      await _dataPreloadService.PreloadDataAsync(cancellationToken: CancellationToken.None);
      _logger.Info("[DataPreloadState] Data preloaded successfully");
      
      StateMachine.Enter<MainMenuState>();
    }
    catch (System.Exception ex)
    {
      _logger.Error(ex, "[DataPreloadState] Failed to preload data");
      // TEMPLATE: Handle error — show error screen or retry
      // Fallback: go to main menu anyway
      StateMachine.Enter<MainMenuState>();
    }
  }

  public override void Exit()
  {
  }
}
