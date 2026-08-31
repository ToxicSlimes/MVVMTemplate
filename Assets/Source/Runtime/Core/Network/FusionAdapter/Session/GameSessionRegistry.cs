using Core.Network.Abstractions.Services;

namespace Core.Network.FusionAdapter.Session
{
    public sealed class GameSessionRegistry : INetworkSessionControl
    {
        private GameSessionMachine _session;

        public bool Exists => _session != null;

        public void Set(GameSessionMachine session) => _session = session;

        public void Clear(GameSessionMachine session)
        {
            if (_session == session)
            {
                _session = null;
            }
        }

        public void StartMatch() => _session?.StartMatch();

        public void EndMatch() => _session?.EndMatch();
    }
}
