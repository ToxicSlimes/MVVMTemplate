using System;
using Core.Abstractions.MVVM.Models.Settings;
using Core.Abstractions.MVVM.ViewModels;
using Core.Application.Commands.Settings;
using Core.Application.Queries.Settings;
using Core.Data.Domain.Enums;
using Core.Infrastructure.Mediator.Abstractions;
using Core.Infrastructure.Mediator.Attributes;
using Cysharp.Threading.Tasks;
using R3;
using Core.Application.Requests.Views;
using Core.MVVM.Views.Settings;

namespace Core.MVVM.ViewModels.Settings
{
    [GenerateViewCommands]
    public class SettingsViewModel : DisposableObject, IVisibilityAware
    {
        private readonly ISettingsModel _model;
        private readonly IMediator _mediator;
        private bool _isInitialized;

        public ReactiveProperty<bool> IsVisible { get; }
        public ReactiveProperty<SettingsTab> CurrentTab { get; }

        public ReactiveProperty<float> MasterVolume { get; }
        public ReactiveProperty<float> MusicVolume { get; }
        public ReactiveProperty<float> SfxVolume { get; }
        public ReactiveProperty<float> VoiceVolume { get; }
        public ReactiveProperty<bool> IsMuteAll { get; }
        public ReactiveProperty<bool> IsMuteMusic { get; }
        public ReactiveProperty<bool> IsMuteSfx { get; }

        public ReactiveProperty<int> ResolutionIndex { get; }
        public ReactiveProperty<QualityPreset> Quality { get; }
        public ReactiveProperty<bool> IsFullscreen { get; }
        public ReactiveProperty<bool> IsVSync { get; }
        public ReactiveProperty<bool> ShowFps { get; }
        public ReactiveProperty<TargetFpsOption> TargetFps { get; }

        public ReactiveProperty<float> Brightness { get; }
        public ReactiveProperty<float> Gamma { get; }
        public ReactiveProperty<float> UiScale { get; }
        public ReactiveProperty<bool> IsColorblindMode { get; }
        public ReactiveProperty<ColorblindType> ColorblindType { get; }

        public ReactiveProperty<float> MouseSensitivity { get; }
        public ReactiveProperty<bool> InvertYAxis { get; }

        public ReactiveProperty<GameLanguage> Language { get; }
        public ReactiveProperty<bool> IsAutoSave { get; }
        public ReactiveProperty<bool> ShowTutorials { get; }
        public ReactiveProperty<bool> ScreenShake { get; }
        public ReactiveProperty<bool> MatureContent { get; }

        public ReactiveCommand<Unit> ToggleCommand { get; set; }
        public ReactiveCommand<Unit> CloseCommand { get; set; }
        public ReactiveCommand<Unit> ApplyCommand { get; set; }
        public ReactiveCommand<Unit> ResetCommand { get; set; }
        public ReactiveCommand<Unit> CancelCommand { get; set; }
        public ReactiveCommand<SettingsTab> SelectTabCommand { get; set; }
        public ReactiveCommand<Unit> NextTabCommand { get; set; }
        public ReactiveCommand<Unit> PreviousTabCommand { get; set; }

        public SettingsViewModel(ISettingsModel model, IMediator mediator)
        {
            _model = model ?? throw new ArgumentNullException(nameof(model));
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));

            _model.LoadFromPlayerPrefs();

            IsVisible = new ReactiveProperty<bool>(_model.IsVisible);
            CurrentTab = new ReactiveProperty<SettingsTab>(_model.CurrentTab);

            MasterVolume = new ReactiveProperty<float>(_model.Audio.MasterVolume);
            MusicVolume = new ReactiveProperty<float>(_model.Audio.MusicVolume);
            SfxVolume = new ReactiveProperty<float>(_model.Audio.SfxVolume);
            VoiceVolume = new ReactiveProperty<float>(_model.Audio.VoiceVolume);
            IsMuteAll = new ReactiveProperty<bool>(_model.Audio.IsMuteAll);
            IsMuteMusic = new ReactiveProperty<bool>(_model.Audio.IsMuteMusic);
            IsMuteSfx = new ReactiveProperty<bool>(_model.Audio.IsMuteSfx);

