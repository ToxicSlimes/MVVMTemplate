using Core.DependencyInjection;
using Core.DependencyInjection.Bindings;
using Core.Mediator.Registration;
using Reflex.Core;

namespace Core.Infrastructure.Morpeh
{
    [UnityEngine.Scripting.Preserve]
    public sealed class MorpehRegistrationContributor : ICoreRegistrationContributor, ICoreStartupContributor
    {
        public void RegisterBindings(ContainerBuilder builder) => builder.AddMorpehEcs();
        public void RegisterMediator(MediatorBuilder builder) { }
        public void Initialize(Container container) => container.Resolve<MorpehWorldProvider>();
    }
}
