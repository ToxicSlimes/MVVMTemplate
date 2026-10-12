using System;
using Reflex.Core;
using Scellecs.Morpeh;

namespace Core.DependencyInjection.Bindings
{
    public sealed class MorpehWorldProvider
    {
        public World World { get; }

        public MorpehWorldProvider()
        {
            var world = Scellecs.Morpeh.World.Default;
            if (world == null || world.IsDisposed)
            {
                throw new InvalidOperationException(
                    "Morpeh default world is not initialized. Keep Morpeh auto-initialization enabled.");
            }

            World = world;
        }
    }

    public static class MorpehBindings
    {
        public static ContainerBuilder AddMorpehEcs(this ContainerBuilder builder)
        {
            builder.AddSingleton(_ => new MorpehWorldProvider());
            return builder;
        }
    }
}
