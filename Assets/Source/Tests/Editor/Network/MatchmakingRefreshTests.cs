using System;
using System.Collections.Generic;
using System.Threading;
using Core.Application.Commands.Matchmaking;
using Core.Application.Handlers.Matchmaking;
using Core.Mediator.Abstractions;
using Core.Network.Abstractions.Services;
using Core.Network.Abstractions.Types;
using Core.Presentation.MVVM.Models.Lobby;
using Core.Presentation.MVVM.ViewModels.Matchmaking;
using Cysharp.Threading.Tasks;
using NUnit.Framework;
using R3;

namespace Core.Tests.Network
{
    public sealed class MatchmakingRefreshTests
    {
        [Test]
        public void SuccessfulRefreshRestoresModelStatus()
        {
            var lobby = new FakeLobby();
            var model = new LobbyModel();
            using var vm = new MatchmakingViewModel(model, new FakeMediator(lobby));
            vm.RefreshCommand.Execute(Unit.Default);
            Assert.That(vm.StatusText.Value, Is.EqualTo("Refreshing sessions..."));
            lobby.Pending.TrySetResult(NetworkStartResult.Success);
            Assert.That(vm.StatusText.Value, Is.EqualTo(model.StatusText));
            Assert.That(lobby.Calls, Is.EqualTo(1));
        }

        [Test]
        public void FailedRefreshReportsFailure()
        {
            var lobby = new FakeLobby();
            using var vm = new MatchmakingViewModel(new LobbyModel(), new FakeMediator(lobby));
            vm.RefreshCommand.Execute(Unit.Default);
            lobby.Pending.TrySetResult(NetworkStartResult.Failure("private", NetworkShutdownReason.InvalidArguments));
            Assert.That(vm.StatusText.Value, Is.EqualTo("Could not refresh sessions"));
        }

        [Test]
        public void RefreshWhileConnectingDoesNotJoinOrChangeStatus()
        {
            var lobby = new FakeLobby();
            var model = new LobbyModel();
            model.SetStatus("Connecting to room...", true);
            using var vm = new MatchmakingViewModel(model, new FakeMediator(lobby));
            vm.RefreshCommand.Execute(Unit.Default);
            Assert.That(lobby.Calls, Is.Zero);
            Assert.That(vm.StatusText.Value, Is.EqualTo("Connecting to room..."));
            Assert.That(vm.IsBusy.Value, Is.True);
        }

        [Test]
        public void OverlappingRefreshIsIgnoredAndNextRefreshIsAllowed()
        {
            var lobby = new FakeLobby();
            using var vm = new MatchmakingViewModel(new LobbyModel(), new FakeMediator(lobby));
            vm.RefreshCommand.Execute(Unit.Default);
            vm.RefreshCommand.Execute(Unit.Default);
            Assert.That(lobby.Calls, Is.EqualTo(1));
            Assert.That(vm.IsBusy.Value, Is.False);
            lobby.Pending.TrySetResult(NetworkStartResult.Success);
            vm.RefreshCommand.Execute(Unit.Default);
            Assert.That(lobby.Calls, Is.EqualTo(2));
            lobby.Pending.TrySetResult(NetworkStartResult.Success);
        }

        [Test]
        public void ExpectedFailureReleasesRefreshGuard()
        {
            var lobby = new FakeLobby();
            using var vm = new MatchmakingViewModel(new LobbyModel(), new FakeMediator(lobby));
            vm.RefreshCommand.Execute(Unit.Default);
            lobby.Pending.TrySetResult(NetworkStartResult.Failure("private", NetworkShutdownReason.InvalidArguments));
            vm.RefreshCommand.Execute(Unit.Default);
            Assert.That(lobby.Calls, Is.EqualTo(2));
            lobby.Pending.TrySetResult(NetworkStartResult.Success);
        }

