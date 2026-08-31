using System;
using System.Collections.Generic;
using Core.Network.Abstractions.Input;
using Fusion;

namespace Core.Network.FusionAdapter.Input
{
    /// <summary>
    /// Типизированный мост между Fusion-free коллектором и <see cref="NetworkInput"/>.
    /// Хранит последний известный ввод каждого игрока, чтобы state authority мог
    /// переприменить его, когда пакет не доехал.
    /// Создавай этот тип явно, если собираешь под IL2CPP: reflection-путь в
    /// <see cref="FusionInputSource"/> не даёт AOT сгенерировать инстанциацию для value-type.
    /// </summary>
    public sealed class FusionInputWriter<TInput> : IFusionInputWriter
        where TInput : unmanaged, INetworkInput
    {
        private readonly INetworkInputCollector<TInput> _collector;
        private readonly Dictionary<int, TInput> _lastByPlayer = new();

        public FusionInputWriter(INetworkInputCollector<TInput> collector)
        {
            _collector = collector ?? throw new ArgumentNullException(nameof(collector));
        }

        public Type InputType => typeof(TInput);

        public void Write(NetworkInput input)
        {
            if (_collector.TryCollect(out var value))
            {
                input.Set(value);
            }
        }

        public void Snapshot(NetworkRunner runner, PlayerRef player)
        {
            if (runner.TryGetInputForPlayer(player, out TInput value))
            {
                _lastByPlayer[player.RawEncoded] = value;
            }
        }

        public bool TryWriteCached(NetworkInput input, PlayerRef player)
        {
            if (!_lastByPlayer.TryGetValue(player.RawEncoded, out var value))
            {
                return false;
            }

            input.Set(value);
            return true;
        }

        public void Forget(PlayerRef player) => _lastByPlayer.Remove(player.RawEncoded);

        public void ClearCache() => _lastByPlayer.Clear();

        /// <summary>
        /// Коллектор может держать подписки на сервис ввода, а собрал его вызывающий код —
        /// освободить сам он не может. Writer владеет коллектором, поэтому цепочку закрывает он.
        /// </summary>
        public void Dispose()
        {
            _lastByPlayer.Clear();
            (_collector as IDisposable)?.Dispose();
        }
    }
}
