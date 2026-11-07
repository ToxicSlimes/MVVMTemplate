using System;
using Core.DependencyInjection;
using Core.Mediator.Registration;
using FFS.Libraries.StaticEcs;
using Reflex.Core;
using UnityEngine;
using UnityEngine.Scripting;

namespace Core.Infrastructure.StaticEcs
{
    [Preserve] public struct NetworkWorld : IWorldType { }
    [Preserve] public struct NetworkSystems : ISystemsType { }
    [Preserve] public struct ViewWorld : IWorldType { }
    [Preserve] public struct ViewSystems : ISystemsType { }

    /// <summary>Register implementations with Reflex as INetworkStaticEcsFeature. Configure runs in binding order before world initialization.</summary>
    public interface INetworkStaticEcsFeature
    {
        void Configure(StaticEcsRuntime<NetworkWorld, NetworkSystems> runtime, Container container);
    }

    /// <summary>Register implementations with Reflex as IViewStaticEcsFeature. Configure runs in binding order before world initialization.</summary>
    public interface IViewStaticEcsFeature
    {
        void Configure(StaticEcsRuntime<ViewWorld, ViewSystems> runtime, Container container);
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
        private StaticEcsRuntime<NetworkWorld, NetworkSystems> _network;
        private StaticEcsRuntime<ViewWorld, ViewSystems> _view;
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
                _network = new StaticEcsRuntime<NetworkWorld, NetworkSystems>();
                foreach (var feature in container.All<INetworkStaticEcsFeature>()) feature.Configure(_network, container);
                _view = new StaticEcsRuntime<ViewWorld, ViewSystems>();
                foreach (var feature in container.All<IViewStaticEcsFeature>()) feature.Configure(_view, container);
                _network.Initialize();
                _view.Initialize();
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

        public bool NetworkUpdateExternal { get; set; }

        public void Update()
        {
            Ensure();
            if (!NetworkUpdateExternal) _network.Update();
            _view.Update();
        }

        public void UpdateNetwork()
        {
            Ensure();
            _network.Update();
        }

        private void Ensure()
        {
            if (_disposed) throw new ObjectDisposedException(nameof(StaticEcsOwner));
            if (_network == null || _view == null) throw new InvalidOperationException("StaticEcs owner is not initialized.");
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
            try { _view?.Dispose(); }
            finally
            {
                _view = null;
                try { _network?.Dispose(); }
                finally { _network = null; }
            }
        }
    }

}
