using Core.Abstractions.Navigation;
using Core.Infrastructure.Mediator.Abstractions;

namespace Core.Application.Commands.Common
{
    public readonly struct NavigateCommand<TTarget> : IRequest<Nothing>
        where TTarget : struct, INavigationTarget { }

    public static class Navigate
    {
        public static NavigateCommand<MainMenuNavigation> ToMainMenu() => new();
        public static NavigateCommand<GameNavigation> ToGame() => new();
        public static NavigateCommand<SettingsNavigation> ToSettings() => new();
        public static NavigateCommand<InventoryNavigation> ToInventory() => new();
    }
}
