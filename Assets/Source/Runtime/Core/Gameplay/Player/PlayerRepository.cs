using Core.Infrastructure.Morpeh;
using Scellecs.Morpeh;

namespace Core.Gameplay.Player
{
    public sealed class PlayerRepository : MorpehRepository
    {
        public PlayerRepository(MorpehWorldProvider worldProvider) : base(worldProvider)
        {
            Players = new PlayerAspect(World);
            Crouching = GetStash<Crouching>();
            CrouchRequests = GetStash<SetCrouchRequest>();
        }

        public PlayerAspect Players { get; }
        public Stash<Crouching> Crouching { get; }
        public Stash<SetCrouchRequest> CrouchRequests { get; }
    }
}
