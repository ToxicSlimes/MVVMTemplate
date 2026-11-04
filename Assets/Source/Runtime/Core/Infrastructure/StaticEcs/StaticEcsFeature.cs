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
    /// Change tracking of a
    /// child is relative to the feature's last run, so a long-disabled feature sees all tracked changes it missed.
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
            foreach (var system in _systems) system.Init();
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
            foreach (var system in _systems) system.Destroy();
        }

        protected void AddSystem(ISystem system)
        {
            if (_initialized) throw new InvalidOperationException("Systems cannot be added after initialization.");
            if (ReferenceEquals(system, null)) throw new ArgumentNullException(nameof(system));
            _systems.Add(system);
        }
    }
}