            ResolutionIndex = new ReactiveProperty<int>(_model.Video.ResolutionIndex);
            Quality = new ReactiveProperty<QualityPreset>(_model.Video.Quality);
            IsFullscreen = new ReactiveProperty<bool>(_model.Video.IsFullscreen);
            IsVSync = new ReactiveProperty<bool>(_model.Video.IsVSync);
            ShowFps = new ReactiveProperty<bool>(_model.Video.ShowFps);
            TargetFps = new ReactiveProperty<TargetFpsOption>(_model.Video.TargetFps);

            Brightness = new ReactiveProperty<float>(_model.Display.Brightness);
            Gamma = new ReactiveProperty<float>(_model.Display.Gamma);
            UiScale = new ReactiveProperty<float>(_model.Display.UiScale);
            IsColorblindMode = new ReactiveProperty<bool>(_model.Display.IsColorblindMode);
            ColorblindType = new ReactiveProperty<ColorblindType>(_model.Display.ColorblindType);

            MouseSensitivity = new ReactiveProperty<float>(_model.Controls.MouseSensitivity);
            InvertYAxis = new ReactiveProperty<bool>(_model.Controls.InvertYAxis);

            Language = new ReactiveProperty<GameLanguage>(_model.Game.Language);
            IsAutoSave = new ReactiveProperty<bool>(_model.Game.IsAutoSave);
            ShowTutorials = new ReactiveProperty<bool>(_model.Game.ShowTutorials);
            ScreenShake = new ReactiveProperty<bool>(_model.Game.ScreenShake);
            MatureContent = new ReactiveProperty<bool>(_model.Game.MatureContent);

