using Core.Abstractions.Navigation;
using Core.Infrastructure.Mediator.Abstractions;

namespace Core.Application.Commands.Common
{
    public readonly struct NavigateCommand<TTarget> : IRequest<Nothing>
        where TTarget : struct, INavigationTarget { }

    public static class Navigate
    {
        public static NavigateCommand<LoginNavigation> ToLogin() => new();
        public static NavigateCommand<MenuSelectionNavigation> ToMenuSelection() => new();
        public static NavigateCommand<ChatNavigation> ToChat() => new();
        public static NavigateCommand<ImageGenNavigation> ToImageGen() => new();
        public static NavigateCommand<MainNavigation> ToMain() => new();
        public static NavigateCommand<StandaloneWorkflowNavigation> ToStandaloneWorkflow() => new();
    }
}
