using System.Collections.Generic;
using Core.Data.Domain.Models.Lobby;
using Core.MVVM.ViewModels.Matchmaking;
using Core.UI.Controls;
using R3;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Core.MVVM.Views.Matchmaking
{
    public class MatchmakingView : PanelView<MatchmakingViewModel>
    {
        [Header("Header")]
        [SerializeField] private BaseButton _backButton;

        [Header("Profile")]
        [SerializeField] private BaseInputField _nicknameInput;

        [Header("Create")]
        [SerializeField] private BaseButton _createButton;

        [Header("Join")]
        [SerializeField] private BaseInputField _codeInput;
        [SerializeField] private BaseButton _joinButton;
        [SerializeField] private BaseButton _refreshButton;

        [Header("Status")]
        [SerializeField] private BaseText _statusText;

        [Header("Sessions")]
        [SerializeField] private RectTransform _sessionsContent;
        [SerializeField] private GameObject _sessionRowTemplate;

        private readonly List<GameObject> _sessionRows = new();

        private void Start()
        {
            if (_sessionRowTemplate != null)
                _sessionRowTemplate.SetActive(false);
        }

        protected override void BindPanel(ref DisposableBag disposables)
        {
            Bind(_backButton, ViewModel.BackCommand, ref disposables);
            Bind(_createButton, ViewModel.CreateCommand, ref disposables);
            Bind(_joinButton, ViewModel.JoinCommand, ref disposables);
            Bind(_refreshButton, ViewModel.RefreshCommand, ref disposables);

            Bind(_nicknameInput, ViewModel.Nickname, ref disposables);
            Bind(_codeInput, ViewModel.RoomCodeInput, ref disposables);

            Bind(_statusText, ViewModel.StatusText, ref disposables);

            BindSessions(ref disposables);
        }

        private void BindSessions(ref DisposableBag disposables)
        {
            RebuildSessions();

            ViewModel.SessionsUpdated
                .Subscribe(this, static (_, self) => self.RebuildSessions())
                .AddTo(ref disposables);
        }

        private void RebuildSessions()
        {
            if (_sessionsContent == null || _sessionRowTemplate == null) return;

            var sessions = ViewModel.Sessions;

            for (var i = _sessionRows.Count - 1; i >= sessions.Count; i--)
            {
                Destroy(_sessionRows[i]);
                _sessionRows.RemoveAt(i);
            }

            for (var i = _sessionRows.Count; i < sessions.Count; i++)
            {
                _sessionRows.Add(CreateRow(i));
            }

            for (var i = 0; i < sessions.Count; i++)
            {
                var row = _sessionRows[i];
                row.SetActive(true);

                var label = row.GetComponentInChildren<TMP_Text>(true);
                if (label != null)
                    label.SetText(FormatSession(sessions[i]));

                var button = row.GetComponentInChildren<Button>(true);
                if (button != null)
                    button.interactable = !sessions[i].IsFull;
            }
        }

        /// <summary>
        /// Кнопка строки подписывается один раз и держится за номер строки, а не за код комнаты.
        /// </summary>
        /// <remarks>
        /// Строки переиспользуются между обновлениями списка, поэтому обработчик, замкнутый на код,
        /// пришлось бы переподключать — а снять его можно только вместе с обработчиком самого
        /// <see cref="BaseButton"/>, который тот вешает в OnEnable и больше не восстанавливает.
        /// </remarks>
        private GameObject CreateRow(int index)
        {
            var row = Instantiate(_sessionRowTemplate, _sessionsContent);
            var button = row.GetComponentInChildren<Button>(true);

            if (button != null)
                button.onClick.AddListener(() => JoinRow(index));

            return row;
        }

        private void JoinRow(int index)
        {
            var sessions = ViewModel.Sessions;

            if (index < 0 || index >= sessions.Count)
            {
                return;
            }

            ViewModel.JoinSession(sessions[index].RoomCode);
        }

        private static string FormatSession(SessionSummary session)
        {
            return session.IsFull
                ? $"{session.RoomCode}   {session.PlayerCount}/{session.MaxPlayers}   FULL"
                : $"{session.RoomCode}   {session.PlayerCount}/{session.MaxPlayers}";
        }
    }
}