            SetupModelSubscriptions();
            SetupViewModelSubscriptions();
            SetupCommands();
            LoadAudioStateAsync().Forget();
        }

        private async UniTaskVoid LoadAudioStateAsync()
        {
            try
            {
                var state = await _mediator.Send(new GetAudioStateQuery());

                _model.Audio.MasterVolume = state.MasterVolume;
                _model.Audio.MusicVolume = state.MusicVolume;
                _model.Audio.SfxVolume = state.SfxVolume;
                _model.Audio.IsMuteAll = state.IsMasterMuted;
                _model.Audio.IsMuteMusic = state.IsMusicMuted;
                _model.Audio.IsMuteSfx = state.IsSfxMuted;

                _isInitialized = true;
            }
            catch (Exception)
            {
                _isInitialized = true;
            }
        }

        private void SetupModelSubscriptions()
        {
            _model.IsVisibleChanged += OnModelIsVisibleChanged;
            _model.CurrentTabChanged += OnModelCurrentTabChanged;
            _model.Audio.AudioSettingsChanged += OnAudioSettingsChanged;
            _model.Video.VideoSettingsChanged += OnVideoSettingsChanged;
            _model.Display.DisplaySettingsChanged += OnDisplaySettingsChanged;
            _model.Controls.ControlsSettingsChanged += OnControlsSettingsChanged;
            _model.Game.GameSettingsChanged += OnGameSettingsChanged;
        }

        private void SetupViewModelSubscriptions()
        {
            IsVisible.Subscribe(this, static (v, s) => s._model.IsVisible = v).AddTo(ref Disposables);
            CurrentTab.Subscribe(this, static (v, s) => s._model.CurrentTab = v).AddTo(ref Disposables);

            MasterVolume.Skip(1).Subscribe(this, static (v, s) =>
            {
                s._model.Audio.MasterVolume = v;
                if (s._isInitialized)
                    s._mediator.Send(new SetAudioVolumeCommand(AudioChannel.Master, v)).Forget();
            }).AddTo(ref Disposables);

            MusicVolume.Skip(1).Subscribe(this, static (v, s) =>
            {
                s._model.Audio.MusicVolume = v;
                if (s._isInitialized)
                    s._mediator.Send(new SetAudioVolumeCommand(AudioChannel.Music, v)).Forget();
            }).AddTo(ref Disposables);

            SfxVolume.Skip(1).Subscribe(this, static (v, s) =>
            {
                s._model.Audio.SfxVolume = v;
                if (s._isInitialized)
                    s._mediator.Send(new SetAudioVolumeCommand(AudioChannel.Sfx, v)).Forget();
            }).AddTo(ref Disposables);

            VoiceVolume.Skip(1).Subscribe(this, static (v, s) =>
            {
                s._model.Audio.VoiceVolume = v;
            }).AddTo(ref Disposables);

            IsMuteAll.Skip(1).Subscribe(this, static (v, s) =>
            {
                s._model.Audio.IsMuteAll = v;
                if (s._isInitialized)
                    s._mediator.Send(new SetAudioMuteCommand(AudioChannel.Master, v)).Forget();
            }).AddTo(ref Disposables);

            IsMuteMusic.Skip(1).Subscribe(this, static (v, s) =>
            {
                s._model.Audio.IsMuteMusic = v;
                if (s._isInitialized)
                    s._mediator.Send(new SetAudioMuteCommand(AudioChannel.Music, v)).Forget();
            }).AddTo(ref Disposables);

            IsMuteSfx.Skip(1).Subscribe(this, static (v, s) =>
            {
                s._model.Audio.IsMuteSfx = v;
                if (s._isInitialized)
                    s._mediator.Send(new SetAudioMuteCommand(AudioChannel.Sfx, v)).Forget();
            }).AddTo(ref Disposables);

            ResolutionIndex.Subscribe(this, static (v, s) => s._model.Video.ResolutionIndex = v).AddTo(ref Disposables);
            Quality.Subscribe(this, static (v, s) => s._model.Video.Quality = v).AddTo(ref Disposables);
            IsFullscreen.Subscribe(this, static (v, s) => s._model.Video.IsFullscreen = v).AddTo(ref Disposables);
            IsVSync.Subscribe(this, static (v, s) => s._model.Video.IsVSync = v).AddTo(ref Disposables);
            ShowFps.Subscribe(this, static (v, s) => s._model.Video.ShowFps = v).AddTo(ref Disposables);
            TargetFps.Subscribe(this, static (v, s) => s._model.Video.TargetFps = v).AddTo(ref Disposables);

            Brightness.Subscribe(this, static (v, s) => s._model.Display.Brightness = v).AddTo(ref Disposables);
            Gamma.Subscribe(this, static (v, s) => s._model.Display.Gamma = v).AddTo(ref Disposables);
            UiScale.Subscribe(this, static (v, s) => s._model.Display.UiScale = v).AddTo(ref Disposables);
            IsColorblindMode.Subscribe(this, static (v, s) => s._model.Display.IsColorblindMode = v).AddTo(ref Disposables);
            ColorblindType.Subscribe(this, static (v, s) => s._model.Display.ColorblindType = v).AddTo(ref Disposables);

            MouseSensitivity.Subscribe(this, static (v, s) => s._model.Controls.MouseSensitivity = v).AddTo(ref Disposables);
            InvertYAxis.Subscribe(this, static (v, s) => s._model.Controls.InvertYAxis = v).AddTo(ref Disposables);

            Language.Subscribe(this, static (v, s) => s._model.Game.Language = v).AddTo(ref Disposables);
            IsAutoSave.Subscribe(this, static (v, s) => s._model.Game.IsAutoSave = v).AddTo(ref Disposables);
            ShowTutorials.Subscribe(this, static (v, s) => s._model.Game.ShowTutorials = v).AddTo(ref Disposables);
            ScreenShake.Subscribe(this, static (v, s) => s._model.Game.ScreenShake = v).AddTo(ref Disposables);
            MatureContent.Subscribe(this, static (v, s) => s._model.Game.MatureContent = v).AddTo(ref Disposables);
        }

        private void SetupCommands()
        {
            ToggleCommand = new ReactiveCommand<Unit>(_ => Toggle());
            ToggleCommand.AddTo(ref Disposables);

            CloseCommand = new ReactiveCommand<Unit>(_ => Close().Forget());
            CloseCommand.AddTo(ref Disposables);

            ApplyCommand = new ReactiveCommand<Unit>(_ => Apply());
            ApplyCommand.AddTo(ref Disposables);

            ResetCommand = new ReactiveCommand<Unit>(_ => ResetToDefaults());
            ResetCommand.AddTo(ref Disposables);

            CancelCommand = new ReactiveCommand<Unit>(_ => Cancel().Forget());
            CancelCommand.AddTo(ref Disposables);

            SelectTabCommand = new ReactiveCommand<SettingsTab>(SelectTab);
            SelectTabCommand.AddTo(ref Disposables);

            NextTabCommand = new ReactiveCommand<Unit>(_ => NavigateTab(1));
            NextTabCommand.AddTo(ref Disposables);

            PreviousTabCommand = new ReactiveCommand<Unit>(_ => NavigateTab(-1));
            PreviousTabCommand.AddTo(ref Disposables);
        }

        private void OnModelIsVisibleChanged(bool value) => IsVisible.Value = value;
        private void OnModelCurrentTabChanged(SettingsTab value) => CurrentTab.Value = value;

        private void OnAudioSettingsChanged()
        {
            MasterVolume.Value = _model.Audio.MasterVolume;
            MusicVolume.Value = _model.Audio.MusicVolume;
            SfxVolume.Value = _model.Audio.SfxVolume;
            VoiceVolume.Value = _model.Audio.VoiceVolume;
            IsMuteAll.Value = _model.Audio.IsMuteAll;
            IsMuteMusic.Value = _model.Audio.IsMuteMusic;
            IsMuteSfx.Value = _model.Audio.IsMuteSfx;
        }

        private void OnVideoSettingsChanged()
        {
            ResolutionIndex.Value = _model.Video.ResolutionIndex;
            Quality.Value = _model.Video.Quality;
            IsFullscreen.Value = _model.Video.IsFullscreen;
            IsVSync.Value = _model.Video.IsVSync;
            ShowFps.Value = _model.Video.ShowFps;
            TargetFps.Value = _model.Video.TargetFps;
        }

        private void OnDisplaySettingsChanged()
        {
            Brightness.Value = _model.Display.Brightness;
            Gamma.Value = _model.Display.Gamma;
            UiScale.Value = _model.Display.UiScale;
            IsColorblindMode.Value = _model.Display.IsColorblindMode;
            ColorblindType.Value = _model.Display.ColorblindType;
        }

        private void OnControlsSettingsChanged()
        {
            MouseSensitivity.Value = _model.Controls.MouseSensitivity;
            InvertYAxis.Value = _model.Controls.InvertYAxis;
        }

        private void OnGameSettingsChanged()
        {
            Language.Value = _model.Game.Language;
            IsAutoSave.Value = _model.Game.IsAutoSave;
            ShowTutorials.Value = _model.Game.ShowTutorials;
            ScreenShake.Value = _model.Game.ScreenShake;
            MatureContent.Value = _model.Game.MatureContent;
        }

        private void Toggle() => _model.IsVisible = !_model.IsVisible;

        private async UniTaskVoid Close()
        {
            _model.IsVisible = false;
            await _mediator.Send(new HideViewRequest<SettingsView, SettingsViewModel>());
        }

        private void Apply() => _model.Apply();

        private void ResetToDefaults()
        {
            _model.ResetToDefaults();
            _mediator.Send(new ResetAudioCommand()).Forget();
            LoadAudioStateAsync().Forget();
        }

        private async UniTaskVoid Cancel()
        {
            _model.LoadFromPlayerPrefs();
            LoadAudioStateAsync().Forget();
            _model.IsVisible = false;
            await _mediator.Send(new HideViewRequest<SettingsView, SettingsViewModel>());
        }

        private void SelectTab(SettingsTab tab) => _model.CurrentTab = tab;

        private void NavigateTab(int direction)
        {
            const int tabCount = 5;
            var currentIndex = (int)_model.CurrentTab;
            var newIndex = (currentIndex + direction + tabCount) % tabCount;
            _model.CurrentTab = (SettingsTab)newIndex;
        }

        public override void Dispose()
        {
            _model.IsVisibleChanged -= OnModelIsVisibleChanged;
            _model.CurrentTabChanged -= OnModelCurrentTabChanged;
            if (_model.Audio != null) _model.Audio.AudioSettingsChanged -= OnAudioSettingsChanged;
            _model.Video.VideoSettingsChanged -= OnVideoSettingsChanged;
            _model.Display.DisplaySettingsChanged -= OnDisplaySettingsChanged;
            _model.Controls.ControlsSettingsChanged -= OnControlsSettingsChanged;
            _model.Game.GameSettingsChanged -= OnGameSettingsChanged;
            _model.Dispose();
            base.Dispose();
        }
    }
}
