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
      
      // TODO: Replace with your actual next state transition
      // Example: StateMachine.Enter<YourMainMenuState>();
    }
    catch (System.Exception ex)
    {
      _logger.Error(ex, "[DataPreloadState] Failed to preload data");
      // TODO: Handle error - show error screen or retry
    }
  }

  public override void Exit()
  {
  }
}
