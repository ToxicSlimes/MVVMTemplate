using Core.Abstractions.MVVM.Services;
using Core.Abstractions.StateMachines;
using Core.Abstractions.StateMachines.States;
using Core.Presentation.MVVM.ViewModels.Inventory;
using Core.Presentation.MVVM.Views.Inventory;
using Cysharp.Threading.Tasks;
using Core.Abstractions.Services;

public class InventoryState : GameStateBase<InventoryState>, IEnterState
{
    private readonly IViewService _viewService;
    private readonly ILogger _logger;

    public InventoryState(IViewService viewService, ILogger logger)
    {
        _viewService = viewService;
        _logger = logger;
    }

    public void Enter()
    {
        _logger.Info("[InventoryState] Entering inventory");
        ShowInventory().Forget();
    }

    private async UniTaskVoid ShowInventory()
    {
        await _viewService.ShowAsync<InventoryView, InventoryViewModel>();
    }

    public override void Exit()
    {
        _logger.Info("[InventoryState] Exiting inventory");
    }
}
