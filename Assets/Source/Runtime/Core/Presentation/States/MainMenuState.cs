using Core.Abstractions.MVVM.Services;
using Core.Abstractions.StateMachines;
using Core.Abstractions.StateMachines.States;
using Core.MVVM.ViewModels.MainMenu;
using Cysharp.Threading.Tasks;
using Core.Abstractions.Services;

public class MainMenuState : GameStateBase<MainMenuState>, IEnterState
{
    private readonly IViewService _viewService;
    private readonly ILogger _logger;

    public MainMenuState(IViewService viewService, ILogger logger)
    {
        _viewService = viewService;
        _logger = logger;
    }

    public void Enter()
    {
        _logger.Info("[MainMenuState] Entering main menu");
        ShowMainMenu().Forget();
    }

    private async UniTaskVoid ShowMainMenu()
    {
        await _viewService.ShowAsync<MainMenuViewModel>();
    }

    public override void Exit()
    {
        _logger.Info("[MainMenuState] Exiting main menu");
    }
}
