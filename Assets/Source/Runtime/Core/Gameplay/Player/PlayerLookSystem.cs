using Core.Gameplay.Input;
using Scellecs.Morpeh;

namespace Core.Gameplay.Player
{
    public sealed class PlayerLookSystem : ISystem
    {
        private readonly PlayerLookService _look;

        private Filter _playerInputs;
        private Filter _players;

        public PlayerLookSystem(PlayerLookService look)
        {
            _look = look;
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
                    _look.Look(playerInput, player);
                }
            }
        }

        public void Dispose()
        {
        }
    }
}
