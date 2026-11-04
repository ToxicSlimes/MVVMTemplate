using System;
using Core.DependencyInjection;
using Core.Mediator.Registration;
using FFS.Libraries.StaticEcs;
using Reflex.Core;
using UnityEngine;
using UnityEngine.Scripting;

namespace Core.Infrastructure.StaticEcs
{
    [Preserve] public struct DefaultStaticEcsWorld : IWorldType { }
    [Preserve] public struct DefaultStaticEcsSystems : ISystemsType { }

    /// <summary>Register implementations with Reflex as IStaticEcsFeature. Configure runs in binding order before world initialization.</summary>
    public interface IStaticEcsFeature
    {
        void Configure(StaticEcsRuntime<DefaultStaticEcsWorld, DefaultStaticEcsSystems> runtime, Container container);
    }

    [Preserve]
    public sealed class StaticEcsRegistrationContributor : ICoreRegistrationContributor, ICoreStartupContributor
    {
        public void RegisterBindings(ContainerBuilder builder) => builder.AddSingleton(_ => new StaticEcsOwner());
        public void RegisterMediator(MediatorBuilder builder) { }
        public void Initialize(Container container) => container.Resolve<StaticEcsOwner>().Initialize(container);
    }

    /// <summary>Reflex owns this singleton; the Unity runner also disposes it on destruction.</summary>
    [Preserve]
    public sealed class StaticEcsOwner : IDisposable
    {
        private StaticEcsRuntime<DefaultStaticEcsWorld, DefaultStaticEcsSystems> _runtime;
        private StaticEcsRunner _runner;
        private bool _disposed;
        private bool _started;

        public void Initialize(Container container, bool startRunner = true)
        {
            if (container == null) throw new ArgumentNullException(nameof(container));
            if (_disposed) throw new ObjectDisposedException(nameof(StaticEcsOwner));
            if (_started) return;
            _started = true;
            try
            {
                _runtime = new StaticEcsRuntime<DefaultStaticEcsWorld, DefaultStaticEcsSystems>();
                foreach (var feature in container.All<IStaticEcsFeature>()) feature.Configure(_runtime, container);
                _runtime.Initialize();
                if (startRunner) StartRunner();
            }
            catch { Dispose(); throw; }
        }

        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
        private void StartRunner()
        {
            if (!Application.isPlaying) return;
            var gameObject = new GameObject("StaticEcs Runner");
            UnityEngine.Object.DontDestroyOnLoad(gameObject);
            _runner = gameObject.AddComponent<StaticEcsRunner>();
            _runner.Attach(this);
        }

        public void Update()
        {
            if (_disposed) throw new ObjectDisposedException(nameof(StaticEcsOwner));
            if (_runtime == null) throw new InvalidOperationException("StaticEcs owner is not initialized.");
            _runtime.Update();
        }

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;
            if (!ReferenceEquals(_runner, null))
            {
                var runner = _runner;
                _runner = null;
                runner.Detach();
                UnityEngine.Object.Destroy(runner.gameObject);
            }
            _runtime?.Dispose();
            _runtime = null;
        }
    }

}
