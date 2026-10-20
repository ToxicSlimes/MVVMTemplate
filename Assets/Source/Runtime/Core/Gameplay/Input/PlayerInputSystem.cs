using Scellecs.Morpeh;

namespace Core.Gameplay.Input
{
    public sealed class PlayerInputSystem : ISystem
    {
        private readonly PlayerInputService _input;

        private Filter _playerInputs;
        private Filter _touchLooks;

        public PlayerInputSystem(PlayerInputService input)
        {
            _input = input;
        }

        public World World { get; set; }

        public void OnAwake()
        {
            _playerInputs = World.Filter.Extend<PlayerInputAspect>().Build();
            _touchLooks = World.Filter.Extend<TouchLookAspect>().Build();
            _input.Create();
        }

        public void OnUpdate(float deltaTime)
        {
            foreach (var playerInput in _playerInputs)
            {
                _input.Read(playerInput);
                foreach (var touchLook in _touchLooks)
                {
                    _input.AddTouch(playerInput, touchLook);
                }
            }
        }

        public void Dispose()
        {
            if (_playerInputs == null)
            {
                return;
            }

            foreach (var playerInput in _playerInputs)
            {
                _input.Remove(playerInput);
            }
        }
    }
}
