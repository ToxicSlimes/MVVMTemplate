using System;
using System.Collections.Generic;
using System.Threading;
using Core.Application.Commands.Matchmaking;
using Core.Application.Handlers.Matchmaking;
using Core.Network.Abstractions.Services;
using Core.Network.Abstractions.Types;
using Cysharp.Threading.Tasks;
using NUnit.Framework;

namespace Core.Tests.Network
{
    public sealed class RefreshSessionListHandlerTests
    {
        [Test]
        public void FailedJoinDoesNotReportSuccessOrExposeNetworkDetails()
        {
            var lobby = new FakeLobby { Result = NetworkStartResult.Failure("private network detail", NetworkShutdownReason.InvalidArguments) };
            var error = Assert.Catch<InvalidOperationException>(() => new RefreshSessionListHandler(lobby).Handle(new RefreshSessionListCommand(), CancellationToken.None).GetAwaiter().GetResult());
            Assert.That(error.Message, Does.Not.Contain("private network detail"));
        }

        [Test]
        public void SuccessfulJoinCompletesAndForwardsCancellationToken()
        {
            using var source = new CancellationTokenSource();
            var lobby = new FakeLobby { Result = NetworkStartResult.Success };
            new RefreshSessionListHandler(lobby).Handle(new RefreshSessionListCommand(), source.Token).GetAwaiter().GetResult();
            Assert.That(lobby.Token, Is.EqualTo(source.Token));
        }

        [Test]
        public void CancellationPropagates()
        {
            var lobby = new FakeLobby { Error = new OperationCanceledException() };
            Assert.Catch<OperationCanceledException>(() => new RefreshSessionListHandler(lobby).Handle(new RefreshSessionListCommand(), CancellationToken.None).GetAwaiter().GetResult());
        }

        [Test]
        public void UnexpectedExceptionPropagates()
        {
            var error = new InvalidOperationException("unexpected");
            var lobby = new FakeLobby { Error = error };
            Assert.That(Assert.Catch<InvalidOperationException>(() => new RefreshSessionListHandler(lobby).Handle(new RefreshSessionListCommand(), CancellationToken.None).GetAwaiter().GetResult()), Is.SameAs(error));
        }

        private sealed class FakeLobby : INetworkLobby
        {
            public NetworkStartResult Result;
            public Exception Error;
            public CancellationToken Token;
            public bool IsInLobby => false;
            public IReadOnlyList<NetworkSessionInfo> Sessions => Array.Empty<NetworkSessionInfo>();
            public UniTask<NetworkStartResult> JoinAsync(string lobbyName = null, CancellationToken cancellationToken = default)
            {
                Token = cancellationToken;
                return Error == null ? UniTask.FromResult(Result) : UniTask.FromException<NetworkStartResult>(Error);
            }
            public UniTask LeaveAsync(CancellationToken cancellationToken = default) => UniTask.CompletedTask;
        }
    }
}
