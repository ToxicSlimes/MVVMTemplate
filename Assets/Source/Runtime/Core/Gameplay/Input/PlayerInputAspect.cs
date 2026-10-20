using Scellecs.Morpeh;
using UnityEngine;

namespace Core.Gameplay.Input
{
    public struct PlayerInputAspect : IFilterExtension
    {
        private readonly Stash<MoveInput> _move;
        private readonly Stash<LookInput> _look;
        private readonly Stash<AttackInput> _attack;
        private readonly Stash<AutoFire> _autoFire;

        public PlayerInputAspect(World world)
        {
            _move = world.GetStash<MoveInput>();
            _look = world.GetStash<LookInput>();
            _attack = world.GetStash<AttackInput>();
            _autoFire = world.GetStash<AutoFire>();
            Entity = default;
        }

        public Entity Entity { get; private set; }

        public ref Vector2 Move => ref _move.Get(Entity).Value;
        public ref Vector2 Look => ref _look.Get(Entity).Value;
        public ref bool Attack => ref _attack.Get(Entity).Value;
        public ref bool AutoFire => ref _autoFire.Get(Entity).Value;

        public PlayerInputAspect For(Entity entity)
        {
            var aspect = this;
            aspect.Entity = entity;
            return aspect;
        }

        public void AddTo(Entity entity)
        {
            _move.Add(entity);
            _look.Add(entity);
            _attack.Add(entity);
            _autoFire.Add(entity);
        }

        public FilterBuilder Extend(FilterBuilder rootFilter) =>
            rootFilter.With<MoveInput>().With<LookInput>().With<AttackInput>().With<AutoFire>();
    }
}
