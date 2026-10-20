using Scellecs.Morpeh;
using UnityEngine;

namespace Core.Gameplay.Player
{
    public struct PlayerAspect : IFilterExtension
    {
        private readonly Stash<PlayerController> _controller;
        private readonly Stash<PlayerCamera> _camera;

        public PlayerAspect(World world)
        {
            _controller = world.GetStash<PlayerController>();
            _camera = world.GetStash<PlayerCamera>();
            Entity = default;
        }

        public Entity Entity { get; private set; }

        public CharacterController Controller => _controller.Get(Entity).Value;
        public Transform Camera => _camera.Get(Entity).Value;

        public PlayerAspect For(Entity entity)
        {
            var aspect = this;
            aspect.Entity = entity;
            return aspect;
        }

        public FilterBuilder Extend(FilterBuilder rootFilter) => rootFilter.With<PlayerController>().With<PlayerCamera>();
    }
}
