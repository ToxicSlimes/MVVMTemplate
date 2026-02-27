using System;
using Core.Abstractions.MVVM.Models.Counter;
using Core.Abstractions.MVVM.ViewModels;
using Core.Application.Commands.Counter;
using Core.Infrastructure.Mediator.Abstractions;
using Core.Infrastructure.Mediator.Attributes;
using Cysharp.Threading.Tasks;
using R3;
using UnityEngine;

namespace Core.MVVM.ViewModels.Counter
{
    /// <summary>
    /// TEMPLATE EXAMPLE: ViewModel — the bridge between Model and View.
    /// 
    /// Key patterns demonstrated:
    /// 1. ReactiveProperty — View subscribes to changes automatically
    /// 2. ReactiveCommand — View binds buttons to commands
    /// 3. Model → ViewModel sync via events
    /// 4. ViewModel → Model sync via Mediator Commands (CQRS)
    /// 5. Proper disposal chain
    /// </summary>
    [GenerateViewCommands]
    public class CounterViewModel : DisposableObject
    {
        private readonly ICounterModel _model;
        private readonly IMediator _mediator;

        // Reactive properties — View auto-subscribes to these
        public ReactiveProperty<int> CounterValue { get; }
        public ReactiveProperty<int> TotalIncrements { get; }
        public ReactiveProperty<string> DisplayText { get; }

        // Commands — View binds buttons to these
        public ReactiveCommand<Unit> IncrementCommand { get; set; }
        public ReactiveCommand<Unit> DecrementCommand { get; set; }
        public ReactiveCommand<Unit> ResetCommand { get; set; }

        public CounterViewModel(ICounterModel model, IMediator mediator)
        {
            _model = model ?? throw new ArgumentNullException(nameof(model));
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));

            CounterValue = new ReactiveProperty<int>(_model.Value);
            TotalIncrements = new ReactiveProperty<int>(_model.TotalIncrements);
            DisplayText = new ReactiveProperty<string>(FormatDisplay(_model.Value));

            SetupModelSubscriptions();
            SetupCommands();
        }

        private void SetupModelSubscriptions()
        {
            _model.ValueChanged += OnModelValueChanged;
        }

        private void SetupCommands()
        {
            IncrementCommand = new ReactiveCommand<Unit>(_ => OnIncrement().Forget());
            IncrementCommand.AddTo(ref Disposables);

            DecrementCommand = new ReactiveCommand<Unit>(_ => OnDecrement().Forget());
            DecrementCommand.AddTo(ref Disposables);

            ResetCommand = new ReactiveCommand<Unit>(_ => OnReset().Forget());
            ResetCommand.AddTo(ref Disposables);
        }

        private void OnModelValueChanged(int oldValue, int newValue)
        {
            CounterValue.Value = newValue;
            TotalIncrements.Value = _model.TotalIncrements;
            DisplayText.Value = FormatDisplay(newValue);
        }

        private async UniTaskVoid OnIncrement()
        {
            await _mediator.Send(new IncrementCounterCommand(1));
        }

        private async UniTaskVoid OnDecrement()
        {
            await _mediator.Send(new IncrementCounterCommand(-1));
        }

        private async UniTaskVoid OnReset()
        {
            await _mediator.Send(new ResetCounterCommand());
        }

        private static string FormatDisplay(int value) => $"Count: {value}";

        public override void Dispose()
        {
            _model.ValueChanged -= OnModelValueChanged;
            base.Dispose();
        }
    }
}
