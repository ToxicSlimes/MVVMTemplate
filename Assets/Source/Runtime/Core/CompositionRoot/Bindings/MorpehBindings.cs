using Core.Infrastructure.Morpeh;
using Reflex.Core;

namespace Core.DependencyInjection.Bindings
{
    public static class MorpehBindings
    {
        public static ContainerBuilder AddMorpehEcs(this ContainerBuilder builder)
        {
            builder.AddSingleton(_ => new MorpehWorldProvider());
            return builder;
        }
    }
}
