using Core.Abstractions.Navigation;
using Core.Abstractions.StateMachines;

namespace Core.Presentation.Handlers
{
    public class MainMenuNavigationHandler : NavigateHandlerBase<MainMenuNavigation, MainMenuState>
    {
        public MainMenuNavigationHandler(IGameStateMachine stateMachine) : base(stateMachine) { }
    }

    public class InventoryNavigationHandler : NavigateHandlerBase<InventoryNavigation, InventoryState>
    {
        public InventoryNavigationHandler(IGameStateMachine stateMachine) : base(stateMachine) { }
    }

    // TEMPLATE: Add more navigation handlers as you add states.
    // Example:
    // public class GameNavigationHandler : NavigateHandlerBase<GameNavigation, GameplayState>
    // {
    //     public GameNavigationHandler(IGameStateMachine stateMachine) : base(stateMachine) { }
    // }
}
