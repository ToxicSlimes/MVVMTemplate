using Scellecs.Morpeh;

namespace Core.Gameplay.Player
{
    public sealed class PlayerCrouchSystem : ISystem
    {
        private readonly PlayerCrouchService _crouch;

        private Filter _requests;
        private Filter _players;

        public PlayerCrouchSystem(PlayerCrouchService crouch)
        {
            _crouch = crouch;
        }

        public World World { get; set; }

        public void OnAwake()
        {
            _requests = World.Filter.With<SetCrouchRequest>().Build();
            _players = World.Filter.Extend<PlayerAspect>().Build();
        }

        public void OnUpdate(float deltaTime)
        {
            foreach (var player in _players)
            {
                foreach (var request in _requests)
                {
                    _crouch.Apply(request, player);
                }

                _crouch.MoveCamera(player, deltaTime);
            }
        }

        public void Dispose()
        {
        }
    }
}
