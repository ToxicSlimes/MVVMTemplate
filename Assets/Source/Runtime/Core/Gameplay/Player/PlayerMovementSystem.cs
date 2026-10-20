using Core.Gameplay.Input;
using Scellecs.Morpeh;

namespace Core.Gameplay.Player
{
    public sealed class PlayerMovementSystem : ISystem
    {
        private readonly PlayerMovementService _movement;

        private Filter _playerInputs;
        private Filter _players;

        public PlayerMovementSystem(PlayerMovementService movement)
        {
            _movement = movement;
        }

        public World World { get; set; }

        public void OnAwake()
        {
            _playerInputs = World.Filter.Extend<PlayerInputAspect>().Build();
            _players = World.Filter.Extend<PlayerAspect>().Build();
        }

        public void OnUpdate(float deltaTime)
        {
            foreach (var playerInput in _playerInputs)
            {
                foreach (var player in _players)
                {
                    _movement.Move(playerInput, player, deltaTime);
                }
            }
        }

        public void Dispose()
        {
        }
    }
}
