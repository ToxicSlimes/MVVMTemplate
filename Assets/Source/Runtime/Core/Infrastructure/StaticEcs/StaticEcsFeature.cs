using System;
using System.Collections.Generic;
using FFS.Libraries.StaticEcs;

namespace Core.Infrastructure.StaticEcs
{
    /// <summary>
    /// A switchable group of systems. StaticEcs cannot add systems after pipeline initialization,
    /// so a feature is registered once before <see cref="StaticEcsRuntime{TWorld,TSystems}.Initialize"/>,
    /// starts disabled, and is toggled with <see cref="Enable"/> and <see cref="Disable"/> (for example on state Enter and Exit).
    /// </summary>
    /// <remarks>
    /// Add child systems from the derived constructor with <see cref="AddSystem"/>; they run in insertion order
    /// while the feature is enabled. Child Init and Destroy run with the pipeline regardless of the enabled state.
    /// Tracking belongs to the feature as a whole, not to each child. When disabled, the feature's
    /// last tick does not advance; missed history is limited by WorldConfig.TrackingBufferSize.
    /// A child skipped by UpdateIsActive does not have an independent tracking cursor.
    /// Child snapshot hooks are not registered with the native pipeline and are not forwarded.
    /// </remarks>
    public abstract class StaticEcsFeature : ISystem
    {
        private readonly List<ISystem> _systems = new List<ISystem>();
        private bool _enabled;
        private bool _initialized;

        public bool IsEnabled => _enabled;

        public void Enable() => _enabled = true;

        public void Disable() => _enabled = false;

        public bool UpdateIsActive() => _enabled;

        public void Init()
        {
            if (_initialized) throw new InvalidOperationException("The feature is already initialized.");
            _initialized = true;
            var initializedCount = 0;
            try
            {
                foreach (var system in _systems)
                {
                    system.Init();
                    initializedCount++;
                }
            }
            catch (Exception initError)
            {
                _initialized = false;
                List<Exception> errors = null;
                for (var i = 0; i < initializedCount; i++)
                {
                    try { _systems[i].Destroy(); }
                    catch (Exception error)
                    {
                        if (errors == null) errors = new List<Exception> { initError };
                        errors.Add(error);
                    }
                }
                if (errors != null) throw new AggregateException(errors);
                throw;
            }
        }

        public void Update()
        {
            foreach (var system in _systems)
                if (system.UpdateIsActive()) system.Update();
        }

        public void Destroy()
        {
            if (!_initialized) return;
            _initialized = false;
            List<Exception> errors = null;
            foreach (var system in _systems)
            {
                try { system.Destroy(); }
                catch (Exception error)
                {
                    if (errors == null) errors = new List<Exception>();
                    errors.Add(error);
                }
            }
            if (errors != null) throw new AggregateException(errors);
        }

        protected void AddSystem(ISystem system)
        {
            if (_initialized) throw new InvalidOperationException("Systems cannot be added after initialization.");
            if (ReferenceEquals(system, null)) throw new ArgumentNullException(nameof(system));
            _systems.Add(system);
        }
    }
}
