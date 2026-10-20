using Core.Infrastructure.Morpeh;
using Scellecs.Morpeh;

namespace Core.Gameplay.Input
{
    public sealed class InputRepository : MorpehRepository
    {
        public InputRepository(MorpehWorldProvider worldProvider) : base(worldProvider)
        {
            PlayerInputs = new PlayerInputAspect(World);
            TouchLooks = new TouchLookAspect(World);
        }

        public PlayerInputAspect PlayerInputs { get; }
        public TouchLookAspect TouchLooks { get; }

        public void CreatePlayerInput() => PlayerInputs.AddTo(CreateEntity());

        public void Remove(Entity entity) => RemoveEntity(entity);
    }
}
