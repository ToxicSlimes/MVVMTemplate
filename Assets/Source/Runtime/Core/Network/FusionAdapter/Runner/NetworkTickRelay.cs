using Fusion;

namespace Core.Network.FusionAdapter.Runner
{
    public sealed class NetworkTickRelay : SimulationBehaviour
    {
        private NetworkTickSource _source;

        public void Bind(NetworkTickSource source) => _source = source;

        public override void FixedUpdateNetwork()
        {
            if (Runner.IsForward) _source.Raise(Runner.DeltaTime);
        }
    }
}
