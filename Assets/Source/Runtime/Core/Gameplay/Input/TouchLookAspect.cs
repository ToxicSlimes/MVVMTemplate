using Scellecs.Morpeh;
using UnityEngine;

namespace Core.Gameplay.Input
{
    public struct TouchLookAspect : IFilterExtension
    {
        private readonly Stash<TouchLookDelta> _delta;
        private readonly Stash<TouchLookHeld> _held;

        public TouchLookAspect(World world)
        {
            _delta = world.GetStash<TouchLookDelta>();
            _held = world.GetStash<TouchLookHeld>();
            Entity = default;
        }

        public Entity Entity { get; private set; }

        public ref Vector2 Delta => ref _delta.Get(Entity).Value;
        public ref bool Held => ref _held.Get(Entity).Value;

        public TouchLookAspect For(Entity entity)
        {
            var aspect = this;
            aspect.Entity = entity;
            return aspect;
        }

        public void AddTo(Entity entity)
        {
            _delta.Add(entity);
            _held.Add(entity);
        }

        public void RemoveFrom(Entity entity)
        {
            _delta.Remove(entity);
            _held.Remove(entity);
        }

        public FilterBuilder Extend(FilterBuilder rootFilter) => rootFilter.With<TouchLookDelta>().With<TouchLookHeld>();
    }
}
