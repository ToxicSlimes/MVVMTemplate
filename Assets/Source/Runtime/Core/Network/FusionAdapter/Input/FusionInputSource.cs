using System;
using System.Collections.Generic;
using Core.Network.Abstractions.Input;
using Core.Network.Abstractions.Services;
using Core.Network.FusionAdapter.Callbacks;
using Core.Network.FusionAdapter.Runner;
using Fusion;

namespace Core.Network.FusionAdapter.Input
{
    /// <summary>
    /// Прокидывает структуру ввода локального игрока в Fusion.
    /// Контракт <see cref="INetworkInput"/> проверяется один раз при регистрации,
    /// в тике работает типизированный writer без рефлексии.
    /// </summary>
    public sealed class FusionInputSource : FusionCallbacksBase, INetworkInputSource, IDisposable
    {
        private readonly INetworkRunnerAccessor _accessor;
        private readonly List<PlayerRef> _players = new();

        private IFusionInputWriter _writer;

        public FusionInputSource(INetworkRunnerAccessor accessor)
        {
            _accessor = accessor;
            _accessor.RegisterCallbacks(this);
        }

        /// <summary>
        /// Когда ввод игрока не доехал, state authority переприменяет последний известный.
        /// Отключи, если игре нужен явный «пустой» ввод вместо повтора.
        /// </summary>
        public bool RepeatLastInputOnMissing { get; set; } = true;

        /// <summary>
        /// Единственный путь регистрации: generic-тип инстанцируется в вызывающем коде,
        /// поэтому IL2CPP видит инстанциацию статически. Рефлексивной перегрузки нет
        /// намеренно — она собиралась бы под AOT и падала в рантайме.
        /// </summary>
        public void Register(IFusionInputWriter writer)
        {
            if (writer == null)
            {
                throw new ArgumentNullException(nameof(writer));
            }

            if (_writer != null && _writer.InputType != writer.InputType)
            {
                throw new InvalidOperationException(
                    $"Ввод '{_writer.InputType.FullName}' уже зарегистрирован. Fusion допускает одну структуру ввода " +
                    $"на игрока — вызови Unregister перед регистрацией '{writer.InputType.FullName}'");
            }

            _writer = writer;
        }

        public void Unregister<TInput>() where TInput : struct
        {
            if (_writer == null || _writer.InputType != typeof(TInput))
            {
                return;
            }

            _writer.Dispose();
            _writer = null;
        }

        public void Dispose()
        {
            _writer?.Dispose();
            _writer = null;
        }

        protected override void OnInput(NetworkRunner runner, NetworkInput input)
        {
            _writer?.Write(input);
            RefreshSnapshots(runner, PlayerRef.None);
        }

        protected override void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input)
        {
            if (_writer == null || !RepeatLastInputOnMissing)
            {
                return;
            }

            _writer.TryWriteCached(input, player);
            RefreshSnapshots(runner, player);
        }

        protected override void OnPlayerJoined(NetworkRunner runner, PlayerRef player)
        {
            RebuildPlayers(runner);
        }

        protected override void OnPlayerLeft(NetworkRunner runner, PlayerRef player)
        {
            RebuildPlayers(runner);
            _writer?.Forget(player);
        }

        protected override void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason)
        {
            _players.Clear();
            _writer?.ClearCache();
        }

        /// <summary>
        /// Кэш нужен только тому, кто получает OnInputMissing, то есть state authority.
        /// Клиенты цикл не выполняют. Список игроков пересобирается на join/leave,
        /// чтобы не аллоцировать энумератор ActivePlayers каждый тик.
        /// </summary>
        private void RefreshSnapshots(NetworkRunner runner, PlayerRef skip)
        {
            if (_writer == null || !RepeatLastInputOnMissing)
            {
                return;
            }

            if (!runner.IsServer && !runner.IsSharedModeMasterClient)
            {
                return;
            }

            for (var i = 0; i < _players.Count; i++)
            {
                if (_players[i] == skip)
                {
                    continue;
                }

                _writer.Snapshot(runner, _players[i]);
            }
        }

        private void RebuildPlayers(NetworkRunner runner)
        {
            _players.Clear();

            foreach (var player in runner.ActivePlayers)
            {
                _players.Add(player);
            }
        }
    }
}
