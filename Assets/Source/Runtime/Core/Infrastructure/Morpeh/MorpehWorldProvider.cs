using System;
using Scellecs.Morpeh;

namespace Core.Infrastructure.Morpeh
{
    public sealed class MorpehWorldProvider
    {
        public MorpehWorldProvider()
        {
            var world = World.Default;
            if (world == null || world.IsDisposed)
            {
                throw new InvalidOperationException(
                    "Morpeh default world is not initialized. Keep Morpeh auto-initialization enabled.");
            }

            World = world;
        }

        public World World { get; }
    }
}
