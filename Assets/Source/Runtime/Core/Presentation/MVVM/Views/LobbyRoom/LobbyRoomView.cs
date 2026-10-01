using Core.Data.Domain.Models.Lobby;
using Core.MVVM.ViewModels.LobbyRoom;
using Core.UI.Controls;
using R3;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Core.MVVM.Views.LobbyRoom
{
    public class LobbyRoomView : PanelView<LobbyRoomViewModel>
    {
        private const string HostRole = "HOST";
        private const string PlayerRole = "PLAYER";

        [Header("Room Code")]
        [SerializeField] private BaseText _roomCodeText;
        [SerializeField] private BaseButton _copyButton;

        [Header("Roster Slots")]
        [SerializeField] private GameObject[] _slotRoots;
        [SerializeField] private Image[] _slotAvatars;
        [SerializeField] private TMP_Text[] _slotNames;
        [SerializeField] private TMP_Text[] _slotRoles;
        [SerializeField] private GameObject[] _slotReadyBadges;

        [Header("Footer")]
        [SerializeField] private BaseToggle _readyToggle;
        [SerializeField] private BaseButton _addBotButton;
        [SerializeField] private BaseButton _startButton;
        [SerializeField] private BaseText _startButtonLabel;
        [SerializeField] private BaseButton _leaveButton;

        protected override void BindPanel(ref DisposableBag disposables)
        {
            BindRoomCode(ref disposables);
            BindFooter(ref disposables);
            BindRoster(ref disposables);
        }

        private void BindRoomCode(ref DisposableBag disposables)
        {
            Bind(_roomCodeText, ViewModel.RoomCode, ref disposables);
            Bind(_copyButton, ViewModel.CopyCodeCommand, ref disposables);
        }

        private void BindFooter(ref DisposableBag disposables)
        {
            BindReadyToggle(ref disposables);

            Bind(_addBotButton, ViewModel.AddBotCommand, ref disposables);
            BindActive(_addBotButton, ViewModel.IsHost, ref disposables);

            Bind(_startButton, ViewModel.StartCommand, ref disposables);
            BindInteractable(_startButton, ViewModel.CanStart, ref disposables);
            BindActive(_startButton, ViewModel.IsHost, ref disposables);

            Bind(_startButtonLabel, ViewModel.StartButtonLabel, ref disposables);
            Bind(_leaveButton, ViewModel.LeaveCommand, ref disposables);
        }

        private void BindReadyToggle(ref DisposableBag disposables)
        {
            if (_readyToggle == null) return;

            ViewModel.IsLocalReady
                .Subscribe(_readyToggle, static (isReady, toggle) => toggle.SetIsOnWithoutNotify(isReady))
                .AddTo(ref disposables);

            _readyToggle.onValueChanged.AsObservable()
                .Where(this, static (isOn, self) => isOn != self.ViewModel.IsLocalReady.CurrentValue)
                .Subscribe(ViewModel.ToggleReadyCommand, static (_, cmd) => cmd.Execute(Unit.Default))
                .AddTo(ref disposables);
        }

        private void BindRoster(ref DisposableBag disposables)
        {
            ViewModel.RosterChanged
                .Subscribe(this, static (_, self) => self.RefreshSlots())
                .AddTo(ref disposables);

            RefreshSlots();
        }

        private void RefreshSlots()
        {
            if (_slotRoots == null) return;

            var roster = ViewModel.Roster;

            for (var i = 0; i < _slotRoots.Length; i++)
            {
                var info = roster != null && i < roster.Count ? roster[i] : LobbyPlayerInfo.Empty;

                ApplyActive(_slotRoots, i, info.IsOccupied);

                if (!info.IsOccupied) continue;

                ApplyColor(_slotAvatars, i, PlayerPalette.Resolve(info.ColorIndex));
                ApplyText(_slotNames, i, info.Nickname);
                ApplyText(_slotRoles, i, info.IsHost ? HostRole : PlayerRole);
                ApplyActive(_slotReadyBadges, i, info.IsReady);
            }
        }

        private static void ApplyActive(GameObject[] targets, int index, bool isActive)
        {
            if (targets == null || index >= targets.Length || targets[index] == null) return;
            targets[index].SetActive(isActive);
        }

        private static void ApplyText(TMP_Text[] targets, int index, string value)
        {
            if (targets == null || index >= targets.Length || targets[index] == null) return;
            targets[index].text = value;
        }

        private static void ApplyColor(Image[] targets, int index, Color color)
        {
            if (targets == null || index >= targets.Length || targets[index] == null) return;
            targets[index].color = color;
        }
    }
}
