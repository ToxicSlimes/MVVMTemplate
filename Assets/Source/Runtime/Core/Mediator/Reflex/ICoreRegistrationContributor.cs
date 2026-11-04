using Core.Mediator.Registration;
using Reflex.Core;

namespace Core.DependencyInjection
{
    /// <summary>
    /// Extends the package composition root without creating a second mediator.
    /// Consumer-owned generated registrations implement this interface.
    /// </summary>
    public interface ICoreRegistrationContributor
    {
        void RegisterBindings(ContainerBuilder builder);
        void RegisterMediator(MediatorBuilder builder);
    }

    public interface ICoreStartupContributor
    {
        void Initialize(Container container);
    }
}
