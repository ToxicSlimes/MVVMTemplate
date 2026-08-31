using System.Collections.Generic;
using System.Threading;
using Core.Network.Abstractions.Types;
using Cysharp.Threading.Tasks;

namespace Core.Network.Abstractions.Services
{
    /// <summary>
    /// Обзор доступных сессий до входа в игру. Живёт отдельно от <see cref="INetworkRunnerController"/>
    /// и держит собственное подключение: лобби и сессия — разные жизненные циклы, и попытка
    /// обслужить оба одним подключением не подтверждена поставкой Fusion.
    /// </summary>
    public interface INetworkLobby
    {
        bool IsInLobby { get; }

        IReadOnlyList<NetworkSessionInfo> Sessions { get; }

        UniTask<NetworkStartResult> JoinAsync(string lobbyName = null, CancellationToken cancellationToken = default);

        UniTask LeaveAsync(CancellationToken cancellationToken = default);
    }
}
