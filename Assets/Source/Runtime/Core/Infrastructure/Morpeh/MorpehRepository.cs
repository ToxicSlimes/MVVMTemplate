using System;
using Scellecs.Morpeh;

namespace Core.Infrastructure.Morpeh
{
    /// <summary>
    /// Base for feature repositories that expose named access to Morpeh data.
    /// Systems and services should depend on concrete repositories instead of the world directly.
    /// </summary>
    public abstract class MorpehRepository
    {
        protected MorpehRepository(MorpehWorldProvider worldProvider)
        {
            if (worldProvider == null)
            {
                throw new ArgumentNullException(nameof(worldProvider));
            }

            World = worldProvider.World;
        }

        protected World World { get; }

        protected Stash<TComponent> GetStash<TComponent>()
            where TComponent : struct, IComponent => World.GetStash<TComponent>();

        protected Entity CreateEntity() => World.CreateEntity();

        protected void RemoveEntity(Entity entity) => World.RemoveEntity(entity);
    }
}
