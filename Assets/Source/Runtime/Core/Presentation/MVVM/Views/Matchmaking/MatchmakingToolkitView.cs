using System.Collections.Generic;
using Core.Data.Domain.Models.Lobby;
using Core.MVVM.ViewModels.Matchmaking;
using Core.UI.Toolkit;
using R3;
using UnityEngine.UIElements;

namespace Core.MVVM.Views.Matchmaking
{
    public class MatchmakingToolkitView : ToolkitPanelView<MatchmakingViewModel>
    {
        private Button _backButton;
        private TextField _nicknameField;
        private TextField _codeField;
        private Button _createButton;
        private Button _joinButton;
        private Button _refreshButton;
        private Label _statusLabel;
        private ScrollView _sessionsScroll;
        private readonly List<Button> _sessionButtons = new();

        protected override void BindPanel(ref DisposableBag disposables)
        {
            QueryElements();
            Bind(_backButton, ViewModel.BackCommand, ref disposables);
            Bind(_nicknameField, ViewModel.Nickname, ref disposables);
            Bind(_codeField, ViewModel.RoomCodeInput, ref disposables);
            Bind(_createButton, ViewModel.CreateCommand, ref disposables);
            Bind(_joinButton, ViewModel.JoinCommand, ref disposables);
            Bind(_refreshButton, ViewModel.RefreshCommand, ref disposables);
            Bind(_statusLabel, ViewModel.StatusText, ref disposables);
            ViewModel.SessionsUpdated
                .Subscribe(this, static (_, self) => self.RebuildSessions())
                .AddTo(ref disposables);
            RebuildSessions();
        }

        private void QueryElements()
        {
            if (Root == null) return;
            _backButton = Root.Q<Button>("back-button");
            _nicknameField = Root.Q<TextField>("nickname-field");
            _codeField = Root.Q<TextField>("code-field");
            _createButton = Root.Q<Button>("create-button");
            _joinButton = Root.Q<Button>("join-button");
            _refreshButton = Root.Q<Button>("refresh-button");
            _statusLabel = Root.Q<Label>("status-label");
            _sessionsScroll = Root.Q<ScrollView>("sessions-scroll");
        }

        private void RebuildSessions()
        {
            if (_sessionsScroll == null) return;

            var container = _sessionsScroll.contentContainer;
            container.Clear();
            _sessionButtons.Clear();

            var sessions = ViewModel.Sessions;
            for (var i = 0; i < sessions.Count; i++)
            {
                var index = i;
                var session = sessions[i];
                var button = new Button(() => ViewModel.JoinSession(session.RoomCode))
                {
                    text = FormatSession(session),
                    name = $"session-row-{i}"
                };
                button.AddToClassList("btn");
                button.AddToClassList("btn-outline");
                button.SetEnabled(!session.IsFull);
                container.Add(button);
                _sessionButtons.Add(button);
            }
        }

        private static string FormatSession(SessionSummary session)
        {
            return session.IsFull
                ? $"{session.RoomCode}   {session.PlayerCount}/{session.MaxPlayers}   FULL"
                : $"{session.RoomCode}   {session.PlayerCount}/{session.MaxPlayers}";
        }
    }
}
