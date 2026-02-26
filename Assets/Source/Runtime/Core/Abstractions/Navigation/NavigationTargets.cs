namespace Core.Abstractions.Navigation
{
    public readonly struct LoginNavigation : INavigationTarget { }

    public readonly struct MenuSelectionNavigation : INavigationTarget { }

    public readonly struct ChatNavigation : INavigationTarget { }

    public readonly struct ImageGenNavigation : INavigationTarget { }

    public readonly struct MainNavigation : INavigationTarget { }

    public readonly struct StandaloneWorkflowNavigation : INavigationTarget { }
}
