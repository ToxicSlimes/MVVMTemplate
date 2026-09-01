using System;
using Core.Abstractions.MVVM.Models.Counter;
using Core.Abstractions.MVVM.ViewModels;
using Core.Application.Commands.Common;
using Core.Application.Commands.Counter;
using Core.Infrastructure.Mediator.Abstractions;
using Core.Infrastructure.Mediator.Attributes;
using Cysharp.Threading.Tasks;
using R3;
using UnityEngine;

namespace Core.MVVM.ViewModels.Counter
{
    [GenerateViewCommands]
    public class CounterViewModel : DisposableObject
    {
        private readonly ICounterModel _model;
        private readonly IMediator _mediator;

        // Counter
        public ReactiveProperty<int> CounterValue { get; }
        public ReactiveProperty<int> TotalIncrements { get; }
        public ReactiveProperty<string> DisplayText { get; }

        // Mode toggle
        public ReactiveProperty<bool> IsGalleryMode { get; }
        public ReactiveProperty<string> ToggleButtonText { get; }

        // Commands
        public ReactiveCommand<Unit> IncrementCommand { get; set; }
        public ReactiveCommand<Unit> DecrementCommand { get; set; }
        public ReactiveCommand<Unit> ResetCommand { get; set; }
        public ReactiveCommand<Unit> ToggleGalleryCommand { get; set; }
        public ReactiveCommand<Unit> BackCommand { get; set; }

        public CounterViewModel(ICounterModel model, IMediator mediator)
        {
            _model = model ?? throw new ArgumentNullException(nameof(model));
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));

            CounterValue = new ReactiveProperty<int>(_model.Value);
            TotalIncrements = new ReactiveProperty<int>(_model.TotalIncrements);
            DisplayText = new ReactiveProperty<string>(FormatDisplay(_model.Value));
            IsGalleryMode = new ReactiveProperty<bool>(false);
            ToggleButtonText = new ReactiveProperty<string>("Show Elements");

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

            ToggleGalleryCommand = new ReactiveCommand<Unit>(_ =>
            {
                IsGalleryMode.Value = !IsGalleryMode.Value;
                ToggleButtonText.Value = IsGalleryMode.Value ? "Show Counter" : "Show Elements";
            });
            ToggleGalleryCommand.AddTo(ref Disposables);

            BackCommand = new ReactiveCommand<Unit>(_ => OnBack().Forget());
            BackCommand.AddTo(ref Disposables);
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

        private async UniTaskVoid OnBack()
        {
            await _mediator.Send(new NavigateBackCommand());
        }

        private static string FormatDisplay(int value) => $"Count: {value}";

        public override void Dispose()
        {
            _model.ValueChanged -= OnModelValueChanged;
            base.Dispose();
        }
    }
}