        [Test]
        public void CancellationReleasesRefreshGuardAndRestoresStatus()
        {
            var lobby = new FakeLobby();
            var model = new LobbyModel();
            using var vm = new MatchmakingViewModel(model, new FakeMediator(lobby));
            Exception reported = null;
            void Capture(Exception exception) => reported = exception;
            var dispatch = UniTaskScheduler.DispatchUnityMainThread;
            var propagate = UniTaskScheduler.PropagateOperationCanceledException;
            UniTaskScheduler.DispatchUnityMainThread = false;
            UniTaskScheduler.PropagateOperationCanceledException = true;
            UniTaskScheduler.UnobservedTaskException += Capture;
            try
            {
                vm.RefreshCommand.Execute(Unit.Default);
                lobby.Pending.TrySetCanceled();
                Assert.That(reported, Is.InstanceOf<OperationCanceledException>());
                Assert.That(vm.StatusText.Value, Is.EqualTo(model.StatusText));
                vm.RefreshCommand.Execute(Unit.Default);
                Assert.That(lobby.Calls, Is.EqualTo(2));
                lobby.Pending.TrySetResult(NetworkStartResult.Success);
            }
            finally
            {
                UniTaskScheduler.UnobservedTaskException -= Capture;
                UniTaskScheduler.DispatchUnityMainThread = dispatch;
                UniTaskScheduler.PropagateOperationCanceledException = propagate;
            }
        }

        [Test]
        public void UnexpectedExceptionIsReportedAndReleasesRefreshGuard()
        {
            var lobby = new FakeLobby();
            using var vm = new MatchmakingViewModel(new LobbyModel(), new FakeMediator(lobby));
            var error = new InvalidOperationException("unexpected");
            Exception reported = null;
            void Capture(Exception exception) => reported = exception;
            var dispatch = UniTaskScheduler.DispatchUnityMainThread;
            UniTaskScheduler.DispatchUnityMainThread = false;
            UniTaskScheduler.UnobservedTaskException += Capture;
            try
            {
                vm.RefreshCommand.Execute(Unit.Default);
                lobby.Pending.TrySetException(error);
                Assert.That(reported, Is.SameAs(error));
                vm.RefreshCommand.Execute(Unit.Default);
                Assert.That(lobby.Calls, Is.EqualTo(2));
                lobby.Pending.TrySetResult(NetworkStartResult.Success);
            }
            finally
            {
                UniTaskScheduler.UnobservedTaskException -= Capture;
                UniTaskScheduler.DispatchUnityMainThread = dispatch;
            }
        }

        private sealed class FakeLobby : INetworkLobby
        {
            public int Calls;
            public UniTaskCompletionSource<NetworkStartResult> Pending;
            public bool IsInLobby => false;
            public IReadOnlyList<NetworkSessionInfo> Sessions => Array.Empty<NetworkSessionInfo>();
            public UniTask<NetworkStartResult> JoinAsync(string lobbyName = null, CancellationToken cancellationToken = default)
            {
                Calls++;
                Pending = new UniTaskCompletionSource<NetworkStartResult>();
                return Pending.Task;
            }
            public UniTask LeaveAsync(CancellationToken cancellationToken = default) => UniTask.CompletedTask;
        }

        private sealed class FakeMediator : IMediator
        {
            private readonly RefreshSessionListHandler _handler;
            public FakeMediator(INetworkLobby lobby) => _handler = new RefreshSessionListHandler(lobby);
            public UniTask<T> Send<T>(IRequest<T> request, CancellationToken cancellationToken = default)
            {
                if (request is RefreshSessionListCommand command && typeof(T) == typeof(Nothing))
                    return (UniTask<T>)(object)_handler.Handle(command, cancellationToken);
                throw new NotSupportedException();
            }
            public UniTask Send(IRequest request, CancellationToken cancellationToken = default) => throw new NotSupportedException();
            public IUniTaskAsyncEnumerable<T> CreateStream<T>(IStreamRequest<T> request, CancellationToken cancellationToken = default) => throw new NotSupportedException();
            public UniTask Publish<T>(T notification, CancellationToken cancellationToken = default) where T : INotification => throw new NotSupportedException();
            public Observable<T> CreateNotificationObservable<T>() where T : INotification => throw new NotSupportedException();
        }
    }
}