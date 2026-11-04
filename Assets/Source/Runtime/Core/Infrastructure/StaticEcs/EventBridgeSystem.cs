using System;
using FFS.Libraries.StaticEcs;

namespace Core.Infrastructure.StaticEcs
{
    /// <summary>Bridges a registered StaticEcs event type to a non-ECS consumer.</summary>
    /// <remarks>
    /// Register TEvent with the world before initialization. The pipeline registers this
    /// receiver during Init, so events sent before Init are not received. Access on the
    /// world thread only; Destroy must run before the world is destroyed.
    /// </remarks>
    public abstract class EventBridgeSystem<TWorld, TEvent> : ISystem
        where TWorld : struct, IWorldType
        where TEvent : struct, IEvent
    {
        private EventReceiver<TWorld, TEvent> _receiver;
        private bool _initialized;

        public void Init()
        {
            if (_initialized) throw new InvalidOperationException("The event bridge is already initialized.");
            _receiver = World<TWorld>.RegisterEventReceiver<TEvent>();
            _initialized = true;
        }

        public void Update()
        {
            if (!_initialized) throw new InvalidOperationException("The event bridge is not initialized.");
            foreach (var item in _receiver)
                Bridge(in item.Value);
        }

        public void Destroy()
        {
            if (!_initialized) return;
            World<TWorld>.DeleteEventReceiver(ref _receiver);
            _initialized = false;
        }

        protected abstract void Bridge(in TEvent value);
    }
}
