using System;
using FFS.Libraries.StaticEcs;

namespace Core.Infrastructure.StaticEcs
{
    internal static class WorldOwnership<TWorld> where TWorld : struct, IWorldType
    {
        internal static bool Reserved;
    }

    // Upstream pipeline resources are keyed by systems type, not world type.
    internal static class SystemsOwnership<TSystems> where TSystems : struct, ISystemsType
    {
        internal static bool Reserved;
    }

    /// <summary>
    /// Owns one StaticEcs world and one systems pipeline. Use unique world and systems markers
    /// per live owner; their state is static. All calls must be made on the same thread.
    /// </summary>
    /// <remarks>
    /// Construction creates the world and pipeline. Register systems with <see cref="Add{TSystem}"/>,
    /// then call <see cref="Initialize"/>, <see cref="Update"/> once per frame and <see cref="Dispose"/>.
    /// Do not operate on the same world or pipeline directly while this owner is alive.
    /// Dispose after a failed initialization. A failed systems initialization is not transactional:
    /// StaticEcs does not call Destroy for systems whose Init completed before another Init threw.
    /// If creation fails before publishing the world, the marker is released. Partial creation
    /// or a disposal failure retains the reservation until a domain reload;
    /// StaticEcs can leave partially initialized or destroyed static pipeline data behind.
    /// </remarks>
    public sealed class StaticEcsRuntime<TWorld, TSystems> : IDisposable
        where TWorld : struct, IWorldType
        where TSystems : struct, ISystemsType
    {
        private enum State { Created, Initializing, Initialized, Disposed }
        private State _state;
        private readonly bool _tick;

        /// <summary>Creates the world and its pipeline; throws if the world already exists.</summary>
        /// <param name="config">StaticEcs world configuration.</param>
        public StaticEcsRuntime(WorldConfig config = default)
        {
            if (WorldOwnership<TWorld>.Reserved || World<TWorld>.Status != WorldStatus.NotCreated)
                throw new InvalidOperationException("This StaticEcs world is owned or its cleanup failed.");
            if (SystemsOwnership<TSystems>.Reserved)
                throw new InvalidOperationException("This StaticEcs systems marker is owned or its cleanup failed.");
            WorldOwnership<TWorld>.Reserved = true;
            SystemsOwnership<TSystems>.Reserved = true;
            _tick = (config.TrackingBufferSize ?? 8) != 0;
            try { World<TWorld>.Create(config); }
            catch
            {
                // The Data constructor can fail before publishing a world. In that case
                // no static pipeline exists and the marker is safe to reuse.
                if (World<TWorld>.Status == WorldStatus.NotCreated)
                {
                    WorldOwnership<TWorld>.Reserved = false;
                    SystemsOwnership<TSystems>.Reserved = false;
                }
                throw;
            }
            try { World<TWorld>.Systems<TSystems>.Create(); }
            catch
            {
                World<TWorld>.Destroy();
                // The pipeline may have been partially created; retain the reservation.
                throw;
            }
#if UNITY_EDITOR
            try { FFS.Libraries.StaticEcs.Unity.EcsDebug<TWorld>.AddWorld<TSystems>(); }
            catch
            {
                // AddWorld publishes global data and a listener before it adds the system.
                // Remove even on partial registration, while the world is still alive.
                try { FFS.Libraries.StaticEcs.Unity.EcsDebug<TWorld>.RemoveWorld(); }
                finally
                {
                    try { World<TWorld>.Systems<TSystems>.Destroy(); }
                    finally { World<TWorld>.Destroy(); }
                }
                // The pipeline may be partially modified; retain both reservations.
                throw;
            }
#endif
        }

        /// <summary>Adds a system before initialization, ordered by ascending priority.</summary>
        /// <remarks>Explicit lifecycle and snapshot hooks are rejected because StaticEcs cannot discover them.</remarks>
        public void Add<TSystem>(TSystem system, short order = 0) where TSystem : ISystem
        {
            Require(State.Created);
            if (ReferenceEquals(system, null)) throw new ArgumentNullException(nameof(system));
            var map = system.GetType().GetInterfaceMap(typeof(ISystem));
            for (var i = 0; i < map.InterfaceMethods.Length; i++)
            {
                var name = map.InterfaceMethods[i].Name;
                if ((name == nameof(ISystem.Init) || name == nameof(ISystem.Update) ||
                     name == nameof(ISystem.Destroy) || name == nameof(ISystem.Write) || name == nameof(ISystem.Read)) &&
                    !map.TargetMethods[i].IsPublic)
                    throw new NotSupportedException($"StaticEcs cannot detect explicit ISystem.{name}; implement it publicly.");
            }
            World<TWorld>.Systems<TSystems>.Add(system, order);
        }

        /// <summary>Initializes the world, then initializes its registered systems.</summary>
        public void Initialize()
        {
            Require(State.Created);
            _state = State.Initializing;
            World<TWorld>.Initialize();
            World<TWorld>.Systems<TSystems>.Initialize();
            _state = State.Initialized;
        }

        /// <summary>Runs the pipeline and advances tracking when enabled by configuration.</summary>
        public void Update()
        {
            Require(State.Initialized);
            World<TWorld>.Systems<TSystems>.Update();
            if (_tick) World<TWorld>.Tick();
        }

        /// <summary>Destroys systems before the world. Subsequent calls are ignored.</summary>
        public void Dispose()
        {
            if (_state == State.Disposed) return;
            _state = State.Disposed;
            try { World<TWorld>.Systems<TSystems>.Destroy(); }
            finally
            {
#if UNITY_EDITOR
                try { FFS.Libraries.StaticEcs.Unity.EcsDebug<TWorld>.RemoveWorld(); }
                finally { World<TWorld>.Destroy(); }
#else
                World<TWorld>.Destroy();
#endif
            }
            WorldOwnership<TWorld>.Reserved = false;
            SystemsOwnership<TSystems>.Reserved = false;
        }

        private void Require(State state)
        {
            if (_state == State.Disposed) throw new ObjectDisposedException(GetType().Name);
            if (_state != state) throw new InvalidOperationException($"Expected {state}, current state is {_state}.");
        }
    }
}
