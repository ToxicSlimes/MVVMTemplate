using System;
using FFS.Libraries.StaticEcs;

namespace Core.Infrastructure.StaticEcs
{
    /// <summary>
    /// Base for feature repositories that expose named access to StaticEcs data of one world.
    /// Systems and services should depend on concrete repositories instead of the static world.
    /// </summary>
    /// <remarks>Construct after the world is created, for example from <see cref="INetworkStaticEcsFeature.Configure"/>.</remarks>
    public abstract class StaticEcsRepository<TWorld> where TWorld : struct, IWorldType
    {
        protected StaticEcsRepository()
        {
            if (World<TWorld>.Status == WorldStatus.NotCreated)
                throw new InvalidOperationException("The StaticEcs world is not created.");
        }

        protected World<TWorld>.Entity CreateEntity() => World<TWorld>.NewEntity<Default>();

        protected World<TWorld>.Entity CreateEntity<TEntityType>() where TEntityType : struct, IEntityType =>
            World<TWorld>.NewEntity<TEntityType>();

        protected bool RemoveEntity(World<TWorld>.Entity entity) => entity.Destroy();

        protected ref TComponent AddComponent<TComponent>(World<TWorld>.Entity entity)
            where TComponent : struct, IComponent => ref entity.Add<TComponent>();

        protected ref TComponent GetComponent<TComponent>(World<TWorld>.Entity entity)
            where TComponent : struct, IComponent => ref entity.Ref<TComponent>();

        protected bool HasComponent<TComponent>(World<TWorld>.Entity entity)
            where TComponent : struct, IComponent => entity.Has<TComponent>();
    }
}
