using System;
using System.Threading;
using R3;

namespace Core.Abstractions.MVVM.ViewModels
{
    public class DisposableObject : IViewModel
    {
        private DisposableBag _disposables;
        private CancellationTokenSource _lifetimeCts = new();
        protected CancellationTokenSource _requestCts;

        public bool IsDisposed { get; private set; }

        protected CancellationToken LifetimeToken => _lifetimeCts.Token;
        protected ref DisposableBag Disposables => ref _disposables;

        protected void SafeCancelAndDisposeRequestCts()
        {
            if (_requestCts != null)
            {
                try
                {
                    _requestCts.Cancel();
                }
                catch (ObjectDisposedException)
                {
                }
                _requestCts.Dispose();
                _requestCts = null;
            }
        }

        protected void CancelRequestCts()
        {
            if (_requestCts != null)
            {
                try
                {
                    _requestCts.Cancel();
                }
                catch (ObjectDisposedException)
                {
                }
            }
        }

        protected CancellationTokenSource CreateNewRequestCts()
        {
            SafeCancelAndDisposeRequestCts();
            _requestCts = new CancellationTokenSource();
            return _requestCts;
        }

        protected static Observable<Unit> ObserveEvent(
            Action<Action> addHandler,
            Action<Action> removeHandler)
        {
            return Observable.FromEvent(h => h, addHandler, removeHandler);
        }

        protected static Observable<T> ObserveEvent<T>(
            Action<Action<T>> addHandler,
            Action<Action<T>> removeHandler)
        {
            return Observable.FromEvent<Action<T>, T>(h => h, addHandler, removeHandler);
        }

        public virtual void Dispose()
        {
            if (IsDisposed) return;
            IsDisposed = true;
            _lifetimeCts.Cancel();
            _lifetimeCts.Dispose();
            SafeCancelAndDisposeRequestCts();
            _disposables.Dispose();
        }
    }
}
